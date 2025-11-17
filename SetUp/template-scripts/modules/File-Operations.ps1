# File-Operations.ps1
# Handles all file system operations for template copying

function Get-ProjectInformation {
    param(
        [switch]$NonInteractive,
        [string]$ProjectName,
        [string]$TargetPath
    )

    $templatePath = (Get-Item -Path $PSScriptRoot).Parent.Parent.Parent.FullName
    # Remove any quotes that might be in the path
    $templatePath = $templatePath.Trim('"').Trim("'")

    if ($NonInteractive) {
        if (-not $ProjectName) {
            throw "ProjectName is required in non-interactive mode"
        }
        if (-not $TargetPath) {
            $TargetPath = Join-Path (Split-Path $templatePath -Parent) $ProjectName
        }
    } else {
        Write-Host "Please provide information for your new project:" -ForegroundColor Cyan
        Write-Host ""

        # Get project name
        $ProjectName = Get-UserInput -Prompt "Project Name" -Validator {
            param($name)
            if ($name -notmatch '^[A-Za-z][A-Za-z0-9_]*$') {
                return "Project name must start with a letter and contain only letters, numbers, and underscores"
            }
            return $true
        }

        # Get target path
        $defaultTarget = Join-Path (Split-Path $templatePath -Parent) $ProjectName
        $TargetPath = Get-UserInput -Prompt "Target Directory" -Default $defaultTarget -Validator {
            param($path)
            if (Test-Path $path) {
                return "Directory already exists. Please choose a different location."
            }
            $parent = Split-Path $path -Parent
            if (-not (Test-Path $parent)) {
                return "Parent directory does not exist: $parent"
            }
            return $true
        }
    }

    # Get database name
    if (-not $NonInteractive) {
        $defaultDbName = "${ProjectName}Db"
        $databaseName = Get-UserInput -Prompt "Database Name" -Default $defaultDbName
    } else {
        $databaseName = "${ProjectName}Db"
    }

    # Build connection string
    $connectionString = "Server=(localdb)\mssqllocaldb;Database=${databaseName};Trusted_Connection=true;MultipleActiveResultSets=true;TrustServerCertificate=true"

    # Folders to include in copy (only these top-level folders will be copied)
    $includeFolders = @(
        'Backend',
        'Frontend'
    )

    # Files and folders to exclude from copy (applied after include filter)
    $exclusions = @(
        '.git',
        '.vs',
        'bin',
        'obj',
        'node_modules',
        'build',
        'dist',
        '.vscode',
        'TestResults',
        '*.user',
        '*.suo',
        '*.log'
    )

    return @{
        OldName = "WebTemplate"
        NewName = $ProjectName
        TemplatePath = $templatePath
        TargetPath = $TargetPath
        DatabaseName = $databaseName
        ConnectionString = $connectionString
        IncludeFolders = $includeFolders
        Exclusions = $exclusions
    }
}

function Test-TemplateStructure {
    param([string]$TemplatePath)

    Write-Info "Validating template structure..."
    Write-Info "Template path: $TemplatePath"

    $requiredPaths = @(
        "Backend\WebTemplate.API",
        "Backend\WebTemplate.Core",
        "Backend\WebTemplate.Data",
        "Frontend\WebTemplate-frontend"
    )

    foreach ($path in $requiredPaths) {
        $fullPath = Join-Path -Path $TemplatePath -ChildPath $path
        Write-Info "Checking: $fullPath"
        if (-not (Test-Path -Path $fullPath)) {
            throw "Template validation failed: Missing required path '$path' (full path: $fullPath)"
        }
    }

    Write-Success "Template structure validated"
}

function Copy-TemplateFiles {
    param(
        [string]$TemplatePath,
        [string]$TargetPath,
        [array]$IncludeFolders,
        [array]$Exclusions
    )

    Write-Info "Copying template files to $TargetPath..."
    Write-Info "Including folders: $($IncludeFolders -join ', ')"

    # Clean paths - remove any quotes
    $TemplatePath = $TemplatePath.Trim('"').Trim("'")
    $TargetPath = $TargetPath.Trim('"').Trim("'")

    # Create target directory
    New-Item -ItemType Directory -Path $TargetPath -Force | Out-Null

    # Build exclusion patterns for robocopy
    # Directories: items that don't have file extensions (no dot followed by 2-5 chars at end)
    # or explicitly known directory patterns
    $excludeDirs = $Exclusions | Where-Object {
        $_ -notmatch '\.[a-zA-Z0-9]{1,5}$' -or $_ -match '^\..*'
    } | ForEach-Object { $_ -replace '/', '\' }

    $excludeFiles = $Exclusions | Where-Object {
        $_ -match '\.[a-zA-Z0-9]{1,5}$' -and $_ -notmatch '^\..*'
    }

    Write-Info "Excluding directories: $($excludeDirs -join ', ')"
    Write-Info "Excluding files: $($excludeFiles -join ', ')"

    # Copy each included folder separately
    foreach ($folder in $IncludeFolders) {
        $sourcePath = Join-Path $TemplatePath $folder
        $destPath = Join-Path $TargetPath $folder

        if (-not (Test-Path $sourcePath)) {
            Write-Warning "Included folder not found: $sourcePath"
            continue
        }

        Write-Info "Copying folder: $folder"

        # Use robocopy for efficient copying
        $robocopyArgs = @(
            $sourcePath,
            $destPath,
            '/E',           # Copy subdirectories including empty
            '/NFL',         # No file list
            '/NDL',         # No directory list
            '/NJH',         # No job header
            '/NJS',         # No job summary
            '/NC',          # No class
            '/NS',          # No size
            '/NP',          # No progress
            '/R:0',         # Number of retries on failed copies (0 = no retries)
            '/W:0'          # Wait time between retries (0 seconds)
        )

        if ($excludeDirs) {
            $robocopyArgs += '/XD'
            $robocopyArgs += $excludeDirs
        }

        if ($excludeFiles) {
            $robocopyArgs += '/XF'
            $robocopyArgs += $excludeFiles
        }

        # Execute robocopy with proper argument expansion
        try {
            $robocopyOutput = & robocopy @robocopyArgs 2>&1
            $exitCode = $LASTEXITCODE
        }
        catch {
            Write-Warning "Robocopy execution failed for ${folder}: $_"
            throw "Failed to execute robocopy for ${folder}: $_"
        }

        # Robocopy exit codes: 0-7 are success, 8+ are errors
        if ($exitCode -ge 8) {
            Write-Warning "Robocopy output: $robocopyOutput"
            throw "Failed to copy folder ${folder}. Robocopy exit code: ${exitCode}"
        }

        Write-Success "Copied $folder successfully"
    }

    Write-Success "All template files copied successfully"
}

function Get-FilesToRebrand {
    param([string]$TargetPath)

    # Clean path - remove any quotes
    $TargetPath = $TargetPath.Trim('"').Trim("'")

    $extensions = @('*.cs', '*.csproj', '*.sln', '*.json', '*.md', '*.ps1', '*.ts', '*.tsx', '*.html', '*.yml', '*.yaml')

    $files = @()
    foreach ($ext in $extensions) {
        $files += Get-ChildItem -Path $TargetPath -Filter $ext -Recurse -File
    }

    return $files
}

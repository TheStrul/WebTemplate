# File-Operations.ps1
# Handles all file system operations for template copying

function Get-ProjectInformation {
    param(
        [switch]$NonInteractive,
        [string]$ProjectName,
        [string]$TargetPath
    )

    $templatePath = (Get-Item $PSScriptRoot).Parent.Parent.FullName

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

    # Files and folders to exclude from copy
    $exclusions = @(
        '.git',
        '.vs',
        'bin',
        'obj',
        'node_modules',
        'build',
        'dist',
        '.vscode',
        'template-scripts',
        'MIGRATION-REMOVAL-SUMMARY.md',
        'VALIDATION-QUICK-START.md',
        'Backend/WebTemplate.Data.Validator/IMPLEMENTATION-SUMMARY.md',
        '*.user',
        '*.suo',
        'package-lock.json'
    )

    return @{
        OldName = "WebTemplate"
        NewName = $ProjectName
        TemplatePath = $templatePath
        TargetPath = $TargetPath
        DatabaseName = $databaseName
        ConnectionString = $connectionString
        Exclusions = $exclusions
    }
}

function Test-TemplateStructure {
    param([string]$TemplatePath)

    Write-Info "Validating template structure..."

    $requiredPaths = @(
        "Backend/WebTemplate.API",
        "Backend/WebTemplate.Core",
        "Backend/WebTemplate.Data",
        "Frontend/webtemplate-frontend",
        "scripts"
    )

    foreach ($path in $requiredPaths) {
        $fullPath = Join-Path $TemplatePath $path
        if (-not (Test-Path $fullPath)) {
            throw "Template validation failed: Missing required path '$path'"
        }
    }

    Write-Success "Template structure validated"
}

function Copy-TemplateFiles {
    param(
        [string]$TemplatePath,
        [string]$TargetPath,
        [array]$Exclusions
    )

    Write-Info "Copying template files to $TargetPath..."

    # Create target directory
    New-Item -ItemType Directory -Path $TargetPath -Force | Out-Null

    # Build exclusion patterns for robocopy
    $excludeDirs = $Exclusions | Where-Object { $_ -notlike '*.*' } | ForEach-Object { $_ -replace '/', '\' }
    $excludeFiles = $Exclusions | Where-Object { $_ -like '*.*' }

    # Use robocopy for efficient copying
    $robocopyArgs = @(
        $TemplatePath,
        $TargetPath,
        '/E',           # Copy subdirectories including empty
        '/NFL',         # No file list
        '/NDL',         # No directory list
        '/NJH',         # No job header
        '/NJS',         # No job summary
        '/NC',          # No class
        '/NS',          # No size
        '/NP'           # No progress
    )

    if ($excludeDirs) {
        $robocopyArgs += '/XD'
        $robocopyArgs += $excludeDirs
    }

    if ($excludeFiles) {
        $robocopyArgs += '/XF'
        $robocopyArgs += $excludeFiles
    }

    $null = & robocopy $robocopyArgs 2>&1

    # Robocopy exit codes: 0-7 are success, 8+ are errors
    if ($LASTEXITCODE -ge 8) {
        throw "Failed to copy template files. Robocopy exit code: $LASTEXITCODE"
    }

    Write-Success "Template files copied successfully"
}

function Get-FilesToRebrand {
    param([string]$TargetPath)

    $extensions = @('*.cs', '*.csproj', '*.sln', '*.json', '*.md', '*.ps1', '*.ts', '*.tsx', '*.html', '*.yml', '*.yaml')

    $files = @()
    foreach ($ext in $extensions) {
        $files += Get-ChildItem -Path $TargetPath -Filter $ext -Recurse -File
    }

    return $files
}

# Validation.ps1
# Validates the newly created project

function Test-NewProject {
    param(
        [string]$TargetPath,
        [string]$ProjectName
    )

    Write-Info "Validating new project..."

    $validationResults = @{
        StructureValid = $false
        SolutionValid = $false
        BuildValid = $false
        ConfigValid = $false
    }

    # Test 1: Directory structure
    Write-Info "Checking directory structure..."
    $validationResults.StructureValid = Test-ProjectStructure -TargetPath $TargetPath -ProjectName $ProjectName

    # Test 2: Solution file
    Write-Info "Checking solution file..."
    $validationResults.SolutionValid = Test-SolutionFile -TargetPath $TargetPath -ProjectName $ProjectName

    # Test 3: Build
    Write-Info "Testing build..."
    $validationResults.BuildValid = Test-ProjectBuild -TargetPath $TargetPath -ProjectName $ProjectName

    # Test 4: Configuration files
    Write-Info "Checking configuration files..."
    $validationResults.ConfigValid = Test-ConfigurationFiles -TargetPath $TargetPath -ProjectName $ProjectName

    # Summary
    Write-Host ""
    Write-Host "Validation Results:" -ForegroundColor Cyan
    foreach ($key in $validationResults.Keys) {
        $status = if ($validationResults[$key]) { "✓ PASS" } else { "✗ FAIL" }
        $color = if ($validationResults[$key]) { "Green" } else { "Red" }
        Write-Host "  $key : $status" -ForegroundColor $color
    }

    $allValid = $validationResults.Values | Where-Object { $_ -eq $false } | Measure-Object | Select-Object -ExpandProperty Count

    if ($allValid -eq 0) {
        Write-Success "All validations passed!"
        return $true
    } else {
        Write-Warning "Some validations failed. Please review the project manually."
        return $false
    }
}

function Test-ProjectStructure {
    param(
        [string]$TargetPath,
        [string]$ProjectName
    )

    $requiredPaths = @(
        "Backend/$ProjectName.API",
        "Backend/$ProjectName.Core",
        "Backend/$ProjectName.Data",
        "Backend/$ProjectName.UserModule",
        "Backend/$ProjectName.Data.Validator",
        "Frontend/$($ProjectName.ToLower())-frontend",
        "scripts"
    )

    foreach ($path in $requiredPaths) {
        $fullPath = Join-Path $TargetPath $path
        if (-not (Test-Path $fullPath)) {
            Write-Warning "Missing: $path"
            return $false
        }
    }

    return $true
}

function Test-SolutionFile {
    param(
        [string]$TargetPath,
        [string]$ProjectName
    )

    $slnPath = Join-Path $TargetPath "$ProjectName.sln"

    if (-not (Test-Path $slnPath)) {
        Write-Warning "Solution file not found: $slnPath"
        return $false
    }

    $content = Get-Content $slnPath -Raw

    # Check that old project name is not present
    if ($content -match 'WebTemplate\.(?!sln)') {
        Write-Warning "Solution file still contains 'WebTemplate' references"
        return $false
    }

    return $true
}

function Test-ProjectBuild {
    param(
        [string]$TargetPath,
        [string]$ProjectName
    )

    Push-Location $TargetPath
    try {
        Write-Info "Restoring NuGet packages..."
        $null = dotnet restore "$ProjectName.sln" -v q 2>&1

        if ($LASTEXITCODE -ne 0) {
            Write-Warning "Package restore failed"
            return $false
        }

        Write-Info "Building solution..."
        $null = dotnet build "$ProjectName.sln" --no-restore -v q 2>&1

        if ($LASTEXITCODE -eq 0) {
            return $true
        } else {
            Write-Warning "Build failed. Check for compilation errors."
            return $false
        }
    }
    catch {
        Write-Warning "Build test failed: $_"
        return $false
    }
    finally {
        Pop-Location
    }
}

function Test-ConfigurationFiles {
    param(
        [string]$TargetPath,
        [string]$ProjectName
    )

    $configFiles = @(
        "Backend/$ProjectName.API/appsettings.json",
        "Backend/$ProjectName.API/appsettings.Development.json",
        "Frontend/$($ProjectName.ToLower())-frontend/package.json"
    )

    foreach ($file in $configFiles) {
        $fullPath = Join-Path $TargetPath $file
        if (-not (Test-Path $fullPath)) {
            Write-Warning "Missing configuration file: $file"
            return $false
        }

        $content = Get-Content $fullPath -Raw

        # Check for unreplaced template names
        if ($content -match 'WebTemplate(?!\.sln)' -and $content -notmatch $ProjectName) {
            Write-Warning "Configuration file contains unreplaced template names: $file"
            return $false
        }
    }

    return $true
}

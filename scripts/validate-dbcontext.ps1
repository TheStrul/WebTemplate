#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Validates that ApplicationDbContext configuration matches 100% with entity definitions
.DESCRIPTION
    This script builds and runs the DbContext validator tool to ensure there's no
    configuration drift between Data Annotations and Fluent API in ApplicationDbContext.
.EXAMPLE
    .\validate-dbcontext.ps1
.NOTES
    Returns exit code 0 if validation passes, non-zero if issues found
#>

param(
    [switch]$NoBuild,
    [switch]$Verbose
)

$ErrorActionPreference = "Stop"

Write-Host "=" -NoNewline -ForegroundColor Cyan
Write-Host ("=" * 79) -ForegroundColor Cyan
Write-Host "WebTemplate - DbContext Validation Tool" -ForegroundColor Cyan
Write-Host "=" -NoNewline -ForegroundColor Cyan
Write-Host ("=" * 79) -ForegroundColor Cyan
Write-Host ""

# Get script directory and project root
$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$ProjectRoot = Split-Path -Parent $ScriptDir
$ProjectPath = Join-Path $ProjectRoot "Backend\WebTemplate.Data.Validator\WebTemplate.Data.Validator.csproj"
$WorkingDir = Join-Path $ProjectRoot "Backend\WebTemplate.Data.Validator"

# Build the validator project if requested
if (-not $NoBuild) {
    Write-Host "Building validator project..." -ForegroundColor Yellow
    $buildArgs = @("build", $ProjectPath, "-c", "Release")
    if (-not $Verbose) {
        $buildArgs += "--nologo"
        $buildArgs += "-v"
        $buildArgs += "minimal"
    }

    & dotnet $buildArgs

    if ($LASTEXITCODE -ne 0) {
        Write-Host "Build failed!" -ForegroundColor Red
        exit $LASTEXITCODE
    }
    Write-Host "Build successful!" -ForegroundColor Green
    Write-Host ""
}

# Run the validator
Write-Host "Running validation..." -ForegroundColor Yellow
Write-Host ""

Push-Location $WorkingDir
try {
    & dotnet run --no-build -c Release
    $exitCode = $LASTEXITCODE

    Write-Host ""
    if ($exitCode -eq 0) {
        Write-Host "✓ Validation PASSED" -ForegroundColor Green
    } elseif ($exitCode -eq 1) {
        Write-Host "✗ Validation FAILED - Configuration issues found" -ForegroundColor Red
    } else {
        Write-Host "✗ Validation ERROR - Tool execution failed" -ForegroundColor Red
    }

    exit $exitCode
}
finally {
    Pop-Location
}

#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Creates a new project from the WebTemplate template with full configuration and rebranding
.DESCRIPTION
    Interactive script that:
    1. Collects project information
    2. Copies template files
    3. Rebrands namespaces and project names
    4. Updates configurations
    5. Initializes git repository
    6. Validates the new project
.EXAMPLE
    .\New-ProjectFromTemplate.ps1
.NOTES
    This script should be run from the template workspace root directory
#>

[CmdletBinding()]
param(
    [Parameter(HelpMessage = "Skip interactive prompts and use defaults")]
    [switch]$NonInteractive,

    [Parameter(HelpMessage = "Project name (if non-interactive)")]
    [string]$ProjectName,

    [Parameter(HelpMessage = "Target directory (if non-interactive)")]
    [string]$TargetPath,

    [Parameter(HelpMessage = "Skip git initialization")]
    [switch]$SkipGit,

    [Parameter(HelpMessage = "Skip final validation")]
    [switch]$SkipValidation
)

$ErrorActionPreference = "Stop"
$ProgressPreference = "SilentlyContinue"

# Get script directory and load modules
$ScriptRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$ModulesPath = Join-Path $ScriptRoot "modules"

# Import all module functions
. (Join-Path $ModulesPath "UI-Helpers.ps1")
. (Join-Path $ModulesPath "File-Operations.ps1")
. (Join-Path $ModulesPath "Rebranding.ps1")
. (Join-Path $ModulesPath "Configuration.ps1")
. (Join-Path $ModulesPath "Git-Operations.ps1")
. (Join-Path $ModulesPath "Validation.ps1")

# Main script execution
try {
    Show-Banner

    Write-Step "Step 1/7: Collecting Project Information"
    $projectInfo = Get-ProjectInformation -NonInteractive:$NonInteractive -ProjectName $ProjectName -TargetPath $TargetPath

    Write-Step "Step 2/7: Validating Template"
    Test-TemplateStructure -TemplatePath $projectInfo.TemplatePath

    Write-Step "Step 3/7: Copying Template Files"
    Copy-TemplateFiles -TemplatePath $projectInfo.TemplatePath -TargetPath $projectInfo.TargetPath -Exclusions $projectInfo.Exclusions

    Write-Step "Step 4/7: Rebranding Project"
    Invoke-ProjectRebranding -TargetPath $projectInfo.TargetPath -OldName $projectInfo.OldName -NewName $projectInfo.NewName

    Write-Step "Step 5/7: Updating Configurations"
    Update-ProjectConfigurations -TargetPath $projectInfo.TargetPath -ProjectInfo $projectInfo

    if (-not $SkipGit) {
        Write-Step "Step 6/7: Initializing Git Repository"
        Initialize-GitRepository -TargetPath $projectInfo.TargetPath -ProjectName $projectInfo.NewName
    } else {
        Write-Step "Step 6/7: Skipping Git Initialization"
    }

    if (-not $SkipValidation) {
        Write-Step "Step 7/7: Validating New Project"
        $null = Test-NewProject -TargetPath $projectInfo.TargetPath -ProjectName $projectInfo.NewName
    } else {
        Write-Step "Step 7/7: Skipping Validation"
    }

    Show-CompletionSummary -ProjectInfo $projectInfo

    exit 0
}
catch {
    Write-Error "Fatal error: $_"
    Write-Host ""
    Write-Host "Stack trace:" -ForegroundColor Red
    Write-Host $_.ScriptStackTrace -ForegroundColor Red
    exit 1
}

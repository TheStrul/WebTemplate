# UI-Helpers.ps1
# Provides user interface functions for the template script

function Show-Banner {
    Write-Host ""
    Write-Host "╔═══════════════════════════════════════════════════════════════════════════╗" -ForegroundColor Cyan
    Write-Host "║                                                                           ║" -ForegroundColor Cyan
    Write-Host "║            WebTemplate Template - New Project Generator                  ║" -ForegroundColor Cyan
    Write-Host "║                                                                           ║" -ForegroundColor Cyan
    Write-Host "╚═══════════════════════════════════════════════════════════════════════════╝" -ForegroundColor Cyan
    Write-Host ""
}

function Write-Step {
    param([string]$Message)
    Write-Host ""
    Write-Host "═══════════════════════════════════════════════════════════════════════════" -ForegroundColor Yellow
    Write-Host "  $Message" -ForegroundColor Yellow
    Write-Host "═══════════════════════════════════════════════════════════════════════════" -ForegroundColor Yellow
    Write-Host ""
}

function Write-Success {
    param([string]$Message)
    Write-Host "✓ $Message" -ForegroundColor Green
}

function Write-Info {
    param([string]$Message)
    Write-Host "ℹ $Message" -ForegroundColor Cyan
}

function Write-Warning {
    param([string]$Message)
    Write-Host "⚠ $Message" -ForegroundColor Yellow
}

function Write-Progress {
    param(
        [string]$Activity,
        [string]$Status,
        [int]$PercentComplete
    )
    Write-Host "  [$PercentComplete%] $Activity - $Status" -ForegroundColor Gray
}

function Show-CompletionSummary {
    param($ProjectInfo)

    Write-Host ""
    Write-Host "╔═══════════════════════════════════════════════════════════════════════════╗" -ForegroundColor Green
    Write-Host "║                                                                           ║" -ForegroundColor Green
    Write-Host "║                     ✓ PROJECT CREATED SUCCESSFULLY                        ║" -ForegroundColor Green
    Write-Host "║                                                                           ║" -ForegroundColor Green
    Write-Host "╚═══════════════════════════════════════════════════════════════════════════╝" -ForegroundColor Green
    Write-Host ""

    Write-Host "Project Details:" -ForegroundColor Cyan
    Write-Host "  Name:              $($ProjectInfo.NewName)" -ForegroundColor White
    Write-Host "  Location:          $($ProjectInfo.TargetPath)" -ForegroundColor White
    Write-Host "  Database:          $($ProjectInfo.DatabaseName)" -ForegroundColor White
    Write-Host "  Connection String: $($ProjectInfo.ConnectionString)" -ForegroundColor White
    Write-Host ""

    Write-Host "Next Steps:" -ForegroundColor Cyan
    Write-Host "  1. Navigate to project:" -ForegroundColor White
    Write-Host "     cd `"$($ProjectInfo.TargetPath)`"" -ForegroundColor Gray
    Write-Host ""
    Write-Host "  2. Review configurations:" -ForegroundColor White
    Write-Host "     - Backend/WebTemplate.API/appsettings.json" -ForegroundColor Gray
    Write-Host "     - .github/copilot-instructions.md" -ForegroundColor Gray
    Write-Host ""
    Write-Host "  3. Initialize database:" -ForegroundColor White
    Write-Host "     - Run: Backend/WebTemplate.Data/Migrations/db-init.sql" -ForegroundColor Gray
    Write-Host ""
    Write-Host "  4. Build and run:" -ForegroundColor White
    Write-Host "     dotnet build" -ForegroundColor Gray
    Write-Host "     dotnet run --project Backend/$($ProjectInfo.NewName).API/$($ProjectInfo.NewName).API.csproj" -ForegroundColor Gray
    Write-Host ""
    Write-Host "  5. Validate configuration:" -ForegroundColor White
    Write-Host "     pwsh scripts/validate-dbcontext.ps1" -ForegroundColor Gray
    Write-Host ""
}

function Get-UserInput {
    param(
        [string]$Prompt,
        [string]$Default,
        [scriptblock]$Validator = $null
    )

    do {
        if ($Default) {
            $userInput = Read-Host "$Prompt [$Default]"
            if ([string]::IsNullOrWhiteSpace($userInput)) {
                $userInput = $Default
            }
        } else {
            $userInput = Read-Host $Prompt
        }

        if ($Validator) {
            $validationResult = & $Validator $userInput
            if ($validationResult -ne $true) {
                Write-Warning $validationResult
                $userInput = $null
            }
        }
    } while ([string]::IsNullOrWhiteSpace($userInput))

    return $userInput
}

function Confirm-Action {
    param(
        [string]$Message,
        [bool]$DefaultYes = $true
    )

    $choices = '&Yes', '&No'
    $default = if ($DefaultYes) { 0 } else { 1 }

    $decision = $Host.UI.PromptForChoice('Confirmation', $Message, $choices, $default)
    return ($decision -eq 0)
}

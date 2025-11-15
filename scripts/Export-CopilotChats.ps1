#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Automated GitHub Copilot chat export script
    Runs ChatExporter and handles git operations

.DESCRIPTION
    This script:
    1. Runs the ChatExporter tool
    2. Checks for new exported files
    3. Creates a git commit with the exports
    4. Updates the session log

.EXAMPLE
    ./Export-CopilotChats.ps1
    ./Export-CopilotChats.ps1 -AutoCommit -Verbose

.NOTES
    Run from WebTemplate repository root
#>

param(
    [switch]$AutoCommit,
    [switch]$Verbose
)

# Colors for output
$colors = @{
    Success = 'Green'
    Error = 'Red'
    Warning = 'Yellow'
    Info = 'Cyan'
}

function Write-Status {
    param([string]$Message, [string]$Status = 'Info')
    $color = $colors[$Status] ?? 'White'
    Write-Host "[$Status] $Message" -ForegroundColor $color
}

# Validate we're in the repo
$repoRoot = Get-Location
$chatExporterPath = Join-Path $repoRoot "Backend\WebTemplate.ChatExporter"

if (-not (Test-Path $chatExporterPath)) {
    Write-Status "ChatExporter not found at $chatExporterPath" "Error"
    exit 1
}

Write-Status "Starting Copilot chat export..." "Info"
Write-Status "Repository: $repoRoot" "Info"
Write-Status "Exporter: $chatExporterPath" "Info"
Write-Host ""

# Get the current state of exports folder
$exportsFolder = Join-Path $repoRoot "SESSIONS\copilot_exports"
$filesBefore = @(Get-ChildItem $exportsFolder -ErrorAction SilentlyContinue)
Write-Status "Files before export: $($filesBefore.Count)" "Info"

# Run the exporter
Write-Host ""
Write-Status "Running ChatExporter..." "Info"
Write-Host ""

try {
    Push-Location $chatExporterPath
    $output = dotnet run 2>&1
    Pop-Location
    
    # Print exporter output
    $output | ForEach-Object { Write-Host $_ }
}
catch {
    Write-Status "ChatExporter failed: $_" "Error"
    exit 1
}

Write-Host ""

# Check for new files
$filesAfter = @(Get-ChildItem $exportsFolder -ErrorAction SilentlyContinue)
$newFiles = @($filesAfter | Where-Object { $filesBefore -notcontains $_ })

Write-Status "Files after export: $($filesAfter.Count)" "Info"
Write-Status "New files created: $($newFiles.Count)" "Info"

if ($newFiles.Count -gt 0) {
    Write-Host ""
    Write-Status "New exports:" "Success"
    $newFiles | ForEach-Object {
        Write-Host "  ✓ $($_.Name)" -ForegroundColor Green
    }
}

# Optional: Auto-commit to git
if ($AutoCommit) {
    Write-Host ""
    Write-Status "Committing to git..." "Info"
    
    try {
        Push-Location $repoRoot
        
        # Stage the exports
        git add "SESSIONS/copilot_exports/*"
        
        # Create commit message
        $timestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
        $fileCount = $newFiles.Count
        $commitMessage = "chore: export Copilot chat history ($fileCount file(s)) - $timestamp"
        
        # Commit
        git commit -m $commitMessage
        
        Pop-Location
        
        Write-Status "Committed to git: $commitMessage" "Success"
    }
    catch {
        Write-Status "Git commit failed: $_" "Warning"
    }
}

Write-Host ""
Write-Status "Export complete!" "Success"
Write-Status "Exports saved to: $exportsFolder" "Info"

if (-not $AutoCommit) {
    Write-Host ""
    Write-Host "Tip: Run with -AutoCommit to automatically commit exports to git:" -ForegroundColor Gray
    Write-Host "  ./Export-CopilotChats.ps1 -AutoCommit" -ForegroundColor Gray
}

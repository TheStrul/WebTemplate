# Git-Operations.ps1
# Handles Git repository initialization

function Initialize-GitRepository {
    param(
        [string]$TargetPath,
        [string]$ProjectName,
        [switch]$NonInteractive
    )

    Write-Info "Initializing Git repository..."

    # Clean path - remove any quotes
    $TargetPath = $TargetPath.Trim('"').Trim("'")

    Push-Location $TargetPath
    try {
        # Initialize git
        git init 2>&1 | Out-Null

        # Create .gitignore if it doesn't exist
        $gitignorePath = Join-Path $TargetPath ".gitignore"
        if (-not (Test-Path $gitignorePath)) {
            New-GitIgnoreFile -Path $gitignorePath
        }

        # Initial commit
        git add . 2>&1 | Out-Null
        git commit -m "Initial commit: $ProjectName created from WebTemplate template" 2>&1 | Out-Null

        Write-Success "Git repository initialized with initial commit"

        # Skip remote setup in non-interactive mode
        if (-not $NonInteractive) {
            # Setup remote repository
            Write-Info "Setting up remote repository..."
            $remoteSetup = Setup-GitRemote -ProjectName $ProjectName

            if ($remoteSetup) {
                Write-Success "Remote repository configured"
            } else {
                Write-Warning "Remote repository not configured - you can set it up later with:"
                Write-Host "  git remote add origin <your-repo-url>" -ForegroundColor Yellow
                Write-Host "  git push -u origin main" -ForegroundColor Yellow
            }
        } else {
            Write-Info "Skipping remote repository setup (non-interactive mode)"
        }
    }
    catch {
        Write-Warning "Git initialization failed: $_"
    }
    finally {
        Pop-Location
    }
}

function Setup-GitRemote {
    param(
        [string]$ProjectName
    )

    # Check if gh CLI is available
    $ghAvailable = Get-Command gh -ErrorAction SilentlyContinue

    if ($ghAvailable) {
        Write-Info "GitHub CLI detected. Would you like to create a remote repository?"
        $response = Read-Host "Create GitHub repository? (y/N)"

        if ($response -eq 'y' -or $response -eq 'Y') {
            try {
                $owner = Read-Host "GitHub owner/organization (or press Enter for default)"
                $isPrivate = Read-Host "Make repository private? (Y/n)"

                $privateFlag = if ($isPrivate -eq 'n' -or $isPrivate -eq 'N') { "--public" } else { "--private" }

                if ([string]::IsNullOrWhiteSpace($owner)) {
                    $result = gh repo create $ProjectName $privateFlag --source=. --remote=origin 2>&1
                } else {
                    $result = gh repo create "$owner/$ProjectName" $privateFlag --source=. --remote=origin 2>&1
                }

                if ($LASTEXITCODE -eq 0) {
                    Write-Success "GitHub repository created successfully"

                    # Push to remote
                    $pushResponse = Read-Host "Push to remote now? (Y/n)"
                    if ($pushResponse -ne 'n' -and $pushResponse -ne 'N') {
                        git branch -M main 2>&1 | Out-Null
                        git push -u origin main 2>&1 | Out-Null
                        Write-Success "Pushed to remote repository"
                    }
                    return $true
                } else {
                    Write-Warning "Failed to create GitHub repository: $result"
                    return $false
                }
            }
            catch {
                Write-Warning "Error creating GitHub repository: $_"
                return $false
            }
        }
    } else {
        Write-Info "GitHub CLI (gh) not found. You can:"
        Write-Host "  1. Install gh CLI: https://cli.github.com/" -ForegroundColor Cyan
        Write-Host "  2. Manually create a repo and set remote later" -ForegroundColor Cyan
    }

    return $false
}

function New-GitIgnoreFile {
    param([string]$Path)

    $gitignoreContent = @"
# Build results
[Dd]ebug/
[Rr]elease/
x64/
x86/
[Aa]rm/
[Aa]rm64/
bld/
[Bb]in/
[Oo]bj/
[Ll]og/
[Ll]ogs/

# Visual Studio cache/options
.vs/
*.suo
*.user
*.userosscache
*.sln.docstates

# Visual Studio Code
.vscode/

# ReSharper
_ReSharper*/
*.[Rr]e[Ss]harper
*.DotSettings.user

# User-specific files
*.rsuser
*.suo
*.user
*.userosscache
*.sln.docstates

# .NET Core
project.lock.json
project.fragment.lock.json
artifacts/

# Node.js
node_modules/
npm-debug.log*
yarn-debug.log*
yarn-error.log*
package-lock.json

# Frontend build
dist/
build/
.cache/

# Environment files
.env
.env.local
.env.*.local

# Database
*.db
*.db-shm
*.db-wal

# OS files
.DS_Store
Thumbs.db

# Logs
*.log
logs/

# Sensitive configuration
appsettings.Production.json
**/appsettings.*.json
!**/appsettings.Development.json
!**/appsettings.json
"@

    Set-Content -Path $Path -Value $gitignoreContent
}

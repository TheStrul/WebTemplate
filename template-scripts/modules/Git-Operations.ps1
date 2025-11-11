# Git-Operations.ps1
# Handles Git repository initialization

function Initialize-GitRepository {
    param(
        [string]$TargetPath,
        [string]$ProjectName
    )

    Write-Info "Initializing Git repository..."

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

        Write-Success "Git repository initialized"
    }
    catch {
        Write-Warning "Git initialization failed: $_"
    }
    finally {
        Pop-Location
    }
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

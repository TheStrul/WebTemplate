# Configuration.ps1
# Handles configuration file updates

function Update-ProjectConfigurations {
    param(
        [string]$TargetPath,
        [hashtable]$ProjectInfo
    )

    Write-Info "Updating project configurations..."

    # Update appsettings.json files
    Update-AppSettings -TargetPath $TargetPath -ProjectInfo $ProjectInfo

    # Update copilot instructions
    Update-CopilotInstructions -TargetPath $TargetPath -ProjectInfo $ProjectInfo

    # Update package.json
    Update-PackageJson -TargetPath $TargetPath -ProjectInfo $ProjectInfo

    # Update README files
    Update-ReadmeFiles -TargetPath $TargetPath -ProjectInfo $ProjectInfo

    Write-Success "Configurations updated successfully"
}

function Update-AppSettings {
    param(
        [string]$TargetPath,
        [hashtable]$ProjectInfo
    )

    Write-Info "Updating appsettings files..."

    $appsettingsFiles = Get-ChildItem -Path $TargetPath -Filter "appsettings*.json" -Recurse

    foreach ($file in $appsettingsFiles) {
        try {
            $json = Get-Content $file.FullName -Raw | ConvertFrom-Json

            # Update connection strings
            if ($json.PSObject.Properties['ConnectionStrings']) {
                if ($json.ConnectionStrings.PSObject.Properties['DefaultConnection']) {
                    $json.ConnectionStrings.DefaultConnection = $ProjectInfo.ConnectionString
                }
                if ($json.ConnectionStrings.PSObject.Properties['SqlServerConnection']) {
                    $json.ConnectionStrings.SqlServerConnection = $ProjectInfo.ConnectionString -replace '\(localdb\\mssqllocaldb\)', 'localhost'
                }
            }

            # Update JWT settings
            if ($json.PSObject.Properties['JwtSettings']) {
                if ($json.JwtSettings.PSObject.Properties['Issuer']) {
                    $json.JwtSettings.Issuer = "$($ProjectInfo.NewName).API"
                }
                if ($json.JwtSettings.PSObject.Properties['Audience']) {
                    $json.JwtSettings.Audience = "$($ProjectInfo.NewName).Client"
                }
            }

            # Update admin seed email
            if ($json.PSObject.Properties['Features']) {
                if ($json.Features.PSObject.Properties['AdminSeed']) {
                    if ($json.Features.AdminSeed.PSObject.Properties['Email']) {
                        $json.Features.AdminSeed.Email = "admin@$($ProjectInfo.NewName.ToLower()).com"
                    }
                }
            }

            # Save with proper formatting
            $json | ConvertTo-Json -Depth 10 | Set-Content $file.FullName
        }
        catch {
            Write-Warning "Could not update $($file.Name): $($_.Exception.Message)"
        }
    }

    Write-Success "Updated $($appsettingsFiles.Count) appsettings files"
}

function Update-CopilotInstructions {
    param(
        [string]$TargetPath,
        [hashtable]$ProjectInfo
    )

    $instructionsPath = Join-Path $TargetPath ".github/copilot-instructions.md"

    if (Test-Path $instructionsPath) {
        Write-Info "Updating Copilot instructions..."

        $content = Get-Content $instructionsPath -Raw

        # Update project-specific instructions
        $content = $content -replace 'WebTemplate Template', "$($ProjectInfo.NewName) Project"
        $content = $content -replace 'CoreWebTemplateDb', $ProjectInfo.DatabaseName

        Set-Content -Path $instructionsPath -Value $content -NoNewline

        Write-Success "Updated Copilot instructions"
    }
}

function Update-PackageJson {
    param(
        [string]$TargetPath,
        [hashtable]$ProjectInfo
    )

    $packageJsonPath = Join-Path $TargetPath "Frontend/$($ProjectInfo.NewName.ToLower())-frontend/package.json"

    if (Test-Path $packageJsonPath) {
        Write-Info "Updating package.json..."

        try {
            $json = Get-Content $packageJsonPath -Raw | ConvertFrom-Json

            if ($json.PSObject.Properties['name']) {
                $json.name = "$($ProjectInfo.NewName.ToLower())-frontend"
            }

            if ($json.PSObject.Properties['description']) {
                $json.description = "$($ProjectInfo.NewName) Frontend Application"
            }

            $json | ConvertTo-Json -Depth 10 | Set-Content $packageJsonPath

            Write-Success "Updated package.json"
        }
        catch {
            Write-Warning "Could not update package.json: $($_.Exception.Message)"
        }
    }
}

function Update-ReadmeFiles {
    param(
        [string]$TargetPath,
        [hashtable]$ProjectInfo
    )

    $readmeFiles = Get-ChildItem -Path $TargetPath -Filter "README.md" -Recurse

    foreach ($readme in $readmeFiles) {
        $content = Get-Content $readme.FullName -Raw

        # Add project-specific header
        $header = @"
# $($ProjectInfo.NewName)

> Created from WebTemplate template on $(Get-Date -Format 'yyyy-MM-dd')

---

"@

        if ($content -notlike "*Created from WebTemplate template*") {
            $content = $header + $content
            Set-Content -Path $readme.FullName -Value $content -NoNewline
        }
    }

    Write-Success "Updated README files"
}

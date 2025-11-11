# Rebranding.ps1
# Handles namespace and project name rebranding

function Invoke-ProjectRebranding {
    param(
        [string]$TargetPath,
        [string]$OldName,
        [string]$NewName
    )

    Write-Info "Rebranding project from '$OldName' to '$NewName'..."

    # Step 1: Rename files and directories
    Rename-ProjectStructure -TargetPath $TargetPath -OldName $OldName -NewName $NewName

    # Step 2: Update file contents
    Update-FileContents -TargetPath $TargetPath -OldName $OldName -NewName $NewName

    Write-Success "Project rebranded successfully"
}

function Rename-ProjectStructure {
    param(
        [string]$TargetPath,
        [string]$OldName,
        [string]$NewName
    )

    Write-Info "Renaming directories and files..."

    $renamed = 0
    $continueRenaming = $true

    # Keep renaming until no more items found (handles parent path changes)
    while ($continueRenaming) {
        # Re-query each iteration to get current paths
        $itemsToRename = Get-ChildItem -Path $TargetPath -Recurse -ErrorAction SilentlyContinue |
            Where-Object { $_.Name -like "*$OldName*" } |
            Sort-Object { $_.FullName.Length } -Descending  # Rename deepest items first

        if ($itemsToRename.Count -eq 0) {
            $continueRenaming = $false
            break
        }

        foreach ($item in $itemsToRename) {
            try {
                # Double-check item still exists
                if (-not (Test-Path $item.FullName)) {
                    continue
                }

                $newItemName = $item.Name -replace $OldName, $NewName
                $parentPath = Split-Path $item.FullName -Parent

                if ($null -eq $parentPath -or [string]::IsNullOrWhiteSpace($parentPath)) {
                    continue
                }

                # Use proper path combination
                $newPath = Join-Path $parentPath $newItemName

                if ($item.FullName -ne $newPath -and -not (Test-Path $newPath)) {
                    # Direct call without splatting
                    Rename-Item $item.FullName $newItemName -Force
                    $renamed++
                }
            }
            catch {
                Write-Warning "Could not rename '$($item.Name)': $($_.Exception.Message)"
            }
        }
    }

    Write-Success "Renamed $renamed files/directories"
}

function Update-FileContents {
    param(
        [string]$TargetPath,
        [string]$OldName,
        [string]$NewName
    )

    Write-Info "Updating file contents..."

    $files = Get-FilesToRebrand -TargetPath $TargetPath
    $updated = 0
    $totalFiles = $files.Count

    foreach ($file in $files) {
        Write-Progress -Activity "Updating contents" -Status $file.Name -PercentComplete (($updated / $totalFiles) * 100)

        $content = Get-Content -Path $file.FullName -Raw -ErrorAction SilentlyContinue

        if ($content) {
            $originalContent = $content

            # Replace specific patterns first (most specific to least specific)
            # Order matters - do compound patterns before simple ones

            # Handle database name
            $content = $content -replace [regex]::Escape("${OldName}Db"), "${NewName}Db"

            # Handle frontend directory name
            $content = $content -replace [regex]::Escape("webtemplate-frontend"), "$($NewName.ToLower())-frontend"

            # Replace project name with word boundaries to avoid replacing framework classes like WebApplication
            $content = $content -replace "\b$([regex]::Escape($OldName))\b", $NewName

            # Case-sensitive replacement for lowercase versions with word boundaries
            if ($OldName.ToLower() -ne $OldName) {
                $oldLower = $OldName.ToLower()
                $newLower = $NewName.ToLower()
                $content = $content -replace "\b$([regex]::Escape($oldLower))\b", $newLower
            }

            if ($content -ne $originalContent) {
                Set-Content -Path $file.FullName -Value $content -NoNewline
                $updated++
            }
        }
    }

    Write-Success "Updated $updated files"
}

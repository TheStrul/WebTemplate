# Rename WebTemplate to WebTemplate throughout the entire project
# Run this from the workspace root

$ErrorActionPreference = "Stop"
$rootPath = $PSScriptRoot

Write-Host "Step 1: Renaming .csproj files..." -ForegroundColor Cyan
Get-ChildItem -Path $rootPath -Filter "WebTemplate*.csproj" -Recurse | ForEach-Object {
    $newName = $_.Name -replace "WebTemplate", "WebTemplate"
    $newPath = Join-Path $_.Directory $newName
    if (Test-Path $newPath) {
        Write-Host "  Skipped: $newName (already exists)" -ForegroundColor Yellow
    } else {
        Rename-Item $_.FullName $newName -Force
        Write-Host "  Renamed: $($_.Name) -> $newName" -ForegroundColor Green
    }
}

Write-Host "`nStep 2: Updating file contents..." -ForegroundColor Cyan
$extensions = @("*.cs", "*.csproj", "*.sln", "*.json", "*.md", "*.ps1", "*.ts", "*.tsx")
$files = Get-ChildItem -Path $rootPath -Include $extensions -Recurse -File |
    Where-Object { $_.FullName -notlike "*\node_modules\*" -and
                   $_.FullName -notlike "*\bin\*" -and
                   $_.FullName -notlike "*\obj\*" -and
                   $_.FullName -notlike "*\.vs\*" }

$updated = 0
foreach ($file in $files) {
    $content = Get-Content $file.FullName -Raw -ErrorAction SilentlyContinue
    if ($content) {
        $originalContent = $content

        # Replace with word boundaries to avoid WebApplication collision
        $content = $content -replace "WebTemplateDb", "WebTemplateDb"
        $content = $content -replace "webtemplate-frontend", "webtemplate-frontend"
        $content = $content -replace "\bWebApp\b", "WebTemplate"
        $content = $content -replace "\bwebapp\b", "webtemplate"

        if ($content -ne $originalContent) {
            Set-Content -Path $file.FullName -Value $content -NoNewline
            $updated++
            Write-Host "  Updated: $($file.Name)" -ForegroundColor Green
        }
    }
}
Write-Host "  Updated $updated files" -ForegroundColor Green

Write-Host "`nStep 3: Renaming directories..." -ForegroundColor Cyan
# Rename deepest first
$dirs = Get-ChildItem -Path $rootPath -Directory -Recurse |
    Where-Object { $_.Name -like "*WebTemplate*" } |
    Sort-Object { $_.FullName.Length } -Descending

foreach ($dir in $dirs) {
    if (-not (Test-Path $dir.FullName)) {
        continue
    }

    $newName = $dir.Name -replace "WebTemplate", "WebTemplate"
    $parentPath = Split-Path $dir.FullName -Parent
    $newPath = Join-Path $parentPath $newName

    if (Test-Path $newPath) {
        Write-Host "  Skipped: $newName (already exists)" -ForegroundColor Yellow
    } else {
        try {
            Rename-Item $dir.FullName $newName -Force
            Write-Host "  Renamed: $($dir.Name) -> $newName" -ForegroundColor Green
        }
        catch {
            Write-Host "  Failed: $($dir.Name) - $($_.Exception.Message)" -ForegroundColor Red
        }
    }
}

Write-Host "`nDone! Project renamed from WebTemplate to WebTemplate" -ForegroundColor Green
Write-Host "Note: Restart VS Code and rebuild solution" -ForegroundColor Yellow

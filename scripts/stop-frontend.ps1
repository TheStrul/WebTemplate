Write-Host "Stopping Frontend..." -ForegroundColor Red
taskkill /f /im node.exe 2>$null
Write-Host "Frontend stopped." -ForegroundColor Green
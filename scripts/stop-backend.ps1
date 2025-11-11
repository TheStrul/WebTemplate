Write-Host "Stopping Backend..." -ForegroundColor Red
taskkill /f /im dotnet.exe 2>$null
Write-Host "Backend stopped." -ForegroundColor Green
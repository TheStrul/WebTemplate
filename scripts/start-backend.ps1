Write-Host "Starting Backend..." -ForegroundColor Green
cd "c:\Users\avist\WebTemplate\Backend\WebTemplate.API"
Start-Process powershell -ArgumentList "-NoExit", "-Command", "dotnet run --launch-profile https"
Write-Host "Backend started at https://localhost:7295" -ForegroundColor Cyan
Write-Host "Starting Frontend..." -ForegroundColor Green
cd "c:\Users\avist\WebTemplate\Frontend\webtemplate-frontend"
Start-Process powershell -ArgumentList "-NoExit", "-Command", "npm start"
Write-Host "Frontend started at http://localhost:3000" -ForegroundColor Cyan
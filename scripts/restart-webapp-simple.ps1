Write-Host "=== WebTemplate Restart ===" -ForegroundColor Yellow

# Resolve project paths relative to this script
$root = $PSScriptRoot
if (-not $root) { $root = Split-Path -Parent $MyInvocation.MyCommand.Definition }
$backendPath = Join-Path $root 'Backend\WebTemplate.API'
$backendProj = Join-Path $backendPath 'WebTemplate.API.csproj'
$frontendPath = Join-Path $root 'Frontend\webtemplate-frontend'

# Stop WebTemplate services (simple and reliable approach)
Write-Host "Stopping WebTemplate services..." -ForegroundColor Red

# Kill processes by port (safest method)
Write-Host "Checking ports 5173 (Frontend), 5294/7295 (Backend)..." -ForegroundColor Yellow

# Port 5173 (Frontend - Vite)
$port5173 = netstat -ano | findstr ":5173"
if ($port5173) {
    $port5173 | ForEach-Object {
        $processId = ($_ -split '\s+')[-1]
        if ($processId -and $processId -match '^\d+$') {
            taskkill /f /pid $processId 2>$null
            Write-Host "Stopped process on port 5173: PID $processId" -ForegroundColor Gray
        }
    }
}

# Ports 5294 & 7295 (Backend)
@(5294, 7295) | ForEach-Object {
    $port = $_
    $connections = netstat -ano | findstr ":$port"
    if ($connections) {
        $connections | ForEach-Object {
            $processId = ($_ -split '\s+')[-1]
            if ($processId -and $processId -match '^\d+$') {
                taskkill /f /pid $processId 2>$null
                Write-Host "Stopped process on port $port`: PID $processId" -ForegroundColor Gray
            }
        }
    }
}

Start-Sleep -Seconds 3
Write-Host "Cleanup completed." -ForegroundColor Green

# Start Backend
Write-Host "Starting Backend..." -ForegroundColor Green
if (-not (Test-Path $backendPath)) {
    Write-Host "Backend path not found: $backendPath" -ForegroundColor Red
} elseif (-not (Test-Path $backendProj)) {
    Write-Host "Backend project file not found: $backendProj" -ForegroundColor Red
} else {
    Start-Process powershell -WorkingDirectory $backendPath -ArgumentList @(
        "-NoExit",
        "-Command",
        "dotnet run --project `"$backendProj`" --launch-profile https"
    )
}

# Wait for backend to be ready (max ~30 seconds)
Write-Host "Waiting for backend to start..." -ForegroundColor Yellow
$timeout = 15
$backendReady = $false

for ($i = 0; $i -lt $timeout; $i++) {
    Start-Sleep -Seconds 2
    try {
        $response = Invoke-WebRequest -Uri "https://localhost:7295" -SkipCertificateCheck -TimeoutSec 3 -ErrorAction SilentlyContinue
        $backendReady = $true
        break
    } catch {
        Write-Host "." -NoNewline -ForegroundColor Gray
    }
}

if (-not $backendReady) {
    Write-Host "`nBackend taking longer than expected, continuing anyway..." -ForegroundColor Yellow
}

Write-Host "`nBackend is ready!" -ForegroundColor Green

# Start Frontend (Vite)
Write-Host "Starting Frontend..." -ForegroundColor Green
if (-not (Test-Path $frontendPath)) {
    Write-Host "Frontend path not found: $frontendPath" -ForegroundColor Red
} else {
    # Ensure dependencies
    if (-not (Test-Path (Join-Path $frontendPath 'node_modules'))) {
        Write-Host "Installing frontend dependencies..." -ForegroundColor Yellow
        Start-Process powershell -WorkingDirectory $frontendPath -ArgumentList "-NoExit", "-Command", "npm install --legacy-peer-deps" | Out-Null
        Start-Sleep -Seconds 5
    }
    # Start Vite dev server
    Start-Process powershell -WorkingDirectory $frontendPath -ArgumentList "-NoExit", "-Command", "npm run dev"
}

# Wait a moment then open login page
Start-Sleep -Seconds 8
Write-Host "Opening frontend..." -ForegroundColor Cyan
Start-Process "http://localhost:5173"

Write-Host "`n=== Done! ===" -ForegroundColor Green
Write-Host "Backend: https://localhost:7295" -ForegroundColor Cyan
Write-Host "Frontend: http://localhost:5173" -ForegroundColor Cyan

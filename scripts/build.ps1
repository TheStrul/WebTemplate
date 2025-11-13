param(
  [ValidateSet('Debug','Release')]
  [string]$Configuration = 'Release',
  [switch]$SkipBackend,
  [switch]$SkipFrontend
)

$ErrorActionPreference = 'Stop'

function Invoke-Step {
  param(
    [string]$Name,
    [scriptblock]$Action
  )
  Write-Host "==> $Name" -ForegroundColor Cyan
  & $Action
}

# Resolve repo root
$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
Set-Location $ScriptDir

# Backend build
if (-not $SkipBackend) {
  Invoke-Step -Name "Restore & build backend ($Configuration)" -Action {
    # Prefer root solution, then Backend\WebTemplate.sln, then scan
    $rootSln = Join-Path $ScriptDir 'WebTemplate.sln'
    $backendSln = Join-Path $ScriptDir 'Backend\WebTemplate.sln'
    if (Test-Path $rootSln) {
      dotnet restore $rootSln
      dotnet build $rootSln -c $Configuration --nologo
    }
    elseif (Test-Path $backendSln) {
      dotnet restore $backendSln
      dotnet build $backendSln -c $Configuration --nologo
    }
    else {
      $solutions = Get-ChildItem -Path (Join-Path $ScriptDir 'Backend') -Filter *.sln -Recurse -ErrorAction SilentlyContinue
      if ($solutions) {
        foreach ($sol in $solutions) {
          dotnet restore $sol.FullName
          dotnet build $sol.FullName -c $Configuration --nologo
        }
      } else {
        $projects = Get-ChildItem -Path (Join-Path $ScriptDir 'Backend') -Filter *.csproj -Recurse -ErrorAction SilentlyContinue
        if (-not $projects) { throw "No backend solution or project files found." }
        foreach ($proj in $projects) {
          dotnet restore $proj.FullName
          dotnet build $proj.FullName -c $Configuration --nologo
        }
      }
    }
  }
}

# Frontend build
if (-not $SkipFrontend) {
  Invoke-Step -Name "Install deps & build frontend" -Action {
    $fePath = Join-Path $ScriptDir 'Frontend\webtemplate-frontend'
    if (-not (Test-Path (Join-Path $fePath 'package.json'))) {
      throw "Frontend package.json not found at $fePath"
    }

    Push-Location $fePath
    try {
      # Detect package manager
      $pm = 'npm'
      if (Test-Path (Join-Path $fePath 'pnpm-lock.yaml')) { $pm = 'pnpm' }
      elseif (Test-Path (Join-Path $fePath 'yarn.lock')) { $pm = 'yarn' }
      elseif (Test-Path (Join-Path $fePath 'package-lock.json')) { $pm = 'npm' }

      function Ensure-Cli($name) {
        $exists = Get-Command $name -ErrorAction SilentlyContinue
        if (-not $exists) { throw "Required CLI '$name' not found in PATH." }
      }

      switch ($pm) {
        'pnpm' { Ensure-Cli 'pnpm'; pnpm install --frozen-lockfile }
        'yarn' { Ensure-Cli 'yarn'; yarn install --frozen-lockfile }
        Default { Ensure-Cli 'npm'; if (Test-Path 'package-lock.json') { npm ci } else { npm install } }
      }

      switch ($pm) {
        'pnpm' { pnpm run build }
        'yarn' { yarn build }
        Default { npm run build }
      }
      Write-Host "Frontend build output at: $fePath\build" -ForegroundColor Green
    }
    finally { Pop-Location }
  }
}

Write-Host "==> All done" -ForegroundColor Green

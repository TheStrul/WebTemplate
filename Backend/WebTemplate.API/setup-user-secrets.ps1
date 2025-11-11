#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Interactive User Secrets Setup Script for WebTemplate.API

.DESCRIPTION
    This script checks if required user secrets are configured and prompts
    the user to set them interactively if they are missing. It ensures the
    application has all necessary secrets for local development.

.NOTES
    File Name      : setup-user-secrets.ps1
    Prerequisite   : .NET 9 SDK
    Author         : WebTemplate Team
#>

[CmdletBinding()]
param(
    [Parameter()]
    [switch]$Force,

    [Parameter()]
    [switch]$NonInteractive
)

$ErrorActionPreference = "Stop"
$InformationPreference = "Continue"

# ANSI color codes for better output
$ColorReset = "`e[0m"
$ColorGreen = "`e[32m"
$ColorYellow = "`e[33m"
$ColorRed = "`e[31m"
$ColorCyan = "`e[36m"
$ColorBold = "`e[1m"

function Write-ColorInfo {
    param([string]$Message)
    Write-Information "${ColorCyan}ℹ ${Message}${ColorReset}"
}

function Write-ColorSuccess {
    param([string]$Message)
    Write-Information "${ColorGreen}✓ ${Message}${ColorReset}"
}

function Write-ColorWarning {
    param([string]$Message)
    Write-Warning "${ColorYellow}⚠ ${Message}${ColorReset}"
}

function Write-ColorError {
    param([string]$Message)
    Write-Error "${ColorRed}✗ ${Message}${ColorReset}"
}

function Write-Header {
    param([string]$Title)
    Write-Host ""
    Write-Host "${ColorBold}${ColorCyan}═══════════════════════════════════════════════════════════════${ColorReset}"
    Write-Host "${ColorBold}${ColorCyan}  $Title${ColorReset}"
    Write-Host "${ColorBold}${ColorCyan}═══════════════════════════════════════════════════════════════${ColorReset}"
    Write-Host ""
}

function Get-UserSecretsList {
    try {
        $output = dotnet user-secrets list 2>&1
        if ($LASTEXITCODE -ne 0) {
            return @{}
        }

        $secrets = @{}
        $output | ForEach-Object {
            if ($_ -match '^(.+?)\s*=\s*(.*)$') {
                $secrets[$matches[1]] = $matches[2]
            }
        }
        return $secrets
    }
    catch {
        return @{}
    }
}

function Test-SecretExists {
    param([string]$SecretKey)

    $secrets = Get-UserSecretsList
    return $secrets.ContainsKey($SecretKey) -and ![string]::IsNullOrWhiteSpace($secrets[$SecretKey])
}

function Set-UserSecretInteractive {
    param(
        [string]$SecretKey,
        [string]$DefaultValue,
        [string]$Description,
        [switch]$IsPassword
    )

    Write-Host ""
    Write-Host "${ColorYellow}Setting: ${ColorBold}$SecretKey${ColorReset}"
    Write-Host "Description: $Description"

    if ($NonInteractive) {
        if ([string]::IsNullOrWhiteSpace($DefaultValue)) {
            Write-ColorError "Cannot set '$SecretKey' in non-interactive mode without a default value"
            return $false
        }
        $value = $DefaultValue
        Write-ColorInfo "Using default value (non-interactive mode)"
    }
    else {
        Write-Host "Default: ${ColorCyan}$DefaultValue${ColorReset}"
        Write-Host ""

        if ($IsPassword) {
            $secureValue = Read-Host "Enter value (or press Enter for default)" -AsSecureString
            $ptr = [System.Runtime.InteropServices.Marshal]::SecureStringToBSTR($secureValue)
            try {
                $value = [System.Runtime.InteropServices.Marshal]::PtrToStringBSTR($ptr)
            }
            finally {
                [System.Runtime.InteropServices.Marshal]::ZeroFreeBSTR($ptr)
            }
        }
        else {
            $value = Read-Host "Enter value (or press Enter for default)"
        }

        if ([string]::IsNullOrWhiteSpace($value)) {
            $value = $DefaultValue
        }
    }

    try {
        dotnet user-secrets set "$SecretKey" "$value" | Out-Null
        if ($LASTEXITCODE -eq 0) {
            Write-ColorSuccess "Saved '$SecretKey'"
            return $true
        }
        else {
            Write-ColorError "Failed to save '$SecretKey'"
            return $false
        }
    }
    catch {
        Write-ColorError "Error saving '$SecretKey': $_"
        return $false
    }
}

# Main script execution
Write-Header "WebTemplate User Secrets Setup"

# Check if .NET SDK is available
try {
    $dotnetVersion = dotnet --version
    Write-ColorSuccess ".NET SDK detected: $dotnetVersion"
}
catch {
    Write-ColorError ".NET SDK not found. Please install .NET 9 SDK or later."
    exit 1
}

# Check if user secrets are initialized
Write-ColorInfo "Checking user secrets initialization..."
$initOutput = dotnet user-secrets list 2>&1
if ($LASTEXITCODE -ne 0 -and $initOutput -like "*No user secrets*") {
    Write-ColorWarning "User secrets not initialized. Initializing now..."
    dotnet user-secrets init
    if ($LASTEXITCODE -eq 0) {
        Write-ColorSuccess "User secrets initialized"
    }
    else {
        Write-ColorError "Failed to initialize user secrets"
        exit 1
    }
}

# Define required secrets
$requiredSecrets = @(
    @{
        Key         = "JwtSettings:SecretKey"
        Default     = "Development-JWT-Secret-Key-2024-Not-For-Production-Use-Only-Local-Development"
        Description = "JWT secret key for token signing (required for authentication)"
        IsPassword  = $true
    },
    @{
        Key         = "AdminSeed:Password"
        Default     = "Admin123!"
        Description = "Admin user password for initial seeding"
        IsPassword  = $true
    }
)

$optionalSecrets = @(
    @{
        Key         = "AdminSeed:Email"
        Default     = "admin@WebTemplate.com"
        Description = "Admin user email address (optional override)"
        IsPassword  = $false
    },
    @{
        Key         = "AdminSeed:FirstName"
        Default     = "System"
        Description = "Admin user first name (optional override)"
        IsPassword  = $false
    },
    @{
        Key         = "AdminSeed:LastName"
        Default     = "Administrator"
        Description = "Admin user last name (optional override)"
        IsPassword  = $false
    }
)

# Check existing secrets
Write-Host ""
Write-ColorInfo "Checking current user secrets configuration..."
$missingRequired = @()
$missingOptional = @()

foreach ($secret in $requiredSecrets) {
    $exists = Test-SecretExists -SecretKey $secret.Key
    if ($exists -and !$Force) {
        Write-ColorSuccess "✓ $($secret.Key) is configured"
    }
    else {
        $missingRequired += $secret
        if (!$exists) {
            Write-ColorWarning "✗ $($secret.Key) is NOT configured"
        }
        else {
            Write-ColorInfo "⟳ $($secret.Key) will be reconfigured (--Force)"
        }
    }
}

foreach ($secret in $optionalSecrets) {
    $exists = Test-SecretExists -SecretKey $secret.Key
    if (!$exists -or $Force) {
        $missingOptional += $secret
    }
}

# If everything is configured and not forcing, exit
if ($missingRequired.Count -eq 0 -and !$Force) {
    Write-Host ""
    Write-ColorSuccess "All required user secrets are configured!"
    Write-Host ""
    Write-ColorInfo "To reconfigure secrets, run: ${ColorBold}.\setup-user-secrets.ps1 -Force${ColorReset}"
    Write-Host ""
    exit 0
}

# Configure missing/forced secrets
if ($missingRequired.Count -gt 0 -or $Force) {
    Write-Host ""
    Write-Header "Configuring Required Secrets"

    $successCount = 0
    foreach ($secret in $missingRequired) {
        $result = Set-UserSecretInteractive `
            -SecretKey $secret.Key `
            -DefaultValue $secret.Default `
            -Description $secret.Description `
            -IsPassword:$secret.IsPassword

        if ($result) {
            $successCount++
        }
    }

    if ($successCount -eq $missingRequired.Count) {
        Write-Host ""
        Write-ColorSuccess "All required secrets configured successfully!"
    }
    else {
        Write-Host ""
        Write-ColorError "Some secrets failed to configure. Please check the errors above."
        exit 1
    }
}

# Ask about optional secrets
if ($missingOptional.Count -gt 0 -and !$NonInteractive) {
    Write-Host ""
    Write-Host "${ColorYellow}Optional Secrets Available${ColorReset}"
    Write-Host "These secrets have defaults in appsettings.Development.json but can be overridden."
    Write-Host ""

    $configureOptional = Read-Host "Do you want to configure optional secrets? (y/N)"
    if ($configureOptional -eq 'y' -or $configureOptional -eq 'Y') {
        Write-Header "Configuring Optional Secrets"

        foreach ($secret in $missingOptional) {
            Set-UserSecretInteractive `
                -SecretKey $secret.Key `
                -DefaultValue $secret.Default `
                -Description $secret.Description `
                -IsPassword:$secret.IsPassword | Out-Null
        }
    }
}

# Display final summary
Write-Host ""
Write-Header "Setup Complete"

Write-ColorInfo "Listing all configured user secrets:"
Write-Host ""
dotnet user-secrets list

Write-Host ""
Write-ColorSuccess "User secrets setup completed successfully!"
Write-Host ""
Write-ColorInfo "You can now run the application with: ${ColorBold}dotnet run${ColorReset}"
Write-Host ""
Write-ColorInfo "To manage secrets later:"
Write-Host "  • List:   ${ColorBold}dotnet user-secrets list${ColorReset}"
Write-Host "  • Set:    ${ColorBold}dotnet user-secrets set 'Key' 'Value'${ColorReset}"
Write-Host "  • Remove: ${ColorBold}dotnet user-secrets remove 'Key'${ColorReset}"
Write-Host "  • Clear:  ${ColorBold}dotnet user-secrets clear${ColorReset}"
Write-Host ""

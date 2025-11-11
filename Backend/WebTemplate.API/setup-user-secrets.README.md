# Setup User Secrets Script

## Quick Start

```powershell
pwsh setup-user-secrets.ps1
```

## What It Does

This interactive script configures required User Secrets for local development:

- Checks if User Secrets are initialized
- Detects missing required secrets (`JwtSettings:SecretKey`, `AdminSeed:Password`)
- Prompts you to configure them with sensible defaults
- Optionally configures additional admin settings
- Validates the configuration

## Usage

### Interactive Mode (Default)

Prompts for each value with defaults:

```powershell
pwsh setup-user-secrets.ps1
```

### Force Reconfiguration

Reconfigure all secrets even if already set:

```powershell
pwsh setup-user-secrets.ps1 -Force
```

### Non-Interactive Mode

Use defaults without prompts (useful for automation):

```powershell
pwsh setup-user-secrets.ps1 -NonInteractive
```

## Features

- ✅ Colored, user-friendly output
- ✅ Validates existing secrets before prompting
- ✅ Provides sensible defaults for development
- ✅ Secure password input
- ✅ Optional secrets configuration
- ✅ Summary of all configured secrets

## Automatic Execution

The script runs automatically when you start the application (`dotnet run`) if required secrets are missing. You'll be prompted:

```text
⚠  MISSING REQUIRED USER SECRETS

The following required secrets are not configured:
  ✗ JwtSettings:SecretKey
  ✗ AdminSeed:Password

Would you like to run the setup script now? (Y/n)
```

## Requirements

- PowerShell Core (`pwsh`) or Windows PowerShell
- .NET 9 SDK
- Project with User Secrets initialized (already done)

## For More Information

See `Backend/USER_SECRETS_SETUP.md` for comprehensive documentation.

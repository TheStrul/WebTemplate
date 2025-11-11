# User Secrets Setup Guide

## Overview

Sensitive configuration values (JWT secrets, admin passwords, API keys) are stored in User Secrets for local development, not in `appsettings.json` files. This prevents accidental commits of secrets to source control.

## Quick Start (Recommended)

**The easiest way to set up user secrets is to run the interactive setup script:**

```powershell
cd Backend/WebTemplate.API
pwsh setup-user-secrets.ps1
```

This script will:

- ✓ Check if user secrets are initialized
- ✓ Detect which secrets are missing
- ✓ Prompt you interactively to configure them
- ✓ Use sensible defaults for development
- ✓ Validate the configuration

**The script runs automatically when you start the application if secrets are missing.**

## Manual Setup (Alternative)

If you prefer to configure secrets manually, follow these steps:

### Prerequisites

- .NET 9 SDK installed
- Project initialized with User Secrets (already done for `WebTemplate.API`)

### Required User Secrets

Run these commands from the `Backend/WebTemplate.API` directory:

```powershell
# JWT Secret Key (required for authentication)
dotnet user-secrets set "JwtSettings:SecretKey" "Development-JWT-Secret-Key-2024-Not-For-Production-Use-Only-Local-Development"

# Admin Seed Password (required for admin user creation)
dotnet user-secrets set "AdminSeed:Password" "Admin123!"
```

### Optional: Custom Admin Configuration

If you want to use a different admin email or credentials:

```powershell
dotnet user-secrets set "AdminSeed:Email" "your-admin@example.com"
dotnet user-secrets set "AdminSeed:FirstName" "Your"
dotnet user-secrets set "AdminSeed:LastName" "Name"
```

### Optional: SMTP Configuration

For local email testing (if not using default localhost:1025):

```powershell
dotnet user-secrets set "Email:SmtpUser" "your-smtp-username"
dotnet user-secrets set "Email:SmtpPassword" "your-smtp-password"
```

## Script Options

The `setup-user-secrets.ps1` script supports several options:

```powershell
# Interactive mode (default) - prompts for each value
pwsh setup-user-secrets.ps1

# Force reconfiguration of all secrets
pwsh setup-user-secrets.ps1 -Force

# Non-interactive mode - uses defaults without prompts (for CI/CD)
pwsh setup-user-secrets.ps1 -NonInteractive
```

## Verification

To list all configured secrets:

```powershell
dotnet user-secrets list
```

Expected output should include:

```text
AdminSeed:Password = Admin123!
JwtSettings:SecretKey = Development-JWT-Secret-Key-2024-Not-For-Production-Use-Only-Local-Development
```

## Automatic Setup on Application Start

When you run the application (`dotnet run`) in Development mode:

1. The application checks for required user secrets
2. If any are missing, you'll see a prompt asking if you want to run the setup script
3. If you choose "Yes" (default), the script runs automatically
4. After setup completes, restart the application

This ensures developers can never accidentally run the app without proper configuration.

## How It Works

1. User Secrets are stored in your user profile directory:
   - **Windows**: `%APPDATA%\Microsoft\UserSecrets\<user_secrets_id>\secrets.json`
   - **macOS/Linux**: `~/.microsoft/usersecrets/<user_secrets_id>/secrets.json`

2. The `<UserSecretsId>` is stored in the `.csproj` file (already configured)

3. In Development environment, User Secrets automatically override values from `appsettings.json`

4. Values are loaded in this order (later sources override earlier):
   - `appsettings.json`
   - `appsettings.Development.json`
   - User Secrets (Development only)
   - Environment Variables
   - Command Line Arguments

## Production Configuration

For production environments, **DO NOT** use User Secrets. Instead:

- **Azure App Service**: Use Application Settings in Azure Portal
- **Azure Key Vault**: Reference secrets via configuration
- **Environment Variables**: Set via deployment pipeline
- **CI/CD Secrets**: Store in GitHub Secrets, Azure DevOps Library, etc.

## Troubleshooting

### "Secret does not exist" or authentication fails

Re-run the required setup commands above.

### Need to change a secret

Simply run the `dotnet user-secrets set` command again with the new value.

### Want to remove all secrets

```powershell
dotnet user-secrets clear
```

### Check which project is configured

```powershell
dotnet user-secrets list --project Backend/WebTemplate.API/WebTemplate.API.csproj
```

## Security Notes

- ✅ User Secrets are stored in **plain text** on your local machine
- ✅ They are **never** included in source control
- ✅ They only work in the **Development** environment
- ❌ **DO NOT** use User Secrets for production secrets
- ❌ **DO NOT** commit the actual secret values to documentation

## Team Onboarding

When a new developer joins:

1. Clone the repository
2. Run the setup commands from this guide
3. Verify with `dotnet user-secrets list`
4. Run the application with `dotnet run` or your preferred method

## Reference

- [Safe storage of app secrets in development in ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets)
- [Configuration in ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/configuration/)

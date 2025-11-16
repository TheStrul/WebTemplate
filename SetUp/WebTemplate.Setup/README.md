# WebTemplate.Setup - Project Generation & Configuration Tool

## Overview

**WebTemplate.Setup** is a comprehensive WinForms application for generating and configuring new WebTemplate-based projects. It provides a GUI-driven workflow for:

- Creating new project workspaces
- Configuring features, secrets, database, and server settings
- Saving/loading configuration profiles for iterative development
- Automating database initialization
- Managing secrets (User Secrets, Azure Key Vault, Environment Variables)

## Architecture

### Models (`Models/`)

All configuration models support validation and can be serialized to JSON.

#### `WorkspaceConfiguration.cs`
Master configuration object containing all project settings:
- **Project Settings**: Name, target path, Git options
- **Features Configuration**: Which features to enable/disable
- **Secrets Configuration**: Strategy and credentials
- **Database Configuration**: Connection strings and initialization
- **Server Configuration**: URLs and timeouts
- **Email Configuration**: SMTP settings
- **Auth Configuration**: JWT settings

#### `ProjectSettings.cs`
- Project name, target path, company name, author
- Git initialization options
- Validation options
- Validates project name as valid C# identifier

#### `FeaturesConfiguration.cs`
**Mirrors `FeaturesOptions` from WebTemplate.Core**, includes:
- Swagger
- CORS (with allowed origins, methods, headers)
- Rate Limiting
- Security Headers
- Health Checks
- Identity/Auth
- Refresh Tokens
- Admin Seed (with email, password, name)
- Exception Handling
- Serilog

Each feature has enable/disable flag plus feature-specific settings.

#### `SecretsConfiguration.cs`
Defines secrets management strategy:
- **Strategy enum**: `UserSecrets`, `AzureKeyVault`, `EnvironmentVariables`, `Mixed`
- **AzureKeyVaultSettings**: Vault URI, Tenant ID, Client ID, managed identity options
- **UserSecretsValues**: Key-value pairs for user secrets
- **EnvironmentVariables**: Key-value pairs for env vars

#### `DatabaseConfiguration.cs`
- Connection string
- Provider (SqlServer, PostgreSQL, MySQL)
- Execute init script flag
- Init script path
- Test connection flag
- Create database if not exists flag

#### `ServerConfiguration.cs`
- Server URL (HTTP)
- HTTPS URL
- Connection timeout
- HTTPS redirection flag

#### `EmailConfiguration.cs`
- SMTP host, port, username, password
- Enable SSL flag
- From email and display name
- Enabled flag

#### `AuthConfiguration.cs`
- JWT secret key
- Issuer, Audience
- Token expiration (minutes)
- Refresh token expiration (days)
- Frontend URL for email confirmation

### Services (`Services/`)

#### `ConfigurationPersistenceService.cs`
**Manages saving/loading/listing workspace configurations**

**Location**: `Configurations/` folder at template root
- Each configuration saved in its own subfolder (by ConfigurationId)
- Main file: `workspace-config.json`

**Methods**:
- `SaveConfigurationAsync()` - Validates and saves configuration
- `LoadConfigurationAsync()` - Loads configuration by ID
- `ListConfigurations()` - Returns list of all saved configurations with summaries
- `DeleteConfiguration()` - Removes a configuration
- `ConfigurationExists()` - Checks if configuration exists
- `GenerateConfigurationId()` - Creates unique ID from name

**Features**:
- JSON serialization with indentation
- Automatic timestamp management (CreatedAt, ModifiedAt)
- Validation before saving
- Unique ID generation

#### `ProjectGenerationService.cs`
**Orchestrates complete project generation workflow**

**Process**:
1. Validates configuration
2. Runs `New-ProjectFromTemplate.ps1` with appropriate arguments
3. Generates configuration files (`appsettings.Local.json`, `appsettings.Production.json`)
4. Configures secrets using selected strategy
5. Initializes database (if configured)
6. Reports progress throughout

**Methods**:
- `GenerateProjectAsync()` - Main generation method with progress reporting
- `BuildScriptArguments()` - Builds PowerShell arguments from configuration
- `GenerateConfigurationFilesAsync()` - Creates appsettings files
- `GenerateLocalSettings()` - Builds development settings
- `GenerateProductionSettings()` - Builds production template
- `ConfigureSecretsAsync()` - Delegates to SecretsService
- `RunPowerShellAsync()` - Executes PowerShell with output capture

**Configuration Files Generated**:
- `appsettings.Local.json` - Full development settings with actual values
- `appsettings.Production.json` - Template with placeholders for production

#### `DatabaseService.cs`
**Handles all database operations**

**Methods**:
- `TestConnectionAsync()` - Tests database connectivity
- `CreateDatabaseIfNotExistsAsync()` - Creates database if missing
- `ExecuteInitScriptAsync()` - Runs `db-init.sql` with batch support
- `SplitSqlBatches()` - Splits script by `GO` statements
- `GetDatabaseName()` - Extracts database name from connection string
- `ValidateConnectionString()` - Validates connection string format

**Features**:
- Handles SQL batches (GO statements)
- 5-minute command timeout for long scripts
- Continues execution on batch errors (collects all errors)
- Proper SQL exception handling

#### `SecretsService.cs`
**Manages secrets using different strategies**

**Strategy Pattern**: Interface `ISecretsStrategy` with implementations:

1. **UserSecretsStrategy**
   - Uses `dotnet user-secrets` CLI
   - Initializes secrets for API project
   - Sets JWT secret, admin password, SMTP password
   - Adds custom user secrets from configuration

2. **AzureKeyVaultStrategy**
   - **STATUS**: Placeholder waiting for user's reference implementation
   - Will connect to Azure Key Vault
   - Store secrets in Key Vault
   - Generate configuration with Key Vault references

3. **EnvironmentVariablesStrategy**
   - Generates `.env` file with secrets
   - Uses double-underscore notation (e.g., `JwtSettings__SecretKey`)
   - Creates `secrets.env` in project root

4. **MixedSecretsStrategy**
   - Configures User Secrets for development
   - Configures Azure Key Vault for production
   - **STATUS**: Partially implemented (Key Vault pending)

**Secrets Configured**:
- `JwtSettings:SecretKey`
- `AdminSeed:Password`
- `EmailSettings:SmtpPassword`
- Plus any custom secrets from configuration

## Configuration Storage Structure

```
Configurations/
├── my-ecommerce-app/
│   └── workspace-config.json
├── my-blog-platform/
│   └── workspace-config.json
└── corporate-intranet/
    └── workspace-config.json
```

Each `workspace-config.json` contains:
- All project settings
- All feature configurations
- All secrets settings
- All infrastructure settings

## User Workflow

### Intended Usage (UI to be built):

1. **User opens WebTemplate.Setup**
2. **Choose action**:
   - "New Configuration" - Create from scratch
   - "Load Configuration" - Load existing configuration
3. **Configure project** (via tabs):
   - **Project Tab**: Name, path, company, author
   - **Features Tab**: Checkboxes for each feature + settings
   - **Secrets Tab**: Choose strategy (User Secrets / Key Vault / Env Vars)
   - **Database Tab**: Connection string, init options
   - **Server Tab**: URLs, timeouts
   - **Email Tab**: SMTP settings (if needed)
   - **Auth Tab**: JWT settings, frontend URL
4. **Save Configuration** - Persist to `Configurations/` folder
5. **Generate Project** - Run full generation workflow
6. **Iterate**:
   - Test the generated project
   - Load same configuration
   - Adjust settings
   - Regenerate

### Benefits:
- **Repeatable**: Save configurations for reuse
- **Iterative**: Test, adjust, regenerate
- **Visual**: No manual JSON editing
- **Validated**: All settings validated before generation
- **Comprehensive**: Handles all aspects of project setup

## Dependencies

**NuGet Packages**:
- `Microsoft.Data.SqlClient` (5.2.2) - For database operations
- `System.Text.Json` (9.0.0) - For JSON serialization

**External Tools**:
- `pwsh` (PowerShell) - For running template scripts
- `dotnet` CLI - For user secrets management
- SQL Server - For database operations

## Clarifications Needed

The following items are marked for clarification with the user:

1. **Configuration Storage Location**
   - Currently: `Configurations/` at template root
   - Alternative: User's AppData folder?
   - **Decision**: Template root chosen for portability

2. **Mixed Secrets Strategy**
   - How exactly should dev vs prod be handled?
   - Should it generate separate config files?
   - **Current**: User Secrets for dev, Key Vault reference for prod

3. **Azure Key Vault Implementation**
   - Waiting for user's reference implementation
   - Need to see existing Key Vault integration code

4. **Configuration File Generation**
   - Full settings or minimal overrides in appsettings.Local.json?
   - **Current**: Full comprehensive settings

5. **Database Provider Support**
   - Currently only SQL Server implemented
   - PostgreSQL/MySQL support needed?

## Next Steps

### Backend Complete ✅
All backend services are implemented and ready.

### UI To Be Built (With User) 🔜
The WinForms UI needs to be designed and implemented:

1. **Main Form** with tabs:
   - Project Settings
   - Features Configuration
   - Secrets Configuration
   - Database Settings
   - Server Settings
   - Email Settings
   - Auth Settings

2. **Configuration Management**:
   - New/Load/Save/Delete buttons
   - Configuration list view
   - Validation feedback

3. **Generation Workflow**:
   - Generate button
   - Progress bar
   - Output log window
   - Success/failure messaging

4. **Features Tab** specifics:
   - Checkbox for each feature
   - Expandable panels for feature-specific settings
   - CORS origins list editor
   - Rate limiting sliders
   - Admin seed form

## Testing Strategy

**Recommended Tests**:
1. Save/Load configuration round-trip
2. Project generation with minimal features
3. Project generation with all features enabled
4. Database initialization with db-init.sql
5. User secrets configuration
6. Environment variables generation
7. Configuration validation (missing required fields)
8. PowerShell script execution
9. Error handling (invalid paths, connection failures)

## File Summary

**Models** (8 files):
- `WorkspaceConfiguration.cs` - Master config + validation
- `ProjectSettings.cs` - Project metadata
- `FeaturesConfiguration.cs` - Feature flags + settings
- `SecretsConfiguration.cs` - Secrets strategy + settings
- `DatabaseConfiguration.cs` - Database settings
- `ServerConfiguration.cs` - Server/hosting settings
- `EmailConfiguration.cs` - SMTP settings
- `AuthConfiguration.cs` - JWT/auth settings

**Services** (4 files):
- `ConfigurationPersistenceService.cs` - Save/load configurations
- `ProjectGenerationService.cs` - Orchestrate project generation
- `DatabaseService.cs` - Database operations
- `SecretsService.cs` - Secrets management strategies

**Total**: 12 new files + updated .csproj

---

## NO FALLBACKS Policy ✅

All services follow the NO FALLBACKS principle:
- Required fields MUST be provided
- Validation fails fast with clear error messages
- No silent defaults for critical settings
- Explicit configuration required

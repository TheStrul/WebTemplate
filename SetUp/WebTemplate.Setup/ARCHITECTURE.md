# WebTemplate.Setup Architecture Diagram

## System Overview

```
┌─────────────────────────────────────────────────────────────────┐
│                     WebTemplate.Setup (WinForms UI)              │
│                         [To Be Built Together]                   │
└────────────────────────┬────────────────────────────────────────┘
                         │
                         ├─────────────────────────────────────┐
                         │                                     │
                         ▼                                     ▼
            ┌────────────────────────┐          ┌───────────────────────┐
            │  ConfigurationPersistence│         │  ProjectGeneration    │
            │       Service            │         │      Service          │
            │  ✅ IMPLEMENTED          │         │  ✅ IMPLEMENTED        │
            └────────────┬─────────────┘         └───────────┬───────────┘
                         │                                    │
                         │                                    │
                         ▼                                    ▼
            ┌────────────────────────┐          ┌───────────────────────┐
            │   Configurations/       │         │  PowerShell Script     │
            │   ├─ config-1/          │         │  New-ProjectFrom       │
            │   │  └─ workspace-      │         │  Template.ps1          │
            │   │     config.json     │         └───────────┬───────────┘
            │   └─ config-2/          │                     │
            │      └─ workspace-      │                     │
            │         config.json     │         ┌───────────┴───────────┐
            └─────────────────────────┘         │                       │
                                                 ▼                       ▼
                                    ┌──────────────────┐  ┌──────────────────┐
                                    │  Database        │  │  Secrets         │
                                    │  Service         │  │  Service         │
                                    │  ✅ IMPLEMENTED   │  │  ✅ IMPLEMENTED   │
                                    └──────────────────┘  └──────────────────┘
                                             │                     │
                                             │                     │
                                             ▼                     ▼
                                    ┌──────────────────┐  ┌──────────────────┐
                                    │  SQL Server      │  │  Strategy        │
                                    │  - Test conn     │  │  Pattern         │
                                    │  - Create DB     │  │  ├─ UserSecrets  │
                                    │  - Execute SQL   │  │  ├─ KeyVault*    │
                                    └──────────────────┘  │  ├─ EnvVars      │
                                                          │  └─ Mixed        │
                                                          └──────────────────┘
```

## Data Flow: Generate Project

```
User Action: "Generate Project"
│
├─ 1. Load Configuration
│     └─ ConfigurationPersistenceService.LoadConfigurationAsync()
│
├─ 2. Validate Configuration
│     └─ WorkspaceConfiguration.Validate()
│         ├─ Project.Validate()
│         ├─ Features.Validate()
│         ├─ Secrets.Validate()
│         ├─ Database.Validate()
│         └─ Server/Email/Auth.Validate()
│
├─ 3. Generate Project
│     └─ ProjectGenerationService.GenerateProjectAsync()
│         │
│         ├─ Run PowerShell Script
│         │   └─ New-ProjectFromTemplate.ps1
│         │       ├─ Copy template files
│         │       ├─ Rebrand namespaces
│         │       ├─ Update project files
│         │       └─ Initialize Git (optional)
│         │
│         ├─ Generate Configuration Files
│         │   ├─ appsettings.Local.json (full config)
│         │   └─ appsettings.Production.json (template)
│         │
│         ├─ Configure Secrets
│         │   └─ SecretsService.ConfigureSecretsAsync()
│         │       └─ [Strategy Pattern]
│         │           ├─ UserSecretsStrategy
│         │           │   └─ dotnet user-secrets set
│         │           ├─ AzureKeyVaultStrategy*
│         │           ├─ EnvironmentVariablesStrategy
│         │           │   └─ Generate .env file
│         │           └─ MixedSecretsStrategy
│         │
│         └─ Initialize Database (if configured)
│             └─ DatabaseService.ExecuteInitScriptAsync()
│                 ├─ Create database (if needed)
│                 └─ Execute db-init.sql (batches)
│
└─ 4. Report Success
      └─ Show generated project path
```

## Configuration Object Hierarchy

```
WorkspaceConfiguration
├─ ConfigurationId: string
├─ ConfigurationName: string
├─ Description: string
├─ CreatedAt: DateTime
├─ ModifiedAt: DateTime
│
├─ Project: ProjectSettings
│   ├─ ProjectName: string
│   ├─ TargetPath: string
│   ├─ CompanyName: string
│   ├─ AuthorName: string
│   ├─ InitializeGit: bool
│   ├─ CreateInitialCommit: bool
│   └─ RunValidation: bool
│
├─ Features: FeaturesConfiguration
│   ├─ Swagger: SwaggerFeature
│   ├─ Cors: CorsFeature
│   ├─ RateLimiting: RateLimitingFeature
│   ├─ SecurityHeaders: SecurityHeadersFeature
│   ├─ HealthChecks: HealthChecksFeature
│   ├─ IdentityAuth: IdentityAuthFeature
│   ├─ RefreshTokens: RefreshTokensFeature
│   ├─ AdminSeed: AdminSeedFeature
│   ├─ ExceptionHandling: ExceptionHandlingFeature
│   └─ Serilog: SerilogFeature
│
├─ Secrets: SecretsConfiguration
│   ├─ Strategy: SecretsStrategy (enum)
│   ├─ KeyVault: AzureKeyVaultSettings?
│   ├─ UserSecretsValues: Dictionary<string, string>
│   └─ EnvironmentVariables: Dictionary<string, string>
│
├─ Database: DatabaseConfiguration
│   ├─ ConnectionString: string
│   ├─ Provider: string
│   ├─ ExecuteInitScript: bool
│   ├─ InitScriptPath: string
│   ├─ TestConnection: bool
│   └─ CreateDatabaseIfNotExists: bool
│
├─ Server: ServerConfiguration
│   ├─ Url: string
│   ├─ HttpsUrl: string?
│   ├─ ConnectionTimeoutSeconds: int
│   └─ UseHttpsRedirection: bool
│
├─ Email: EmailConfiguration
│   ├─ Enabled: bool
│   ├─ SmtpHost: string
│   ├─ SmtpPort: int
│   ├─ SmtpUsername: string
│   ├─ SmtpPassword: string
│   ├─ EnableSsl: bool
│   ├─ FromEmail: string
│   └─ FromName: string
│
└─ Auth: AuthConfiguration
    ├─ SecretKey: string
    ├─ Issuer: string
    ├─ Audience: string
    ├─ ExpirationMinutes: int
    ├─ RefreshTokenExpirationDays: int
    └─ FrontendUrl: string
```

## UI Structure (Proposed)

```
┌────────────────────────────────────────────────────────────────┐
│  WebTemplate.Setup - Project Configuration                     │
├────────────────────────────────────────────────────────────────┤
│  Configuration: [my-ecommerce-app ▼]  [New] [Load] [Save] [Del]│
├────────────────────────────────────────────────────────────────┤
│  ┌─ Tabs ─────────────────────────────────────────────────┐   │
│  │ [Project] [Features] [Secrets] [Database] [Server] ... │   │
│  ├──────────────────────────────────────────────────────────┤   │
│  │                                                          │   │
│  │  CURRENT TAB CONTENT                                     │   │
│  │  (Form fields, checkboxes, inputs, etc.)                │   │
│  │                                                          │   │
│  │                                                          │   │
│  │                                                          │   │
│  │                                                          │   │
│  └──────────────────────────────────────────────────────────┘   │
├────────────────────────────────────────────────────────────────┤
│  [Validate]                                 [Generate Project] │
├────────────────────────────────────────────────────────────────┤
│  Progress: ████████████░░░░░░░░░░░░░░░░░░░░░░░░ 40%          │
│  Status: Configuring secrets...                               │
└────────────────────────────────────────────────────────────────┘
```

## Secrets Strategy Pattern

```
                    ISecretsStrategy
                           │
                           │ interface
           ┌───────────────┼───────────────┬───────────────┐
           │               │               │               │
           ▼               ▼               ▼               ▼
  UserSecretsStrategy  KeyVaultStrategy  EnvVarsStrategy  MixedStrategy
  ✅ IMPLEMENTED       ⏸️ PENDING         ✅ IMPLEMENTED   ⏸️ PENDING
           │               │               │               │
           │               │               │               │
           ▼               ▼               ▼               ▼
     dotnet CLI       Azure SDK       .env file      User + KeyVault
     user-secrets     Key Vault       generator      combination
```

## File Storage Structure

```
WebTemplate/
├─ Configurations/              ← Configuration storage
│  ├─ my-ecommerce-app/
│  │  └─ workspace-config.json
│  ├─ my-blog-platform/
│  │  └─ workspace-config.json
│  └─ corporate-intranet/
│     └─ workspace-config.json
│
├─ template-scripts/            ← PowerShell scripts
│  └─ New-ProjectFromTemplate.ps1
│
├─ Backend/                     ← Template source
│  └─ WebTemplate.*/
│
└─ WebTemplate.Setup/           ← This application
   ├─ Models/
   │  ├─ WorkspaceConfiguration.cs
   │  ├─ ProjectSettings.cs
   │  ├─ FeaturesConfiguration.cs
   │  ├─ SecretsConfiguration.cs
   │  ├─ DatabaseConfiguration.cs
   │  ├─ ServerConfiguration.cs
   │  ├─ EmailConfiguration.cs
   │  └─ AuthConfiguration.cs
   ├─ Services/
   │  ├─ ConfigurationPersistenceService.cs
   │  ├─ ProjectGenerationService.cs
   │  ├─ DatabaseService.cs
   │  └─ SecretsService.cs
   └─ [UI Files - To Be Built]
```

## Legend

- ✅ IMPLEMENTED - Fully functional, tested, ready
- ⏸️ PENDING - Placeholder, needs implementation
- * - Waiting for user's reference code

---

**All backend infrastructure is ready for UI integration!**

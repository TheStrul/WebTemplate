# Hierarchical Configuration Singleton Pattern

## Overview

This document describes the **NO FALLBACKS** hierarchical configuration singleton pattern implemented in WebTemplate. This pattern ensures all required configuration is explicitly provided, fails fast with clear error messages, and provides proper dependency layering across the application.

## Architecture

### Three-Layer Hierarchy

```
ICoreConfiguration (WebTemplate.Core)
    ↓
IDataConfiguration (WebTemplate.Data)
    ↓
IApiConfiguration (WebTemplate.API)
```

Each layer extends the previous one, creating a clean dependency hierarchy:

- **Core Layer**: Fundamental configuration (JWT, Auth, Email, AppUrls, etc.)
- **Data Layer**: Database and persistence configuration (ConnectionString, Retry policies)
- **API Layer**: HTTP and hosting configuration (Features, CORS, Logging, etc.)

### NO FALLBACKS Policy

**CRITICAL**: This architecture follows a strict **NO FALLBACKS** policy:

- ❌ No default values for required configuration
- ❌ No `?? fallbackValue` or `|| defaultValue` patterns
- ❌ No `.GetValueOrDefault()` with defaults
- ✅ All required configuration must be explicitly provided
- ✅ Fails fast at startup with clear error messages
- ✅ Errors guide developers to fix configuration issues

## Implementation Guide

### Step 1: Define Configuration Interfaces

**Location**: `WebTemplate.Core/Configuration/`

```csharp
// ICoreConfiguration.cs
namespace WebTemplate.Core.Configuration;

public interface ICoreConfiguration
{
    JwtSettings Jwt { get; }
    AuthSettings Auth { get; }
    EmailSettings Email { get; }
    AppUrls AppUrls { get; }
    AdminSeedSettings AdminSeed { get; }
    UserModuleFeatures UserModuleFeatures { get; }
    ResponseMessages ResponseMessages { get; }
}
```

**Location**: `WebTemplate.Data/Configuration/`

```csharp
// IDataConfiguration.cs
namespace WebTemplate.Data.Configuration;

public interface IDataConfiguration : ICoreConfiguration
{
    string ConnectionString { get; }
    DatabaseRetrySettings DatabaseRetry { get; }
}
```

**Location**: `WebTemplate.API/Configuration/`

```csharp
// IApiConfiguration.cs
namespace WebTemplate.API.Configuration;

public interface IApiConfiguration : IDataConfiguration
{
    FeaturesOptions Features { get; }
    string AllowedHosts { get; }
    LoggingSettings Logging { get; }
}
```

### Step 2: Implement Configuration Classes

Each implementation validates configuration on construction:

```csharp
// CoreConfiguration.cs
public class CoreConfiguration : ICoreConfiguration
{
    public JwtSettings Jwt { get; }
    public AuthSettings Auth { get; }
    // ... other properties

    public CoreConfiguration(IConfiguration configuration)
    {
        // Bind settings - NO FALLBACKS!
        Jwt = configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>()
            ?? throw new InvalidOperationException(
                $"Required configuration section '{JwtSettings.SectionName}' is missing or invalid.");

        ValidateJwtSettings(Jwt);

        // Repeat for all settings...
    }

    private static void ValidateJwtSettings(JwtSettings settings)
    {
        if (string.IsNullOrWhiteSpace(settings.SecretKey))
            throw new InvalidOperationException(
                "JwtSettings:SecretKey is required but not configured. " +
                "Please set it in User Secrets or environment variables.");

        if (settings.AccessTokenExpiryMinutes <= 0)
            throw new InvalidOperationException(
                "JwtSettings:AccessTokenExpiryMinutes must be greater than 0.");

        // Validate all required fields...
    }
}
```

**Key Validation Patterns:**

1. **Required strings**: Check for `IsNullOrWhiteSpace`, throw with specific path
2. **Required objects**: Check for `null`, throw with section name
3. **Value constraints**: Validate ranges, formats, relationships
4. **Clear error messages**: Include configuration path and helpful guidance

### Step 3: Register as Singleton

**Location**: `Program.cs`

```csharp
// Create single instance
var apiConfig = new ApiConfiguration(builder.Configuration);

// Register for all three interfaces
builder.Services.AddSingleton<IApiConfiguration>(apiConfig);
builder.Services.AddSingleton<IDataConfiguration>(apiConfig);
builder.Services.AddSingleton<ICoreConfiguration>(apiConfig);
```

**Why Singleton?**
- Configuration is immutable after startup
- Validation happens once at startup (fail fast)
- Better performance (no repeated binding)
- Single source of truth

### Step 4: Inject into Services

Replace `IOptions<T>` with configuration interfaces:

**Before (IOptions pattern):**
```csharp
public class TokenService : ITokenService
{
    private readonly JwtSettings _jwtSettings;

    public TokenService(IOptions<JwtSettings> jwtSettings)
    {
        _jwtSettings = jwtSettings.Value;
    }
}
```

**After (Configuration singleton):**
```csharp
public class TokenService : ITokenService
{
    private readonly JwtSettings _jwtSettings;

    public TokenService(ICoreConfiguration configuration)
    {
        _jwtSettings = configuration.Jwt;
    }
}
```

**Benefits:**
- One constructor parameter instead of multiple `IOptions<T>`
- Access all related configuration through single object
- Proper dependency layering (Core services use ICoreConfiguration)

### Step 5: Update Unit Tests

Replace `Options.Create()` with `Mock<ICoreConfiguration>`:

**Before:**
```csharp
public class TokenServiceTests
{
    private readonly JwtSettings _jwtSettings = new() { /* ... */ };

    [Fact]
    public void Test()
    {
        var service = new TokenService(Options.Create(_jwtSettings));
        // ...
    }
}
```

**After:**
```csharp
public class TokenServiceTests
{
    private readonly JwtSettings _jwtSettings = new() { /* ... */ };
    private readonly Mock<ICoreConfiguration> _configMock = new();

    public TokenServiceTests()
    {
        _configMock.Setup(c => c.Jwt).Returns(_jwtSettings);
    }

    [Fact]
    public void Test()
    {
        var service = new TokenService(_configMock.Object);
        // ...
    }
}
```

## Configuration Structure

### appsettings.json

```json
{
  "JwtSettings": {
    "Issuer": "WebTemplate.API",
    "Audience": "WebTemplate.Client",
    "AccessTokenExpiryMinutes": 60,
    "RefreshTokenExpiryDays": 30,
    "ClockSkewMinutes": 5
  },
  "AuthSettings": {
    "Password": {
      "RequiredLength": 8,
      "RequireDigit": true,
      "RequireLowercase": true,
      "RequireUppercase": true,
      "RequireNonAlphanumeric": true,
      "RequiredUniqueChars": 6
    },
    "User": {
      "RequireConfirmedEmail": false
    }
  },
  "Email": {
    "Provider": "Smtp",
    "From": "no-reply@webtemplate.com",
    "FromName": "WebTemplate",
    "SmtpHost": "smtp.example.com",
    "SmtpPort": 587,
    "SmtpEnableSsl": true
  },
  "AppUrls": {
    "FrontendBaseUrl": "https://localhost:5173",
    "ConfirmEmailPath": "/confirm-email"
  },
  "Features": {
    "AdminSeed": {
      "Enabled": true,
      "Email": "admin@WebTemplate.com",
      "FirstName": "System",
      "LastName": "Administrator"
    },
    "Cors": {
      "Enabled": true,
      "AllowedOrigins": ["https://localhost:5173"]
    },
    "RateLimiting": {
      "Enabled": true,
      "PermitLimit": 100,
      "WindowSeconds": 60
    }
  },
  "ConnectionStrings": {
    "DefaultConnection": "Server=...;Database=...;"
  },
  "AllowedHosts": "*"
}
```

### User Secrets (Sensitive Data)

**NEVER** commit secrets to source control. Use user secrets for development:

```powershell
# Initialize user secrets
dotnet user-secrets init

# Set required secrets
dotnet user-secrets set "JwtSettings:SecretKey" "Your-Super-Secret-Key-Min-32-Chars"
dotnet user-secrets set "Features:AdminSeed:Password" "Admin123!@#"
dotnet user-secrets set "Email:SmtpUsername" "your-smtp-username"
dotnet user-secrets set "Email:SmtpPassword" "your-smtp-password"
```

## Error Messages

The pattern provides helpful error messages when configuration is missing:

```
System.InvalidOperationException:
JwtSettings:SecretKey is required but not configured.
Please set it in User Secrets or environment variables.

To set in user secrets:
  dotnet user-secrets set "JwtSettings:SecretKey" "your-secret-key"
```

## Benefits

### 1. **Fail Fast**
Configuration errors are caught at startup, not at runtime when a feature is used.

### 2. **Clear Errors**
Error messages include:
- Exact configuration path (e.g., `JwtSettings:SecretKey`)
- What's wrong (missing, invalid format, out of range)
- How to fix it (where to set it, example commands)

### 3. **Type Safety**
Strongly-typed settings classes instead of string-based `configuration["Key"]` calls.

### 4. **Dependency Layering**
- Core services → `ICoreConfiguration`
- Data services → `IDataConfiguration`
- API controllers → `IApiConfiguration`

Each layer only accesses configuration it needs.

### 5. **Single Source of Truth**
One instance of configuration, validated once at startup, shared across the application.

### 6. **Testability**
Easy to mock with `Mock<ICoreConfiguration>` in unit tests.

### 7. **NO FALLBACKS**
Eliminates hidden bugs from fallback values. All required config must be explicit.

## Migration Checklist

When implementing this pattern in a new workspace:

- [ ] Create `ICoreConfiguration` interface in Core project
- [ ] Create `CoreConfiguration` implementation with validation
- [ ] Create `IDataConfiguration` interface in Data project
- [ ] Create `DataConfiguration` implementation extending Core
- [ ] Create `IApiConfiguration` interface in API project
- [ ] Create `ApiConfiguration` implementation extending Data
- [ ] Register as singleton in `Program.cs`
- [ ] Replace all `IOptions<T>` injections with configuration interfaces
- [ ] Update all unit tests to use `Mock<ICoreConfiguration>`
- [ ] Remove all fallback logic (`??`, `||`, `.GetValueOrDefault()`)
- [ ] Add validation for all required configuration
- [ ] Document required user secrets in README
- [ ] Test startup with missing configuration (should fail fast)
- [ ] Test startup with invalid configuration (should fail fast)
- [ ] Verify all unit tests pass

## Common Patterns

### Conditional Features

```csharp
// Feature enabled/disabled based on configuration
if (configuration.Features.Swagger.Enabled)
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
```

### Feature-Specific Settings

```csharp
public class AdminSeedFeatureOptions
{
    public bool Enabled { get; set; } = false;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
}

// Validate only when feature is enabled
if (_options.AdminSeed.Enabled)
{
    if (string.IsNullOrWhiteSpace(_options.AdminSeed.Email))
        throw new InvalidOperationException(
            "Features:AdminSeed:Email is required when Enabled is true.");

    if (string.IsNullOrWhiteSpace(_options.AdminSeed.Password))
        throw new InvalidOperationException(
            "Features:AdminSeed:Password is required when Enabled is true. " +
            "Set in user secrets: dotnet user-secrets set 'Features:AdminSeed:Password' 'YourPassword'");
}
```

### Complex Validation

```csharp
private static void ValidateCorsSettings(CorsFeatureOptions cors)
{
    if (cors.Enabled)
    {
        if (cors.AllowedOrigins == null || cors.AllowedOrigins.Length == 0)
            throw new InvalidOperationException(
                "Features:Cors:AllowedOrigins is required when CORS is enabled.");

        foreach (var origin in cors.AllowedOrigins)
        {
            if (!Uri.TryCreate(origin, UriKind.Absolute, out _))
                throw new InvalidOperationException(
                    $"Invalid CORS origin URL: '{origin}'. Must be a valid absolute URL.");
        }
    }
}
```

## Best Practices

1. **Validate early**: Check configuration in constructors, not on first use
2. **Be specific**: Error messages should include exact configuration path
3. **Provide guidance**: Tell developers how to fix the problem
4. **Use constants**: Define section names as constants (e.g., `JwtSettings.SectionName`)
5. **Test validation**: Write unit tests for validation logic
6. **Document secrets**: List all required user secrets in README
7. **Environment-aware**: Consider different validation for Development vs Production
8. **Immutable**: Don't provide setters after construction
9. **Scoped validation**: Only validate what's needed based on feature flags
10. **NO FALLBACKS**: Never use default values for required configuration

## See Also

- `WebTemplate.Core/Configuration/` - Core configuration interfaces and settings
- `WebTemplate.Data/Configuration/` - Data layer configuration
- `WebTemplate.API/Configuration/` - API layer configuration
- `Program.cs` - Singleton registration
- Unit test examples in `WebTemplate.UnitTests/Services/`

---

**Remember**: Configuration bugs hide in fallbacks. Make configuration explicit, validate early, fail fast!

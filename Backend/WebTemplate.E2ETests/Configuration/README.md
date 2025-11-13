# E2E Test Configuration Pattern

## Overview

The E2E tests follow the **hierarchical configuration singleton pattern** with **NO FALLBACKS** policy, matching the main application's configuration architecture.

## Architecture

```
ITestConfiguration (Interface)
    ↓
TestConfiguration (Singleton Implementation)
    ├── ServerSettings
    ├── AdminCredentials
    └── TestExecutionSettings
```

## Configuration Hierarchy

### 1. ServerSettings
- **BaseUrl**: Auto-detected from launchSettings.json or E2E_BASE_URL env var
- **HealthEndpoint**: Health check endpoint path (default: `/health`)
- **ConnectionTimeoutSeconds**: Timeout for server detection (default: 2)

### 2. AdminCredentials
- **Email**: Hardcoded as `admin@WebTemplate.com`
- **Password**: Hardcoded as `Admin123!@#` (must match server configuration)

### 3. TestExecutionSettings
- **RequestTimeoutSeconds**: HTTP request timeout (default: 30)
- **VerboseLogging**: Enable detailed logging (default: false)
- **MaxRetryAttempts**: Retry attempts for flaky operations (default: 0)

## NO FALLBACKS Policy

Following the project's strict NO FALLBACKS policy:

✅ **Allowed**:
- Reading from environment variables
- Reading from launchSettings.json
- Using explicit defaults documented in code

❌ **NOT Allowed**:
- Hardcoded port scanning as fallback
- Silent fallback values when required config is missing
- Guessing configuration values

### Running E2E Tests

**Minimum steps:**

1. Start the server (BaseUrl auto-detected from launchSettings.json):

```powershell
cd Backend/WebTemplate.API
dotnet run
```

2. Run E2E tests (admin credentials are hardcoded in TestConfiguration):

```powershell
cd Backend/WebTemplate.E2ETests
dotnet test
```

**Optional server URL override:**

```powershell
# Override server URL (default: auto-detected from launchSettings.json)
$env:E2E_BASE_URL = 'https://localhost:7295'
```

## Usage in Tests

```csharp
public class MyE2ETests : E2ETestBase
{
    [Fact]
    public async Task MyTest()
    {
        // Configuration is automatically available via base class
        // Config property provides access to all settings

        // Example: Access server URL
        var serverUrl = Config.Server.BaseUrl;

        // Example: Use admin credentials
        var token = await LoginAsAdminAsync(); // Uses Config.Admin.Email/Password

        // Example: Make authenticated request
        SetAuthToken(token);
        var response = await Client.GetAsync("/api/user");
    }
}
```

## Benefits of This Pattern

1. **Type Safety**: Strongly-typed configuration properties
2. **NO FALLBACKS**: Explicit configuration required - fails fast with clear errors
3. **Singleton**: Single source of truth, initialized once
4. **Testability**: Interface allows mocking in unit tests
5. **Consistency**: Matches main application's configuration pattern
6. **Validation**: Configuration validated at initialization
7. **Clear Errors**: Helpful error messages when configuration is missing

## Error Messages

When required configuration is missing:

```
E2E_ADMIN_PASSWORD environment variable is not set.

To run E2E tests, you must set this environment variable:
  PowerShell: $env:E2E_ADMIN_PASSWORD = 'your-value'
  Persistent: [Environment]::SetEnvironmentVariable('E2E_ADMIN_PASSWORD', 'your-value', 'User')

NO FALLBACKS - explicit configuration required!
```

## Persistent Configuration (Recommended)

Set environment variables permanently for your user account:

```powershell
# PowerShell (requires admin restart of VS2022 after running)
[Environment]::SetEnvironmentVariable('E2E_ADMIN_PASSWORD', 'Admin123!@#', 'User')
[Environment]::SetEnvironmentVariable('E2E_ADMIN_EMAIL', 'admin@WebTemplate.com', 'User')

# Verify
[Environment]::GetEnvironmentVariable('E2E_ADMIN_PASSWORD', 'User')
```

After setting, **restart Visual Studio 2022** completely to pick up the new environment variables.

## See Also

- [CONFIGURATION-PATTERN.md](../../CONFIGURATION-PATTERN.md) - Main application configuration pattern
- [E2ETests README.md](../README.md) - General E2E testing guide

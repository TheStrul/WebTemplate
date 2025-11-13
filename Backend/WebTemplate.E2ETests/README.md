# WebTemplate E2E Tests

End-to-end tests that run against a **real running backend server**. Unlike the integration tests in `WebTemplate.ApiTests` which use in-memory testing, these tests:

- Connect to a real backend via HTTP
- Test deployed environments (local, staging, production)
- Verify the full stack including network, hosting, and external dependencies
- Useful for smoke tests after deployment

## ✨ Smart Server Detection

E2E tests **automatically detect** if the server is running and which port to connect to:

1. **Auto-detection**: Reads `launchSettings.json` and scans common ports (5294, 7295, 5000, 5001)
2. **Health check**: Validates server is responsive before running tests
3. **Clear errors**: If server isn't running, you get a helpful error message with instructions

**No manual configuration needed!** Just start the server and run tests.

## Prerequisites

### 1. Start the Backend Server

```powershell
cd WebTemplate.API
dotnet run
```

The E2E tests will automatically detect the server on any of these ports:
- `http://localhost:5294` (default HTTP)
- `https://localhost:7295` (default HTTPS)
- Custom ports from `launchSettings.json`

### 2. Configure Admin Password

E2E tests require the admin password to authenticate. Set it as an environment variable:

```powershell
# PowerShell
[Environment]::SetEnvironmentVariable('E2E_ADMIN_PASSWORD', 'YourAdminPassword', 'Process')

# Or in your terminal session
$env:E2E_ADMIN_PASSWORD = 'YourAdminPassword'
```

**Note**: This must match the password configured in `Features:AdminSeed:Password` user secrets.

## Running the Tests

### Step 1: Enable Tests

By default, E2E tests are **skipped** to prevent failures in CI/CD when no server is running. To enable them:

- Remove the `Skip` attribute from individual tests, or
- Remove the Skip attribute from all tests in a class

Example:
```csharp
// Before
[Fact(Skip = "Requires running backend server. Remove Skip attribute to run.")]

// After
[Fact]
```

### Step 2: Run Tests

```powershell
# Run all E2E tests (with backend running)
dotnet test WebTemplate.E2ETests

# Run specific test class
dotnet test WebTemplate.E2ETests --filter "FullyQualifiedName~SmokeTests"

# Run with verbose output
dotnet test WebTemplate.E2ETests --logger "console;verbosity=detailed"
```

## Configuration

Configure the backend URL and credentials via environment variables:

```powershell
# Windows (PowerShell) - Optional, only if different from defaults
$env:E2E_BASE_URL = "http://localhost:5294"  # Or https://localhost:7295 if using HTTPS profile
$env:E2E_ADMIN_EMAIL = "admin@WebTemplate.com"
$env:E2E_ADMIN_PASSWORD = "Admin123!"

dotnet test WebTemplate.E2ETests
```

```bash
# Linux/Mac
export E2E_BASE_URL="https://staging.example.com"
export E2E_ADMIN_EMAIL="admin@example.com"
export E2E_ADMIN_PASSWORD="SecurePassword123!"

dotnet test WebTemplate.E2ETests
```

## Test Categories

### 1. Smoke Tests (`SmokeTests.cs`)
Quick tests to verify basic functionality:
- Server is running and responsive
- Admin login works
- User registration works
- Basic authenticated endpoints work

**Use case**: Run after every deployment to catch critical issues

### 2. Auth Flow Tests (`AuthFlowTests.cs`)
Complete authentication scenarios:
- Full registration → login → logout flow
- Token refresh
- Invalid credentials handling
- Duplicate registration prevention

**Use case**: Verify authentication system works end-to-end

## Example: Testing Staging Environment

```powershell
# Set staging environment URL
$env:E2E_BASE_URL = "https://staging.yourapp.com"
$env:E2E_ADMIN_EMAIL = "admin@staging.example.com"
$env:E2E_ADMIN_PASSWORD = "StagingPassword123!"

# Enable tests (remove Skip attributes)
# Then run
dotnet test WebTemplate.E2ETests
```

## Comparison: E2E vs Integration Tests

| Aspect | E2E Tests (this project) | Integration Tests (ApiTests) |
|--------|-------------------------|------------------------------|
| Backend | Real running server | In-memory (WebApplicationFactory) |
| Database | Real database | Test database (isolated) |
| Network | Real HTTP calls | In-process |
| Speed | Slower (~seconds) | Fast (~milliseconds) |
| Setup | Server must be running | Automatic |
| Use Case | Deployment verification | Development & CI/CD |

## Best Practices

1. **Keep E2E tests minimal** - They're slower than integration tests
2. **Focus on critical paths** - Registration, login, key business flows
3. **Run in CI/CD** - After successful deployment to staging/production
4. **Use unique test data** - Tests generate unique emails to avoid conflicts
5. **Clean up test data** - Consider cleanup scripts for staging environments

## Troubleshooting

### Tests fail with connection errors

- Ensure backend is running: `dotnet run` in WebTemplate.API
- Check the URL matches: verify E2E_BASE_URL or default `http://localhost:5294`
- For HTTPS: Use `dotnet run --launch-profile https` and set `$env:E2E_BASE_URL = "https://localhost:7295"`
- Check HTTPS certificate trust: run `dotnet dev-certs https --trust` if using HTTPS

### Authentication fails
- Verify admin credentials in the backend
- Check that admin seeding is enabled in configuration
- Ensure database has the admin user seeded

### Tests timeout
- Increase timeout in `E2ETestBase` constructor (default: 30 seconds)
- Check backend server performance
- Verify database connectivity from the backend

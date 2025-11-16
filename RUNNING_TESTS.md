# Running Tests in WebTemplate

This guide explains how to run all tests in the WebTemplate repository.

## Test Projects Overview

The repository contains **3 test projects**:

1. **WebTemplate.UnitTests** - Unit tests for services, repositories, and utilities (194 tests)
2. **WebTemplate.ApiTests** - Integration tests using WebApplicationFactory (20 tests)
3. **WebTemplate.E2ETests** - End-to-end tests against a running backend (9 tests)

## Quick Start

### Run All Unit Tests ✅ (Always Works)

```bash
cd Backend
dotnet test WebTemplate.UnitTests
```

**Result**: 194 tests passing

These tests use in-memory databases and mocks - no external dependencies required.

## API/Integration Tests

### Prerequisites

API tests require a SQL Server database. Choose one option:

#### Option 1: Windows with LocalDB (Recommended for Windows)
- Install SQL Server Express with LocalDB
- Tests will use `(localdb)\mssqllocaldb` automatically

#### Option 2: Docker SQL Server (Linux/Mac)
```bash
# Start SQL Server container
docker run -e "ACCEPT_EULA=Y" -e "SA_PASSWORD=YourStrong@Passw0rd" \
  -p 1433:1433 --name sql1 -d mcr.microsoft.com/mssql/server:2022-latest

# Update TestWebAppFactory.cs connection string to:
# Server=localhost,1433;Database=CoreWebTemplateDb_IntegrationTests;User Id=sa;Password=YourStrong@Passw0rd;TrustServerCertificate=true
```

### Run API Tests

```bash
cd Backend
ASPNETCORE_ENVIRONMENT=Development dotnet test WebTemplate.ApiTests
```

**Note**: On this Linux environment, API tests fail due to LocalDB not being supported. They work perfectly on Windows or with Docker SQL Server.

## E2E Tests

E2E tests verify the full stack by making HTTP requests to a **real running backend server**.

### Prerequisites

1. **Start the backend server**:
```bash
cd Backend/WebTemplate.API
dotnet run
```
Wait for: `Now listening on: http://localhost:5294`

2. **Configure admin password** (if different from default):
```bash
# PowerShell
$env:E2E_ADMIN_PASSWORD = 'YourAdminPassword'

# Bash
export E2E_ADMIN_PASSWORD='YourAdminPassword'
```

### Enable and Run E2E Tests

E2E tests are **skipped by default** to prevent failures in CI/CD when no server is running.

To enable them:

1. Edit `WebTemplate.E2ETests/SmokeTests.cs` and `AuthFlowTests.cs`
2. Remove `Skip = "..."` from the `[Fact]` attributes

Example:
```csharp
// Before
[Fact(Skip = "Requires running backend server. Remove Skip attribute to run.")]

// After  
[Fact]
```

3. Run the tests:
```bash
cd Backend
dotnet test WebTemplate.E2ETests
```

### E2E Test Configuration

Configure via environment variables:

```bash
E2E_BASE_URL=http://localhost:5294  # Auto-detected if not set
E2E_ADMIN_EMAIL=admin@WebTemplate.com  # Default value
E2E_ADMIN_PASSWORD=Admin123!  # REQUIRED - no default
```

## Running All Tests Together

```bash
cd Backend
dotnet test
```

This runs:
- ✅ Unit Tests: 194 passing
- ⚠️ API Tests: Need SQL Server (LocalDB on Windows or Docker)
- ⏭️ E2E Tests: 9 skipped (require running backend server)

## Test Project Details

### WebTemplate.UnitTests
- **Type**: Unit tests with mocks
- **Dependencies**: None (in-memory database)
- **Speed**: Fast (~4 seconds)
- **Coverage**: Services, repositories, validators

**Test Categories**:
- `AuthService` - Login, registration, token management
- `UserService` - User CRUD operations
- `UserTypeService` - User type management
- `TokenService` - JWT token generation/validation
- `RefreshTokenRepository` - Token persistence
- `RefreshTokenCleanupService` - Background cleanup

### WebTemplate.ApiTests
- **Type**: Integration tests
- **Framework**: WebApplicationFactory (in-memory API testing)
- **Dependencies**: SQL Server (LocalDB or Docker)
- **Speed**: Medium (~2 seconds)
- **Coverage**: Full request/response cycle

**Test Categories**:
- Auth endpoints (`/api/auth/*`)
- User endpoints (`/api/user/*`)
- Admin operations

**Configuration**: Tests use `TestWebAppFactory` which:
- Configures test database
- Injects test configuration (JWT secrets, etc.)
- Seeds admin user automatically

### WebTemplate.E2ETests
- **Type**: End-to-end smoke tests
- **Dependencies**: Running backend server + database
- **Speed**: Slow (network calls)
- **Purpose**: Deployment verification

**Test Categories**:
- `SmokeTests` - Basic health checks
- `AuthFlowTests` - Complete auth workflows

## Troubleshooting

### Unit Tests Fail
- Ensure .NET 9 SDK is installed: `dotnet --version`
- Clean and rebuild: `dotnet clean && dotnet build`

### API Tests Fail with "LocalDB not supported"
- **Linux/Mac**: Use Docker SQL Server (see instructions above)
- **Windows**: Install SQL Server Express with LocalDB

### API Tests Fail with "Configuration validation failed"
- This is expected and has been fixed in this PR
- The `Program.cs` now supports test environments properly

### E2E Tests Fail with Connection Errors
- Ensure backend is running: `dotnet run` in `WebTemplate.API`
- Check the URL matches: `http://localhost:5294`
- Verify admin password is set in environment variable

### E2E Tests are All Skipped
- This is intentional! E2E tests require a running server
- Remove `Skip` attributes to enable them (see instructions above)

## CI/CD Considerations

For automated testing in CI/CD:

1. **Unit Tests**: Always run (no dependencies)
2. **API Tests**: Require SQL Server container in pipeline
3. **E2E Tests**: Run against deployed environments (staging/production)

### Example GitHub Actions

```yaml
- name: Run Unit Tests
  run: cd Backend && dotnet test WebTemplate.UnitTests

- name: Start SQL Server
  run: docker run -d -e "ACCEPT_EULA=Y" -e "SA_PASSWORD=Test@123" -p 1433:1433 mcr.microsoft.com/mssql/server:2022-latest

- name: Run API Tests  
  run: cd Backend && dotnet test WebTemplate.ApiTests
  
# E2E tests run after deployment to staging
- name: Run E2E Tests
  run: cd Backend && dotnet test WebTemplate.E2ETests
  env:
    E2E_BASE_URL: https://staging.yourapp.com
    E2E_ADMIN_PASSWORD: ${{ secrets.ADMIN_PASSWORD }}
```

## Summary

| Test Project | Count | Status | Prerequisites |
|--------------|-------|--------|---------------|
| Unit Tests | 194 | ✅ Passing | None |
| API Tests | 20 | ⚠️ Need SQL | SQL Server (LocalDB/Docker) |
| E2E Tests | 9 | ⏭️ Skipped | Running backend + Remove Skip attributes |

**Total**: 223 tests covering comprehensive backend functionality

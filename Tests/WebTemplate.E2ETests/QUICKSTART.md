# Quick Start: Running E2E Tests

## Option 1: Run Against Local Backend (Recommended for first try)

### Step 1: Start the Backend

Open a terminal and run:

```powershell
cd WebTemplate.API
dotnet run
```

Wait for: `Now listening on: http://localhost:5294`

**Note:** To use HTTPS instead, run `dotnet run --launch-profile https` and set `$env:E2E_BASE_URL = "https://localhost:7295"` before running tests.

### Step 2: Enable a Test
In `SmokeTests.cs`, remove the `Skip` attribute from one test:
```csharp
// Change this:
[Fact(Skip = "Requires running backend server. Remove Skip attribute to run.")]

// To this:
[Fact]
```

### Step 3: Run the Test
In a new terminal:
```powershell
dotnet test WebTemplate.E2ETests
```

## Option 2: Run Against Staging/Production

```powershell
# Set environment variables
$env:E2E_BASE_URL = "https://your-staging-url.com"
$env:E2E_ADMIN_EMAIL = "admin@example.com"
$env:E2E_ADMIN_PASSWORD = "YourPassword"

# Remove Skip attributes from tests
# Then run
dotnet test WebTemplate.E2ETests
```

## What You'll See

✅ **If backend is running**: Tests execute and verify endpoints
❌ **If backend is NOT running**: Connection errors (expected)
⏭️ **With Skip attribute**: Tests are skipped (default behavior)

## Example Output (Success)
```
Starting test execution, please wait...
✓ Passed Backend_IsRunning_AndResponsive [2s]
✓ Passed Auth_AdminLogin_Succeeds [1s]

Test Run Successful.
Total tests: 2, Passed: 2, Failed: 0, Skipped: 0
```

See README.md for full documentation.

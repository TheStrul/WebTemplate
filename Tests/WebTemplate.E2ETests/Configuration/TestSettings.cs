namespace WebTemplate.E2ETests.Configuration;

/// <summary>
/// Server connection settings for E2E tests.
/// NO FALLBACKS - explicit configuration required.
/// </summary>
public class ServerSettings
{
    /// <summary>
    /// Base URL of the server to test against.
    /// Priority: E2E_BASE_URL env var → launchSettings.json
    /// NO FALLBACKS to hardcoded ports.
    /// </summary>
    public string BaseUrl { get; set; } = string.Empty;

    /// <summary>
    /// Health check endpoint path
    /// </summary>
    public string HealthEndpoint { get; set; } = "/health";

    /// <summary>
    /// Connection timeout in seconds
    /// </summary>
    public int ConnectionTimeoutSeconds { get; set; } = 2;
}

/// <summary>
/// Admin credentials for E2E test authentication.
/// NO FALLBACKS - all credentials must be explicitly configured.
/// </summary>
public class AdminCredentials
{
    /// <summary>
    /// Admin email address.
    /// Must be set via E2E_ADMIN_EMAIL environment variable.
    /// NO FALLBACKS - fails if not configured.
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Admin password.
    /// Must be set via E2E_ADMIN_PASSWORD environment variable.
    /// NO FALLBACKS - fails if not configured.
    /// </summary>
    public string Password { get; set; } = string.Empty;
}

/// <summary>
/// Test execution settings controlling test behavior.
/// </summary>
public class TestExecutionSettings
{
    /// <summary>
    /// HTTP request timeout in seconds for API calls during tests
    /// </summary>
    public int RequestTimeoutSeconds { get; set; } = 30;

    /// <summary>
    /// Whether to log detailed request/response information
    /// </summary>
    public bool VerboseLogging { get; set; } = false;

    /// <summary>
    /// Maximum number of retry attempts for flaky operations
    /// </summary>
    public int MaxRetryAttempts { get; set; } = 0;
}

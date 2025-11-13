namespace WebTemplate.E2ETests.Configuration;

/// <summary>
/// Test configuration interface providing access to all configuration objects
/// needed for E2E tests. Follows the hierarchical configuration singleton pattern.
/// NO FALLBACKS - all values must be explicitly configured.
/// </summary>
public interface ITestConfiguration
{
    /// <summary>
    /// Server connection settings for E2E tests
    /// </summary>
    ServerSettings Server { get; }

    /// <summary>
    /// Admin credentials for E2E test authentication
    /// </summary>
    AdminCredentials Admin { get; }

    /// <summary>
    /// Test execution settings (timeouts, retries, etc.)
    /// </summary>
    TestExecutionSettings Execution { get; }
}

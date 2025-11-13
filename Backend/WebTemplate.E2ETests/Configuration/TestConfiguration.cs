using Microsoft.Extensions.Configuration;
using WebTemplate.Core.Configuration;

namespace WebTemplate.E2ETests.Configuration;

/// <summary>
/// E2E Test configuration that loads THE SAME appsettings.Local.json used by the API.
/// ONE CONFIGURATION TO RULE THEM ALL - API and E2E tests share the same config file!
/// NO FALLBACKS - validates all required configuration on initialization.
/// </summary>
public class TestConfiguration : ITestConfiguration
{
    private static TestConfiguration? _instance;
    private static readonly object _lock = new();

    /// <summary>
    /// The master ApplicationConfiguration loaded from appsettings.Local.json
    /// </summary>
    public ApplicationConfiguration AppConfig { get; }

    // Convenience properties for backward compatibility
    public ServerSettings Server => new()
    {
        BaseUrl = AppConfig.Server.Url,
        HealthEndpoint = AppConfig.Server.HealthEndpoint,
        ConnectionTimeoutSeconds = AppConfig.Server.ConnectionTimeoutSeconds
    };

    public AdminCredentials Admin => new()
    {
        Email = AppConfig.Features.AdminSeed.Email,
        Password = AppConfig.Features.AdminSeed.Password
    };

    public TestExecutionSettings Execution { get; }

    private TestConfiguration()
    {
        // Find and load THE SAME appsettings.Local.json that the API uses
        var apiProjectPath = FindApiProjectDirectory();
        var configFilePath = Path.Combine(apiProjectPath, "appsettings.Local.json");

        if (!File.Exists(configFilePath))
        {
            throw new InvalidOperationException(
                $"Cannot find appsettings.Local.json at: {configFilePath}\n\n" +
                "E2E tests require the API's appsettings.Local.json file to be present.\n" +
                "Ensure the API project is configured correctly.");
        }

        // Build configuration from the same file
        var configuration = new ConfigurationBuilder()
            .AddJsonFile(configFilePath, optional: false, reloadOnChange: false)
            .Build();

        // Load ApplicationConfiguration - same validation as API!
        AppConfig = new ApplicationConfiguration(configuration);

        // Initialize Execution settings (test-specific, not in shared config)
        Execution = new TestExecutionSettings
        {
            RequestTimeoutSeconds = 30,
            VerboseLogging = false,
            MaxRetryAttempts = 0
        };

        // Log configuration on initialization
        Console.WriteLine($"[E2E Tests] Configuration loaded from: {configFilePath}");
        Console.WriteLine($"  Server URL: {AppConfig.Server.Url}");
        Console.WriteLine($"  Admin Email: {AppConfig.Features.AdminSeed.Email}");
        Console.WriteLine($"  Request Timeout: {Execution.RequestTimeoutSeconds}s");
    }

    /// <summary>
    /// Finds the WebTemplate.API project directory by searching up the directory tree.
    /// </summary>
    private static string FindApiProjectDirectory()
    {
        var currentDir = new DirectoryInfo(Directory.GetCurrentDirectory());

        // Search up to 10 levels
        for (int i = 0; i < 10 && currentDir != null; i++)
        {
            // Look for WebTemplate.API directory
            var apiPath = Path.Combine(currentDir.FullName, "WebTemplate.API");
            if (Directory.Exists(apiPath))
                return apiPath;

            // Also check sibling directory (when running from E2ETests folder)
            var siblingPath = Path.Combine(currentDir.FullName, "..", "WebTemplate.API");
            var fullSiblingPath = Path.GetFullPath(siblingPath);
            if (Directory.Exists(fullSiblingPath))
                return fullSiblingPath;

            currentDir = currentDir.Parent;
        }

        throw new InvalidOperationException(
            "Cannot find WebTemplate.API project directory. " +
            "Ensure E2E tests are run from within the solution structure.");
    }

    /// <summary>
    /// Gets the singleton instance of TestConfiguration.
    /// Thread-safe lazy initialization with validation.
    /// </summary>
    public static TestConfiguration Instance
    {
        get
        {
            if (_instance != null)
                return _instance;

            lock (_lock)
            {
                _instance ??= new TestConfiguration();
                return _instance;
            }
        }
    }
}

using WebTemplate.Setup.Models;
using WebTemplate.Setup.Services;

namespace WebTemplate.Setup;

/// <summary>
/// Example usage of the WebTemplate.Setup backend services
/// This can be used for testing before the UI is built
/// </summary>
public static class UsageExamples
{
    /// <summary>
    /// Example: Create and save a configuration
    /// </summary>
    public static async Task CreateAndSaveConfiguration()
    {
        var config = new WorkspaceConfiguration
        {
            ConfigurationId = "my-test-project",
            ConfigurationName = "My Test Project",
            Description = "A test configuration for demonstration",

            Project = new ProjectSettings
            {
                ProjectName = "MyTestApp",
                TargetPath = @"C:\Projects\MyTestApp",
                CompanyName = "My Company",
                AuthorName = "John Doe",
                InitializeGit = true,
                RunValidation = true
            },

            Features = new FeaturesConfiguration
            {
                Swagger = new SwaggerFeature { Enabled = true },
                Cors = new CorsFeature
                {
                    Enabled = true,
                    AllowedOrigins = new[] { "http://localhost:3000" }
                },
                RateLimiting = new RateLimitingFeature { Enabled = false },
                SecurityHeaders = new SecurityHeadersFeature { Enabled = true },
                HealthChecks = new HealthChecksFeature { Enabled = true },
                IdentityAuth = new IdentityAuthFeature { Enabled = true },
                RefreshTokens = new RefreshTokensFeature { Enabled = true },
                AdminSeed = new AdminSeedFeature
                {
                    Enabled = true,
                    Email = "admin@myapp.com",
                    Password = "Admin123!",
                    FirstName = "Admin",
                    LastName = "User"
                },
                ExceptionHandling = new ExceptionHandlingFeature { Enabled = true },
                Serilog = new SerilogFeature { Enabled = false }
            },

            Secrets = new SecretsConfiguration
            {
                Strategy = SecretsStrategy.UserSecrets,
                UserSecretsValues = new Dictionary<string, string>
                {
                    ["CustomSecret"] = "CustomValue"
                }
            },

            Database = new DatabaseConfiguration
            {
                ConnectionString = "Server=localhost;Database=MyTestApp;Integrated Security=true;TrustServerCertificate=true;",
                ExecuteInitScript = true,
                TestConnection = false, // Set to true to test before generation
                CreateDatabaseIfNotExists = true
            },

            Server = new ServerConfiguration
            {
                Url = "http://localhost:5000",
                HttpsUrl = "https://localhost:5001",
                ConnectionTimeoutSeconds = 30,
                UseHttpsRedirection = true
            },

            Email = new EmailConfiguration
            {
                Enabled = false // Set to true and configure if needed
            },

            Auth = new AuthConfiguration
            {
                SecretKey = "Development-Secret-Key-At-Least-32-Characters-Long",
                Issuer = "MyTestAppAPI",
                Audience = "MyTestAppClient",
                ExpirationMinutes = 60,
                RefreshTokenExpirationDays = 7,
                FrontendUrl = "http://localhost:3000"
            }
        };

        // Save the configuration
        var persistenceService = new ConfigurationPersistenceService();
        var result = await persistenceService.SaveConfigurationAsync(config);

        if (result.Success)
        {
            Console.WriteLine($"✓ Configuration saved: {result.Message}");
        }
        else
        {
            Console.WriteLine($"✗ Failed to save: {result.Message}");
        }
    }

    /// <summary>
    /// Example: Load a configuration
    /// </summary>
    public static async Task LoadConfiguration()
    {
        var persistenceService = new ConfigurationPersistenceService();
        var result = await persistenceService.LoadConfigurationAsync("my-test-project");

        if (result.Success && result.Config != null)
        {
            Console.WriteLine($"✓ Configuration loaded: {result.Config.ConfigurationName}");
            Console.WriteLine($"  Project: {result.Config.Project.ProjectName}");
            Console.WriteLine($"  Features: {result.Config.Features.Swagger.Enabled} features enabled");
        }
        else
        {
            Console.WriteLine($"✗ Failed to load: {result.Message}");
        }
    }

    /// <summary>
    /// Example: List all configurations
    /// </summary>
    public static void ListConfigurations()
    {
        var persistenceService = new ConfigurationPersistenceService();
        var configurations = persistenceService.ListConfigurations();

        Console.WriteLine($"Found {configurations.Count} configuration(s):");
        foreach (var config in configurations)
        {
            Console.WriteLine($"  - {config.ConfigurationName} ({config.ConfigurationId})");
            Console.WriteLine($"    Project: {config.ProjectName}");
            Console.WriteLine($"    Modified: {config.ModifiedAt:yyyy-MM-dd HH:mm}");
        }
    }

    /// <summary>
    /// Example: Generate a project from configuration
    /// REQUIRES: Dependency Injection setup with ITemplateEngine registered
    /// </summary>
    public static async Task GenerateProject(ProjectGenerationService generationService)
    {
        // Load existing configuration
        var persistenceService = new ConfigurationPersistenceService();
        var loadResult = await persistenceService.LoadConfigurationAsync("my-test-project");

        if (!loadResult.Success || loadResult.Config == null)
        {
            Console.WriteLine($"✗ Failed to load configuration: {loadResult.Message}");
            return;
        }

        var config = loadResult.Config;

        // Create progress reporter
        var progress = new Progress<string>(message =>
        {
            Console.WriteLine($"[Progress] {message}");
        });

        // Generate project using injected service
        var result = await generationService.GenerateProjectAsync(config, progress);

        if (result.Success)
        {
            Console.WriteLine($"✓ Project generated successfully!");
            Console.WriteLine($"  Location: {result.GeneratedPath}");
        }
        else
        {
            Console.WriteLine($"✗ Generation failed: {result.Message}");
        }
    }

    /// <summary>
    /// Example: Test database connection
    /// </summary>
    public static async Task TestDatabaseConnection()
    {
        var connectionString = "Server=localhost;Database=MyTestApp;Integrated Security=true;TrustServerCertificate=true;";

        var dbService = new DatabaseService();

        // Validate connection string format
        var validation = dbService.ValidateConnectionString(connectionString);
        if (!validation.IsValid)
        {
            Console.WriteLine($"✗ Invalid connection string: {validation.Message}");
            return;
        }

        // Test connection
        var result = await dbService.TestConnectionAsync(connectionString);
        if (result.Success)
        {
            Console.WriteLine($"✓ Database connection successful");
        }
        else
        {
            Console.WriteLine($"✗ Connection failed: {result.Message}");
        }
    }

    /// <summary>
    /// Example: Validate a configuration
    /// </summary>
    public static void ValidateConfiguration()
    {
        var config = new WorkspaceConfiguration
        {
            // Incomplete configuration for demonstration
            ConfigurationId = "test",
            ConfigurationName = "Test Config"
            // Missing required fields
        };

        var validation = config.Validate();
        if (validation.IsValid)
        {
            Console.WriteLine("✓ Configuration is valid");
        }
        else
        {
            Console.WriteLine("✗ Configuration validation failed:");
            foreach (var error in validation.Errors)
            {
                Console.WriteLine($"  - {error}");
            }
        }
    }

    /// <summary>
    /// Example: Run all tests
    /// REQUIRES: Dependency Injection setup with all required services
    /// </summary>
    public static async Task RunAllExamples(ProjectGenerationService? generationService = null)
    {
        Console.WriteLine("=== WebTemplate.Setup Backend Examples ===\n");

        Console.WriteLine("1. Creating and saving configuration...");
        await CreateAndSaveConfiguration();
        Console.WriteLine();

        Console.WriteLine("2. Listing configurations...");
        ListConfigurations();
        Console.WriteLine();

        Console.WriteLine("3. Loading configuration...");
        await LoadConfiguration();
        Console.WriteLine();

        Console.WriteLine("4. Validating configuration...");
        ValidateConfiguration();
        Console.WriteLine();

        Console.WriteLine("5. Testing database connection...");
        await TestDatabaseConnection();
        Console.WriteLine();

        if (generationService != null)
        {
            Console.WriteLine("6. Generating project...");
            await GenerateProject(generationService);
        }
        else
        {
            Console.WriteLine("6. Generating project (skipped - requires DI setup)...");
            Console.WriteLine("   (Pass ProjectGenerationService to RunAllExamples to enable)");
        }
        Console.WriteLine();

        Console.WriteLine("=== All examples completed ===");
    }
}

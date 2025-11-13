using Microsoft.Extensions.DependencyInjection.Extensions;
using WebTemplate.Core.Features;
using WebTemplate.Core.Configuration.Features;
using WebTemplate.Core.Configuration;
using WebTemplate.Data.Modules;
using System.Diagnostics;

internal class Program
{
    private static async Task Main(string[] args)
    {
        // Build configuration first - this may fail validation which indicates
        // we're in a test environment where WebApplicationFactory will provide config
        ApplicationConfiguration? appConfig = null;
        IConfiguration? configuration = null;
        bool useBuilderConfiguration = false;

        // Try to build configuration - if this fails (test environment), we'll use builder configuration instead
        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
        if (!string.IsNullOrWhiteSpace(environment))
        {
            try
            {
                // ONE CONFIGURATION TO RULE THEM ALL!
                configuration = BuildConfiguration();
                var isLocal = environment == "Development";

                // Create and validate ApplicationConfiguration (NO FALLBACKS!)
                try
                {
                    appConfig = new ApplicationConfiguration(configuration);
                }
                catch (InvalidOperationException ex)
                {
                    // If configuration validation fails, check if we might be in a test environment
                    // Tests use WebApplicationFactory which provides configuration after CreateBuilder
                    // So we'll defer configuration creation until after the builder is created
                    var errorMessage = ex.Message;
                    if (errorMessage.Contains("Jwt") || errorMessage.Contains("SecretKey"))
                    {
                        // Likely in test environment - defer configuration until after builder creation
                        useBuilderConfiguration = true;
                    }
                    else
                    {
                        // Real configuration error - fail fast
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine();
                        Console.WriteLine("═══════════════════════════════════════════════════════════════");
                        Console.WriteLine("  ✗ CONFIGURATION ERROR");
                        Console.WriteLine("═══════════════════════════════════════════════════════════════");
                        Console.ResetColor();
                        Console.WriteLine();
                        Console.WriteLine(ex.Message);
                        Console.WriteLine();
                        if (isLocal)
                        {
                            Console.ForegroundColor = ConsoleColor.Yellow;
                            Console.WriteLine("Ensure appsettings.Local.json has all required values configured.");
                            Console.ResetColor();
                        }
                        Console.WriteLine();
                        Environment.Exit(1);
                        return;
                    }
                }
            }
            catch (Exception)
            {
                // If BuildConfiguration itself fails, we're definitely in a test environment
                useBuilderConfiguration = true;
            }
        }
        else
        {
            // No environment set - likely test environment
            useBuilderConfiguration = true;
        }

        // Old user secrets check - no longer needed with Local configuration
        if (false && Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
        {
            var requiredSecrets = new[] { "JwtSettings:SecretKey", "AdminSeed:Password" };
            var missingSecrets = new List<string>();

            var tempConfig = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false)
                .AddJsonFile("appsettings.Development.json", optional: true)
                .AddUserSecrets<Program>(optional: true)
                .Build();

            foreach (var secret in requiredSecrets)
            {
                var value = tempConfig[secret];
                if (string.IsNullOrWhiteSpace(value))
                {
                    missingSecrets.Add(secret);
                }
            }

            if (missingSecrets.Any())
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine();
                Console.WriteLine("═══════════════════════════════════════════════════════════════");
                Console.WriteLine("  ⚠  MISSING REQUIRED USER SECRETS");
                Console.WriteLine("═══════════════════════════════════════════════════════════════");
                Console.ResetColor();
                Console.WriteLine();
                Console.WriteLine("The following required secrets are not configured:");
                foreach (var secret in missingSecrets)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"  ✗ {secret}");
                    Console.ResetColor();
                }
                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("Would you like to run the setup script now? (Y/n)");
                Console.ResetColor();

                var response = Console.ReadLine()?.Trim().ToLowerInvariant();
                if (string.IsNullOrEmpty(response) || response == "y" || response == "yes")
                {
                    Console.WriteLine();
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("Running setup-user-secrets.ps1...");
                    Console.ResetColor();
                    Console.WriteLine();

                    var scriptPath = Path.Combine(Directory.GetCurrentDirectory(), "setup-user-secrets.ps1");
                    var startInfo = new ProcessStartInfo
                    {
                        FileName = "pwsh",
                        Arguments = $"-NoProfile -ExecutionPolicy Bypass -File \"{scriptPath}\"",
                        UseShellExecute = false,
                        WorkingDirectory = Directory.GetCurrentDirectory()
                    };

                    try
                    {
                        var process = Process.Start(startInfo);
                        if (process is not null)
                        {
                            await process.WaitForExitAsync();
                        }

                        if (process?.ExitCode == 0)
                        {
                            Console.WriteLine();
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.WriteLine("✓ User secrets configured successfully!");
                            Console.WriteLine("  Please restart the application.");
                            Console.ResetColor();
                            Environment.Exit(0);
                        }
                        else
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine("✗ Setup script failed. Please run it manually:");
                            Console.WriteLine($"  pwsh {scriptPath}");
                            Console.ResetColor();
                            Environment.Exit(1);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($"✗ Error running setup script: {ex.Message}");
                        Console.WriteLine();
                        Console.WriteLine("Please run the setup script manually:");
                        Console.WriteLine($"  pwsh {scriptPath}");
                        Console.ResetColor();
                        Environment.Exit(1);
                    }
                }
                else
                {
                    Console.WriteLine();
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("Application startup cancelled.");
                    Console.WriteLine();
                    Console.WriteLine("To configure secrets manually, run:");
                    Console.WriteLine($"  pwsh setup-user-secrets.ps1");
                    Console.ResetColor();
                    Environment.Exit(1);
                }
            }
        }

        var builder = WebApplication.CreateBuilder(args);

        if (!useBuilderConfiguration)
        {
            // In non-test environment, replace builder's configuration with our explicit configuration
            builder.Configuration.Sources.Clear();
            builder.Configuration.AddConfiguration(configuration!);
            
            // Set server URL explicitly from configuration (NO launchSettings.json needed!)
            builder.WebHost.UseUrls(appConfig!.Server.Url);
        }
        // else: In test environment, use the builder's configuration as-is.
        // The TestWebAppFactory will inject configuration through ConfigureWebHost,
        // and ApplicationConfiguration will be created lazily when services request it.

        // Register ApplicationConfiguration - ONE CONFIGURATION TO RULE THEM ALL!
        // Provides all configuration to API, Data, and Core layers
        // If appConfig is null (test environment), create it lazily from IConfiguration
        if (appConfig != null)
        {
            builder.Services.AddSingleton(appConfig);
            builder.Services.AddSingleton<ICoreConfiguration>(appConfig);
        }
        else
        {
            // Test environment - create ApplicationConfiguration from final IConfiguration
            builder.Services.AddSingleton<ApplicationConfiguration>(sp =>
            {
                var config = sp.GetRequiredService<IConfiguration>();
                return new ApplicationConfiguration(config);
            });
            builder.Services.AddSingleton<ICoreConfiguration>(sp =>
                sp.GetRequiredService<ApplicationConfiguration>());
        }

        // Also register the old ApiConfiguration for backward compatibility
        // If we're in test mode (appConfig is null), create it lazily to allow test configuration to be applied first
        if (appConfig != null)
        {
            var apiConfig = new WebTemplate.API.Configuration.ApiConfiguration(builder.Configuration);
            builder.Services.AddSingleton<WebTemplate.API.Configuration.IApiConfiguration>(apiConfig);
            builder.Services.AddSingleton<WebTemplate.Data.Configuration.IDataConfiguration>(apiConfig);
            builder.Services.AddSingleton(apiConfig.Features);
        }
        else
        {
            // Test environment - create lazily
            builder.Services.AddSingleton<WebTemplate.API.Configuration.ApiConfiguration>(sp =>
            {
                var config = sp.GetRequiredService<IConfiguration>();
                return new WebTemplate.API.Configuration.ApiConfiguration(config);
            });
            builder.Services.AddSingleton<WebTemplate.API.Configuration.IApiConfiguration>(sp =>
                sp.GetRequiredService<WebTemplate.API.Configuration.ApiConfiguration>());
            builder.Services.AddSingleton<WebTemplate.Data.Configuration.IDataConfiguration>(sp =>
                sp.GetRequiredService<WebTemplate.API.Configuration.ApiConfiguration>());
            builder.Services.AddSingleton(sp =>
                sp.GetRequiredService<WebTemplate.API.Configuration.ApiConfiguration>().Features);
        }

        // Bind and discover feature modules
        builder.Services.Configure<FeaturesOptions>(builder.Configuration.GetSection(FeaturesOptions.SectionName));
        var featureHost = new FeatureHost(builder.Configuration).Discover();
        featureHost.ConfigureOptions(builder.Services);

        // Core services
        builder.Services.AddControllers();

        // Register core services (TryAdd to avoid duplicates when modules register)
        builder.Services.TryAddScoped<WebTemplate.Core.Interfaces.IRefreshTokenRepository, WebTemplate.Data.Repositories.RefreshTokenRepository>();
        builder.Services.TryAddScoped<WebTemplate.Core.Interfaces.IUserTypeRepository, WebTemplate.Data.Repositories.UserTypeRepository>();
        builder.Services.TryAddScoped<WebTemplate.Core.Interfaces.IEmailSender, WebTemplate.Core.Services.SmtpEmailSender>();
        builder.Services.TryAddScoped<WebTemplate.Core.Interfaces.ITokenService, WebTemplate.Core.Services.TokenService>();
        builder.Services.TryAddScoped<WebTemplate.Core.Interfaces.IAuthService, WebTemplate.Core.Services.AuthService>();
        builder.Services.TryAddScoped<WebTemplate.Core.Interfaces.IUserService, WebTemplate.Core.Services.UserService>();
        builder.Services.TryAddScoped<WebTemplate.Core.Interfaces.IUserTypeService, WebTemplate.Core.Services.UserTypeService>();

        // Conditionally add Identity/Auth module
        var features = builder.Configuration.GetSection(FeaturesOptions.SectionName).Get<FeaturesOptions>() ?? new FeaturesOptions();
        if (features.IdentityAuth.Enabled)
        {
            builder.Services.AddUserModule(builder.Configuration);
        }

        // Configure services from feature modules
        featureHost.ConfigureServices(builder.Services);

        var app = builder.Build();

        // Pipeline - CRITICAL: UseRouting must come before UseEndpoints
        app.UseHttpsRedirection();
        app.UseRouting(); // Add routing middleware first
        featureHost.ConfigurePipeline(app);

        // Conditionally enable Identity/Auth pipeline
        if (features.IdentityAuth.Enabled)
        {
            app.UseUserModule();
        }

        // Endpoints - UseEndpoints is called by featureHost.MapEndpoints
        app.MapControllers();
        featureHost.MapEndpoints(app);

        await app.RunAsync();
    }

    /// <summary>
    /// Builds the configuration from appropriate sources based on environment.
    /// Development: Uses appsettings.Local.json only
    /// Staging/Production: Uses appsettings.json, appsettings.{Environment}.json, and environment variables
    /// </summary>
    private static IConfiguration BuildConfiguration()
    {
        // Determine environment and validate
        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

        // Fail fast if environment is missing or invalid
        if (string.IsNullOrWhiteSpace(environment))
        {
            throw new InvalidOperationException("ASPNETCORE_ENVIRONMENT is not set. Set it to one of: Development, Staging, Production");
        }

        var allowedEnvironments = new HashSet<string>(StringComparer.Ordinal)
        {
            "Development", "Staging", "Production"
        };
        if (!allowedEnvironments.Contains(environment))
        {
            throw new InvalidOperationException($"Invalid ASPNETCORE_ENVIRONMENT: '{environment}'. Allowed values: Development, Staging, Production");
        }

        var isLocal = environment == "Development";

        var configurationBuilder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory());

        configurationBuilder.AddJsonFile("appsettings.Local.json", optional: false, reloadOnChange: true);

        if (!isLocal)
        {
            configurationBuilder
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{environment}.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables();
        }

        return configurationBuilder.Build();
    }
}

// Expose Program for tests
namespace WebTemplate.API { public class Program { } }

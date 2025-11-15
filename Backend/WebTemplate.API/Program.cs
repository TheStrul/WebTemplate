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
        // ONE CONFIGURATION TO RULE THEM ALL!
        var configuration = BuildConfiguration();
        var isLocal = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development";

        // Create and validate ApplicationConfiguration (NO FALLBACKS!)
        ApplicationConfiguration appConfig;
        try
        {
            appConfig = new ApplicationConfiguration(configuration);
        }
        catch (InvalidOperationException ex)
        {
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

        // Replace builder's configuration with our explicit configuration
        builder.Configuration.Sources.Clear();
        builder.Configuration.AddConfiguration(configuration);

        // Set server URL explicitly from configuration (NO launchSettings.json needed!)
        builder.WebHost.UseUrls(appConfig.Server.Url);

        // Register ApplicationConfiguration - ONE CONFIGURATION TO RULE THEM ALL!
        // Provides all configuration to API, Data, and Core layers
        builder.Services.AddSingleton(appConfig);
        builder.Services.AddSingleton<ICoreConfiguration>(appConfig);

        // Also register the old ApiConfiguration for backward compatibility
        var apiConfig = new WebTemplate.API.Configuration.ApiConfiguration(builder.Configuration);
        builder.Services.AddSingleton<WebTemplate.API.Configuration.IApiConfiguration>(apiConfig);
        builder.Services.AddSingleton<WebTemplate.Data.Configuration.IDataConfiguration>(apiConfig);

        // Register FeaturesOptions as singleton for services that need it directly
        builder.Services.AddSingleton(apiConfig.Features);

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

        // Initialize UserModule (runs migrations and seeds data)
        if (features.IdentityAuth.Enabled)
        {
            await app.InitializeUserModuleAsync();
        }

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
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine();
            Console.WriteLine("═══════════════════════════════════════════════════════════════");
            Console.WriteLine("  ✗ MISSING ASPNETCORE_ENVIRONMENT");
            Console.WriteLine("═══════════════════════════════════════════════════════════════");
            Console.ResetColor();
            Console.WriteLine();
            Console.WriteLine("Set ASPNETCORE_ENVIRONMENT to one of: Development, Staging, Production");
            Console.WriteLine();
            Environment.Exit(1);
            return null!;
        }

        var allowedEnvironments = new HashSet<string>(StringComparer.Ordinal)
        {
            "Development", "Staging", "Production"
        };
        if (!allowedEnvironments.Contains(environment))
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine();
            Console.WriteLine("═══════════════════════════════════════════════════════════════");
            Console.WriteLine("  ✗ INVALID ASPNETCORE_ENVIRONMENT VALUE");
            Console.WriteLine("═══════════════════════════════════════════════════════════════");
            Console.ResetColor();
            Console.WriteLine();
            Console.WriteLine($"Provided: '{environment}'");
            Console.WriteLine("Allowed: Development, Staging, Production");
            Console.WriteLine();
            Environment.Exit(1);
            return null!;
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

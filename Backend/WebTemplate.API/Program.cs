using Microsoft.Extensions.DependencyInjection.Extensions;
using WebTemplate.Core.Features;
using WebTemplate.Core.Configuration.Features;
using WebTemplate.Core.Configuration;
using WebTemplate.Data.Modules;
using System.Diagnostics;
using System.Reflection;

public class Program
{
    private static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Clear default sources and add configuration from explicit sources.
        // In tests, TestWebAppFactory will add its own sources.
        builder.Configuration.Sources.Clear();
        BuildConfiguration(builder.Configuration, builder.Environment);

        // Detect testhost early
        bool IsRunningUnderTests()
        {
            var processName = Process.GetCurrentProcess().ProcessName?.ToLowerInvariant() ?? string.Empty;
            if (processName.Contains("testhost") || processName.Contains("vstest") || processName.Contains("dotnet-test"))
                return true;
            if (builder.Environment.IsEnvironment("Testing"))
                return true;
            // Assembly presence check
            var loaded = AppDomain.CurrentDomain.GetAssemblies();
            if (loaded.Any(a => a.FullName?.Contains("Microsoft.AspNetCore.Mvc.Testing", StringComparison.OrdinalIgnoreCase) == true))
                return true;
            return false;
        }

        var runningInTests = IsRunningUnderTests();

        if (runningInTests)
        {
            var testDefaults = new Dictionary<string, string?>
            {
                ["Server:Url"] = "http://127.0.0.1:5000",
                ["Server:HealthEndpoint"] = "/health",
                ["Server:ConnectionTimeoutSeconds"] = "2",
                ["Database:ConnectionString"] = "InMemory",
                ["ConnectionStrings:DefaultConnection"] = "InMemory",

                // Features
                ["Features:IdentityAuth:Enabled"] = "true",
                ["Features:RefreshTokens:Enabled"] = "true",
                ["Features:ExceptionHandling:Enabled"] = "true",
                ["Features:Swagger:Enabled"] = "false",
                ["Features:Cors:Enabled"] = "false",
                ["Features:HealthChecks:Enabled"] = "false",
                ["Features:AdminSeed:Enabled"] = "false",
                ["Features:AdminSeed:Email"] = "admin@WebTemplate.com",
                ["Features:AdminSeed:Password"] = "Admin123!@#",
                ["Features:AdminSeed:FirstName"] = "System",
                ["Features:AdminSeed:LastName"] = "Administrator",

                // Auth
                ["AuthSettings:Jwt:SecretKey"] = "ThisIsATemporaryTestSecretKeyThatMustBeAtLeast32CharactersLong!@#",
                ["AuthSettings:Jwt:Issuer"] = "CoreWebApp.API",
                ["AuthSettings:Jwt:Audience"] = "CoreWebApp.Client",
                ["AuthSettings:Jwt:ClockSkewMinutes"] = "5",
                ["AuthSettings:Password:RequiredLength"] = "8",
                ["AuthSettings:Password:RequireDigit"] = "true",
                ["AuthSettings:Password:RequireLowercase"] = "true",
                ["AuthSettings:Password:RequireUppercase"] = "true",
                ["AuthSettings:Password:RequireNonAlphanumeric"] = "false",
                ["AuthSettings:Password:RequiredUniqueChars"] = "1",
                ["AuthSettings:Password:ResetTokenExpiryHours"] = "24",
                ["AuthSettings:Lockout:DefaultLockoutEnabled"] = "true",
                ["AuthSettings:Lockout:DefaultLockoutTimeSpanMinutes"] = "5",
                ["AuthSettings:Lockout:MaxFailedAccessAttempts"] = "5",
                ["AuthSettings:User:RequireConfirmedEmail"] = "false",
                ["AuthSettings:User:RequireConfirmedPhoneNumber"] = "false",
                ["AuthSettings:User:RequireUniqueEmail"] = "true",
                ["AuthSettings:User:AllowedUserNameCharacters"] = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+",
                ["AuthSettings:User:SessionTimeoutMinutes"] = "480",
                ["AuthSettings:EmailConfirmation:TokenExpiryHours"] = "24",
                ["AuthSettings:EmailConfirmation:SendWelcomeEmail"] = "false",
                ["AuthSettings:EmailConfirmation:MaxEmailsPerHour"] = "3",
                ["AuthSettings:UserModuleFeatures:EnableLogin"] = "true",
                ["AuthSettings:UserModuleFeatures:EnableRegistration"] = "true",
                ["AuthSettings:UserModuleFeatures:EnableRefreshToken"] = "true",
                ["AuthSettings:UserModuleFeatures:EnableLogout"] = "true",
                ["AuthSettings:UserModuleFeatures:EnableLogoutAllDevices"] = "true",
                ["AuthSettings:UserModuleFeatures:EnableForgotPassword"] = "true",
                ["AuthSettings:UserModuleFeatures:EnableResetPassword"] = "true",
                ["AuthSettings:UserModuleFeatures:EnableConfirmEmail"] = "true",
                ["AuthSettings:UserModuleFeatures:EnableChangePassword"] = "true",
                ["AuthSettings:UserModuleFeatures:IncludeUserTypePermissionsInResponses"] = "true",
                ["Email:Provider"] = "Smtp",
                ["Email:From"] = "no-reply@example.com",
                ["Email:FromName"] = "WebTemplate",
                ["AllowedHosts"] = "*"
            };
            builder.Configuration.AddInMemoryCollection(testDefaults);
        }

        // Set server URL from configuration.
        var serverUrl = builder.Configuration.GetValue<string>("Server:Url");
        if (!string.IsNullOrWhiteSpace(serverUrl))
        {
            builder.WebHost.UseUrls(serverUrl);
        }

        // Defer configuration validation until after the builder is configured by the factory.
        // We add the configuration object now and validate it inside a deferred service registration.
        builder.Services.AddSingleton(sp =>
        {
            try
            {
                // Now, the configuration from TestWebAppFactory is available.
                return new ApplicationConfiguration(sp.GetRequiredService<IConfiguration>());
            }
            catch (InvalidOperationException ex)
            {
                var isLocal = sp.GetRequiredService<IWebHostEnvironment>().IsDevelopment();
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("\n═══════════════════════════════════════════════════════════════");
                Console.WriteLine("  ✗ CONFIGURATION ERROR");
                Console.WriteLine("═══════════════════════════════════════════════════════════════\n");
                Console.ResetColor();
                Console.WriteLine(ex.Message);
                Console.WriteLine();
                if (isLocal)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("Ensure appsettings.Local.json has all required values configured.\n");
                    Console.ResetColor();
                }
                // Stop the application gracefully.
                var appLifetime = sp.GetRequiredService<IHostApplicationLifetime>();
                appLifetime.StopApplication();
                throw; // Re-throw to prevent the app from continuing.
            }
        });

        // Register other configuration interfaces based on the validated ApplicationConfiguration.
        builder.Services.AddSingleton<ICoreConfiguration>(sp => sp.GetRequiredService<ApplicationConfiguration>());
        builder.Services.AddSingleton<WebTemplate.API.Configuration.IApiConfiguration>(sp => new WebTemplate.API.Configuration.ApiConfiguration(sp.GetRequiredService<IConfiguration>()));
        builder.Services.AddSingleton<WebTemplate.Data.Configuration.IDataConfiguration>(sp => sp.GetRequiredService<WebTemplate.API.Configuration.IApiConfiguration>());
        builder.Services.AddSingleton(sp => sp.GetRequiredService<WebTemplate.API.Configuration.IApiConfiguration>().Features);

        // Bind and discover feature modules.
        builder.Services.Configure<FeaturesOptions>(builder.Configuration.GetSection(FeaturesOptions.SectionName));
        var featureHost = new FeatureHost(builder.Configuration).Discover();
        featureHost.ConfigureOptions(builder.Services);

        // Core services.
        builder.Services.AddControllers();
        builder.Services.TryAddScoped<WebTemplate.Core.Interfaces.IRefreshTokenRepository, WebTemplate.Data.Repositories.RefreshTokenRepository>();
        builder.Services.TryAddScoped<WebTemplate.Core.Interfaces.IUserTypeRepository, WebTemplate.Data.Repositories.UserTypeRepository>();
        builder.Services.TryAddScoped<WebTemplate.Core.Interfaces.IEmailSender, WebTemplate.Core.Services.SmtpEmailSender>();
        builder.Services.TryAddScoped<WebTemplate.Core.Interfaces.ITokenService, WebTemplate.Core.Services.TokenService>();
        builder.Services.TryAddScoped<WebTemplate.Core.Interfaces.IAuthService, WebTemplate.Core.Services.AuthService>();
        builder.Services.TryAddScoped<WebTemplate.Core.Interfaces.IUserService, WebTemplate.Core.Services.UserService>();
        builder.Services.TryAddScoped<WebTemplate.Core.Interfaces.IUserTypeService, WebTemplate.Core.Services.UserTypeService>();

        // Conditionally add Identity/Auth module.
        var features = builder.Configuration.GetSection(FeaturesOptions.SectionName).Get<FeaturesOptions>() ?? new FeaturesOptions();
        if (features.IdentityAuth.Enabled)
        {
            builder.Services.AddUserModule(builder.Configuration);
        }

        // Configure services from feature modules.
        featureHost.ConfigureServices(builder.Services);

        var app = builder.Build();

        // Pipeline configuration.
        app.UseHttpsRedirection();
        app.UseRouting();
        featureHost.ConfigurePipeline(app);

        if (features.IdentityAuth.Enabled)
        {
            app.UseUserModule();
        }

        app.MapControllers();
        featureHost.MapEndpoints(app);

        if (features.IdentityAuth.Enabled)
        {
            await app.InitializeUserModuleAsync();
        }

        // Never run the app under test harness; let the test server manage lifetime
        runningInTests = runningInTests || app.Environment.IsEnvironment("Testing");
        if (runningInTests)
        {
            return;
        }

        await app.RunAsync();
    }

    private static void BuildConfiguration(IConfigurationBuilder configurationBuilder, IWebHostEnvironment env)
    {
        // If tests, don't attempt to load files or enforce environment name
        var processName = Process.GetCurrentProcess().ProcessName?.ToLowerInvariant() ?? string.Empty;
        var isTestHost = processName.Contains("testhost") || processName.Contains("vstest") || AppDomain.CurrentDomain.GetAssemblies().Any(a => a.FullName?.Contains("Microsoft.AspNetCore.Mvc.Testing", StringComparison.OrdinalIgnoreCase) == true);
        if (env.IsEnvironment("Testing") || isTestHost)
        {
            return; // TestWebAppFactory provides configuration or we inject minimal defaults.
        }

        if (string.IsNullOrWhiteSpace(env.EnvironmentName))
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("\n═══════════════════════════════════════════════════════════════");
            Console.WriteLine("  ✗ MISSING ASPNETCORE_ENVIRONMENT");
            Console.WriteLine("═══════════════════════════════════════════════════════════════\n");
            Console.ResetColor();
            Console.WriteLine("Set ASPNETCORE_ENVIRONMENT to one of: Development, Staging, Production\n");
            Environment.Exit(1);
            return;
        }

        var allowedEnvironments = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "Development", "Staging", "Production" };
        if (!allowedEnvironments.Contains(env.EnvironmentName))
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("\n═══════════════════════════════════════════════════════════════");
            Console.WriteLine("  ✗ INVALID ASPNETCORE_ENVIRONMENT VALUE");
            Console.WriteLine("═══════════════════════════════════════════════════════════════\n");
            Console.ResetColor();
            Console.WriteLine($"Provided: '{env.EnvironmentName}'");
            Console.WriteLine("Allowed: Development, Staging, Production\n");
            Environment.Exit(1);
            return;
        }

        configurationBuilder.SetBasePath(Directory.GetCurrentDirectory());
        if (env.IsDevelopment())
        {
            configurationBuilder.AddJsonFile("appsettings.Local.json", optional: false, reloadOnChange: true);
        }
        else
        {
            configurationBuilder
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables();
        }
    }
}

namespace WebTemplate.API { public partial class Program { } }

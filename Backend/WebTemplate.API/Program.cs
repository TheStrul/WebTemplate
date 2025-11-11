using WebTemplate.UserModule;
using Microsoft.Extensions.DependencyInjection.Extensions;
using WebTemplate.Core.Features;
using WebTemplate.Core.Configuration.Features;
using System.Diagnostics;

// Check for required user secrets in Development environment
if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
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

// Bind and discover feature modules
builder.Services.Configure<FeaturesOptions>(builder.Configuration.GetSection(FeaturesOptions.SectionName));
var featureHost = new FeatureHost(builder.Configuration).Discover();
featureHost.ConfigureOptions(builder.Services);

// Core services
builder.Services.AddControllers();

// Existing options binding
builder.Services.Configure<WebTemplate.Core.Configuration.JwtSettings>(builder.Configuration.GetSection(WebTemplate.Core.Configuration.JwtSettings.SectionName));
builder.Services.Configure<WebTemplate.Core.Configuration.AuthSettings>(builder.Configuration.GetSection(WebTemplate.Core.Configuration.AuthSettings.SectionName));
builder.Services.Configure<WebTemplate.Core.Configuration.EmailSettings>(builder.Configuration.GetSection(WebTemplate.Core.Configuration.EmailSettings.SectionName));
builder.Services.Configure<WebTemplate.Core.Configuration.UserModuleFeatures>(builder.Configuration.GetSection("UserModule:Features"));
builder.Services.Configure<WebTemplate.Core.Configuration.ResponseMessages>(builder.Configuration.GetSection("ResponseMessages"));
builder.Services.Configure<WebTemplate.Core.Configuration.AppUrls>(builder.Configuration.GetSection(WebTemplate.Core.Configuration.AppUrls.SectionName));
builder.Services.Configure<WebTemplate.Core.Configuration.AdminSeedSettings>(builder.Configuration.GetSection(WebTemplate.Core.Configuration.AdminSeedSettings.SectionName));

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

// Expose Program for tests
namespace WebTemplate.API { public class Program { } }

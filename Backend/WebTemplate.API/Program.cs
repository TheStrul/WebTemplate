using Microsoft.Extensions.DependencyInjection.Extensions;
using WebTemplate.Core.Features;
using WebTemplate.Core.Configuration.Features;
using WebTemplate.Core.Configuration;
using WebTemplate.Data.Modules;
using System.Diagnostics;
using System.Reflection;

internal class Program
{
    private static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Clear default sources and add configuration from explicit sources.
        // In tests, TestWebAppFactory will add its own sources.
        builder.Configuration.Sources.Clear();
        BuildConfiguration(builder.Configuration, builder.Environment);

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

        await app.RunAsync();
    }

    private static void BuildConfiguration(IConfigurationBuilder configurationBuilder, IWebHostEnvironment env)
    {
        if (env.IsEnvironment("Testing"))
        {
            return; // TestWebAppFactory provides configuration.
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

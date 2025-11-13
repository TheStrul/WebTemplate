using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using WebTemplate.Core.Configuration;
using WebTemplate.Core.Entities;
using WebTemplate.Core.Interfaces;
using WebTemplate.Core.Services;
using WebTemplate.Data.Context;
using WebTemplate.Data.Repositories;

namespace WebTemplate.Data.Modules;

/// <summary>
/// User module for Identity, Authentication, and Authorization
/// </summary>
public static class UserModule
{
    public static IServiceCollection AddUserModule(this IServiceCollection services, IConfiguration rootConfig)
    {
        // Resolve module configuration with simple override strategy:
        // 1) External JSON pointed by UserModule:ConfigPath or env USER_MODULE_CONFIG
        // 2) appsettings section: UserModule
        // 3) Fall back to legacy sections: JwtSettings/AuthSettings/ConnectionStrings
        var (moduleJwt, moduleAuth, connectionString) = ResolveModuleConfig(rootConfig);

        // Bind options to keep parity with current code
        services.Configure<JwtSettings>(moduleJwt);
        services.Configure<AuthSettings>(moduleAuth);

        // DbContext
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(connectionString, sqlOptions =>
            {
                sqlOptions.EnableRetryOnFailure(5, TimeSpan.FromSeconds(30), null);
            }));

        // Identity
        services.AddIdentity<ApplicationUser, IdentityRole>(options =>
        {
            var passwordConfig = moduleAuth.GetSection("PasswordRequirements");
            options.Password.RequireDigit = passwordConfig.GetValue<bool>("RequireDigit");
            options.Password.RequireLowercase = passwordConfig.GetValue<bool>("RequireLowercase");
            options.Password.RequireUppercase = passwordConfig.GetValue<bool>("RequireUppercase");
            options.Password.RequireNonAlphanumeric = passwordConfig.GetValue<bool>("RequireNonAlphanumeric");
            options.Password.RequiredLength = passwordConfig.GetValue<int>("RequiredLength");
            options.Password.RequiredUniqueChars = passwordConfig.GetValue<int>("RequiredUniqueChars");

            options.User.RequireUniqueEmail = true;
            options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";

            options.SignIn.RequireConfirmedEmail = moduleAuth.GetValue<bool>("RequireEmailConfirmation");
            options.SignIn.RequireConfirmedAccount = false;

            options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
            options.Lockout.MaxFailedAccessAttempts = 5;
            options.Lockout.AllowedForNewUsers = true;
        })
        .AddEntityFrameworkStores<ApplicationDbContext>()
        .AddDefaultTokenProviders();

        // JWT
        var jwtSettings = new JwtSettings();
        moduleJwt.Bind(jwtSettings);
        var key = Encoding.ASCII.GetBytes(jwtSettings.SecretKey);

        services.AddAuthentication(x =>
        {
            x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(x =>
        {
            x.RequireHttpsMetadata = true;
            x.SaveToken = true;
            x.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = jwtSettings.Issuer,
                ValidateAudience = true,
                ValidAudience = jwtSettings.Audience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.FromMinutes(jwtSettings.ClockSkewMinutes)
            };
        });

        // CORS
        var allowed = moduleAuth.GetSection("AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();
        services.AddCors(options =>
        {
            options.AddPolicy("UserModuleCors", policy =>
            {
                policy.WithOrigins(allowed)
                      .AllowAnyHeader()
                      .AllowAnyMethod()
                      .AllowCredentials();
            });
        });

        // Repositories + Services
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();

        return services;
    }

    public static IApplicationBuilder UseUserModule(this IApplicationBuilder app)
    {
        app.UseCors("UserModuleCors");
        app.UseAuthentication();
        app.UseAuthorization();
        return app;
    }

    public static async Task InitializeUserModuleAsync(this IHost app)
    {
        using var scope = app.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("UserModule");

        try
        {
            await context.Database.MigrateAsync();

            // roles
            string[] roles = { "Admin", "User", "Moderator" };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            // seed admin
            var adminEmail = "admin@corewebapp.com";
            var admin = await userManager.FindByEmailAsync(adminEmail);
            if (admin == null)
            {
                var user = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true,
                    FirstName = "System",
                    LastName = "Administrator",
                    UserTypeId = 1,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                var result = await userManager.CreateAsync(user, "Admin123!");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, "Admin");
                    logger.LogInformation("Default admin created: {email}", adminEmail);
                }
                else
                {
                    logger.LogWarning("Failed to create admin: {errors}", string.Join(", ", result.Errors.Select(e => e.Description)));
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "UserModule initialization failed");
        }
    }

    private static (IConfigurationSection jwt, IConfigurationSection auth, string connectionString) ResolveModuleConfig(IConfiguration root)
    {
        // external JSON path via config or env
        var moduleSection = root.GetSection("UserModule");
        var externalPath = moduleSection.GetValue<string>("ConfigPath") ??
                           Environment.GetEnvironmentVariable("USER_MODULE_CONFIG");

        IConfiguration? externalConfig = null;
        if (!string.IsNullOrWhiteSpace(externalPath))
        {
            try
            {
                var fullPath = Path.IsPathRooted(externalPath)
                    ? externalPath
                    : Path.Combine(AppContext.BaseDirectory, externalPath);
                if (File.Exists(fullPath))
                {
                    externalConfig = new ConfigurationBuilder()
                        .AddJsonFile(fullPath, optional: true, reloadOnChange: false)
                        .Build();
                }
            }
            catch { /* ignore errors, fall back below */ }
        }

        IConfigurationSection PickSection(string moduleKey, string legacyKey)
        {
            if (externalConfig != null)
            {
                var s = externalConfig.GetSection("UserModule").Exists()
                    ? externalConfig.GetSection("UserModule").GetSection(moduleKey)
                    : externalConfig.GetSection(moduleKey);
                if (s.Exists()) return s;
            }
            var mod = moduleSection.GetSection(moduleKey);
            if (mod.Exists()) return mod;
            return root.GetSection(legacyKey);
        }

        var jwt = PickSection("Jwt", "JwtSettings");
        var auth = PickSection("Auth", "AuthSettings");

        string conn = string.Empty;
        if (externalConfig != null)
        {
            var cs = externalConfig.GetSection("UserModule:Db");
            if (!cs.Exists()) cs = externalConfig.GetSection("Db");
            conn = cs.GetValue<string>("ConnectionString") ?? conn;
        }
        if (string.IsNullOrWhiteSpace(conn))
        {
            var csMod = moduleSection.GetSection("Db");
            conn = csMod.GetValue<string>("ConnectionString") ?? string.Empty;
            if (string.IsNullOrWhiteSpace(conn))
            {
                conn = root.GetConnectionString("DefaultConnection") ?? string.Empty;
            }
        }

        if (string.IsNullOrWhiteSpace(conn))
        {
            throw new InvalidOperationException(
                "Connection string is required but not configured. Please provide a connection string via one of these methods: " +
                "1) UserModule:Db:ConnectionString, 2) ConnectionStrings:DefaultConnection in appsettings.json, " +
                "3) External config file specified in UserModule:ConfigPath or USER_MODULE_CONFIG environment variable.");
        }

        return (jwt, auth, conn);
    }
}

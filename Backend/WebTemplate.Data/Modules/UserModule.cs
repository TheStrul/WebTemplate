using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System.Reflection;
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
        // For test environments, the DbContext is configured by TestWebAppFactory.
        // We can skip the rest of the setup for DbContext and connection strings.
        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
        var isTesting = "Testing".Equals(environment, StringComparison.OrdinalIgnoreCase);

        // Resolve module configuration with simple override strategy:
        // 1) External JSON pointed by UserModule:ConfigPath or env USER_MODULE_CONFIG
        // 2) appsettings section: UserModule
        // 3) Fall back to legacy sections: JwtSettings/AuthSettings/ConnectionStrings
        var (moduleJwt, moduleAuth, connectionString) = ResolveModuleConfig(rootConfig);

        // Bind options to keep parity with current code
        services.Configure<JwtSettings>(moduleJwt);
        services.Configure<AuthSettings>(moduleAuth);

        // DbContext - only add if not in a test environment where it's added separately
        if (!isTesting)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException(
                    "Connection string is required but not configured. Please provide a connection string via one of these methods: " +
                    "1) UserModule:Db:ConnectionString, 2) ConnectionStrings:DefaultConnection in appsettings.json, " +
                    "3) External config file specified in UserModule:ConfigPath or USER_MODULE_CONFIG environment variable.");
            }

            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(connectionString, sqlOptions =>
                {
                    sqlOptions.EnableRetryOnFailure(5, TimeSpan.FromSeconds(30), null);
                }));
        }

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
        var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();

        try
        {
            // Initialize database schema using SQL script (SQL-first approach)
            await InitializeDatabaseSchemaAsync(context, logger);

            // roles
            string[] roles = { "Admin", "User", "Moderator" };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            // seed admin from configuration (NO FALLBACKS)
            var adminSeedConfig = config.GetSection("Features:AdminSeed");
            var adminEnabled = adminSeedConfig.GetValue<bool>("Enabled");
            
            if (adminEnabled)
            {
                var adminEmail = adminSeedConfig.GetValue<string>("Email")
                    ?? throw new InvalidOperationException("Features:AdminSeed:Email is required but not configured");
                var adminPassword = adminSeedConfig.GetValue<string>("Password")
                    ?? throw new InvalidOperationException("Features:AdminSeed:Password is required but not configured");
                var adminFirstName = adminSeedConfig.GetValue<string>("FirstName") ?? "System";
                var adminLastName = adminSeedConfig.GetValue<string>("LastName") ?? "Administrator";
                
                var admin = await userManager.FindByEmailAsync(adminEmail);
                if (admin == null)
                {
                    var user = new ApplicationUser
                    {
                        UserName = adminEmail,
                        Email = adminEmail,
                        EmailConfirmed = true,
                        FirstName = adminFirstName,
                        LastName = adminLastName,
                        UserTypeId = 1,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    };

                    var result = await userManager.CreateAsync(user, adminPassword);
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
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "UserModule initialization failed");
        }
    }

    private static async Task InitializeDatabaseSchemaAsync(ApplicationDbContext context, ILogger logger)
    {
        try
        {
            // Check if database exists and has tables
            var connection = context.Database.GetDbConnection();
            await connection.OpenAsync();
            
            using var command = connection.CreateCommand();
            command.CommandText = "SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'AspNetRoles'";
            var result = await command.ExecuteScalarAsync();
            
            if (result != null && (int)result > 0)
            {
                logger.LogInformation("Database schema already initialized");
                return;
            }

            logger.LogInformation("Initializing database schema from SQL script");
            
            // Find the SQL script - try multiple locations
            string? sqlScriptPath = null;
            
            // Try 1: Look in the Data project directory relative to AppContext.BaseDirectory
            var potentialPaths = new[]
            {
                // From bin output
                Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "WebTemplate.Data", "Migrations", "db-init.sql"),
                // Direct path from project
                Path.Combine(Directory.GetCurrentDirectory(), "..", "WebTemplate.Data", "Migrations", "db-init.sql"),
                Path.Combine(Directory.GetCurrentDirectory(), "WebTemplate.Data", "Migrations", "db-init.sql"),
                // Search up from current directory
                Path.Combine(AppContext.BaseDirectory, "db-init.sql"),
            };

            foreach (var potentialPath in potentialPaths)
            {
                var normalizedPath = Path.GetFullPath(potentialPath);
                if (File.Exists(normalizedPath))
                {
                    sqlScriptPath = normalizedPath;
                    break;
                }
            }
            
            if (string.IsNullOrEmpty(sqlScriptPath) || !File.Exists(sqlScriptPath))
            {
                logger.LogWarning("SQL init script not found. Tried paths: {paths}", string.Join(", ", potentialPaths.Select(p => Path.GetFullPath(p))));
                // Don't throw - allow app to start even if schema isn't initialized
                logger.LogInformation("Continuing without schema initialization - migrations may not exist or database may already be initialized");
                return;
            }

            var sqlScript = await File.ReadAllTextAsync(sqlScriptPath);
            
            // Split by GO statements and execute each batch
            var batches = sqlScript.Split(new[] { "\nGO\n", "\nGO\r\n", "\r\nGO\r\n" }, StringSplitOptions.RemoveEmptyEntries);
            
            foreach (var batch in batches)
            {
                if (string.IsNullOrWhiteSpace(batch)) continue;
                
                using var batchCommand = connection.CreateCommand();
                batchCommand.CommandText = batch.Trim();
                batchCommand.CommandTimeout = 300;
                
                try
                {
                    await batchCommand.ExecuteNonQueryAsync();
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, "Error executing SQL batch (continuing anyway): {message}", ex.Message);
                }
            }
            
            logger.LogInformation("Database schema initialization completed");
        }
        finally
        {
            await context.Database.CloseConnectionAsync();
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
            
            // Try legacy key first
            var legacy = root.GetSection(legacyKey);
            if (legacy.Exists()) return legacy;
            
            // For Jwt, also try AuthSettings:Jwt
            if (legacyKey == "JwtSettings")
            {
                var authJwt = root.GetSection("AuthSettings:Jwt");
                if (authJwt.Exists()) return authJwt;
            }
            
            // For Auth, also try AuthSettings
            if (legacyKey == "AuthSettings")
            {
                var auth = root.GetSection("AuthSettings");
                if (auth.Exists()) return auth;
            }
            
            return legacy;
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

        return (jwt, auth, conn);
    }
}

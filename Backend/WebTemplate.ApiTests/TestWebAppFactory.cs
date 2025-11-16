namespace WebTemplate.ApiTests
{
    using System.Collections.Generic;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Mvc.Testing;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using WebTemplate.Core.Entities;
    using WebTemplate.Data.Context;
    using Xunit;

    public class TestWebAppFactory : WebApplicationFactory<WebTemplate.API.Program>, IAsyncLifetime
    {
        private const string TestDatabaseName = "CoreWebTemplateDb_IntegrationTests";
        private bool _seeded = false;
        private readonly SemaphoreSlim _seedLock = new SemaphoreSlim(1, 1);

        // Ensure environment is set as early as possible
        static TestWebAppFactory()
        {
            System.Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Testing");
            System.Environment.SetEnvironmentVariable("DOTNET_ENVIRONMENT", "Testing");
        }

        public TestWebAppFactory()
        {
            System.Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Testing");
            System.Environment.SetEnvironmentVariable("DOTNET_ENVIRONMENT", "Testing");
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Testing");

            // Make sure environment variables also reflect Testing for libraries that read env vars directly
            System.Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Testing");
            System.Environment.SetEnvironmentVariable("DOTNET_ENVIRONMENT", "Testing");

            builder.ConfigureAppConfiguration((ctx, configBuilder) =>
            {
                // Don't load any files - rely on in-memory configuration only
                configBuilder.Sources.Clear();

                // Configure everything needed for tests in-memory
                var testConfig = new Dictionary<string, string?>
                {
                    // Server configuration
                    ["Server:Url"] = "http://localhost:5000",
                    ["Server:HealthEndpoint"] = "/health",
                    ["Server:ConnectionTimeoutSeconds"] = "2",

                    // Required by ApiConfiguration
                    ["AllowedHosts"] = "*",
                    
                    // Database (will be overridden to InMemory in ConfigureServices)
                    ["Database:ConnectionString"] = "InMemory",
                    ["ConnectionStrings:DefaultConnection"] = "InMemory",

                    // Auth settings - Password
                    ["AuthSettings:Password:RequiredLength"] = "8",
                    ["AuthSettings:Password:RequireDigit"] = "true",
                    ["AuthSettings:Password:RequireLowercase"] = "true",
                    ["AuthSettings:Password:RequireUppercase"] = "true",
                    ["AuthSettings:Password:RequireNonAlphanumeric"] = "false",
                    ["AuthSettings:Password:RequiredUniqueChars"] = "1",
                    ["AuthSettings:Password:ResetTokenExpiryHours"] = "24",

                    // Auth settings - Lockout
                    ["AuthSettings:Lockout:DefaultLockoutEnabled"] = "true",
                    ["AuthSettings:Lockout:DefaultLockoutTimeSpanMinutes"] = "5",
                    ["AuthSettings:Lockout:MaxFailedAccessAttempts"] = "5",

                    // Auth settings - User
                    ["AuthSettings:User:RequireConfirmedEmail"] = "false",
                    ["AuthSettings:User:RequireConfirmedPhoneNumber"] = "false",
                    ["AuthSettings:User:RequireUniqueEmail"] = "true",
                    ["AuthSettings:User:AllowedUserNameCharacters"] = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+",
                    ["AuthSettings:User:SessionTimeoutMinutes"] = "480",

                    // Auth settings - Email Confirmation
                    ["AuthSettings:EmailConfirmation:TokenExpiryHours"] = "24",
                    ["AuthSettings:EmailConfirmation:SendWelcomeEmail"] = "false",
                    ["AuthSettings:EmailConfirmation:MaxEmailsPerHour"] = "3",

                    // Auth settings - JWT
                    ["AuthSettings:Jwt:SecretKey"] = "ThisIsATemporaryTestSecretKeyThatMustBeAtLeast32CharactersLong!@#",
                    ["AuthSettings:Jwt:Issuer"] = "CoreWebApp.API",
                    ["AuthSettings:Jwt:Audience"] = "CoreWebApp.Client",
                    ["AuthSettings:Jwt:AccessTokenExpiryMinutes"] = "60",
                    ["AuthSettings:Jwt:RefreshTokenExpiryDays"] = "30",
                    ["AuthSettings:Jwt:ClockSkewMinutes"] = "5",

                    // Auth settings - User Module Feature flags
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

                    // Email settings
                    ["Email:Provider"] = "Smtp",
                    ["Email:From"] = "no-reply@example.com",
                    ["Email:FromName"] = "WebTemplate",

                    // AppUrls
                    ["AuthSettings:AppUrls:ConfirmEmailPath"] = "/confirm-email",

                    // Admin seed configuration (disabled - tests seed explicitly)
                    ["Features:AdminSeed:Enabled"] = "false",
                    ["Features:AdminSeed:Email"] = "admin@WebTemplate.com",
                    ["Features:AdminSeed:Password"] = "Admin123!@#",
                    ["Features:AdminSeed:FirstName"] = "System",
                    ["Features:AdminSeed:LastName"] = "Administrator",
                    
                    // Feature flags
                    ["Features:Swagger:Enabled"] = "false",
                    ["Features:Cors:Enabled"] = "false",
                    ["Features:RateLimiting:Enabled"] = "false",
                    ["Features:SecurityHeaders:Enabled"] = "false",
                    ["Features:HealthChecks:Enabled"] = "false",
                    ["Features:IdentityAuth:Enabled"] = "true",
                    ["Features:RefreshTokens:Enabled"] = "true",
                    ["Features:ExceptionHandling:Enabled"] = "true",
                    ["Features:EmailModule:Enabled"] = "false",

                    // UserModule overrides to ensure Identity options are configured
                    ["UserModule:Auth:PasswordRequirements:RequireDigit"] = "true",
                    ["UserModule:Auth:PasswordRequirements:RequireLowercase"] = "true",
                    ["UserModule:Auth:PasswordRequirements:RequireUppercase"] = "true",
                    ["UserModule:Auth:PasswordRequirements:RequireNonAlphanumeric"] = "false",
                    ["UserModule:Auth:PasswordRequirements:RequiredLength"] = "8",
                    ["UserModule:Auth:PasswordRequirements:RequiredUniqueChars"] = "1"
                };

                configBuilder.AddInMemoryCollection(testConfig);
            });

            builder.ConfigureServices((ctx, services) =>
            {
                // Remove all EF database provider registrations
                var allEfDescriptors = services
                    .Where(d => d.ServiceType.FullName?.Contains("EntityFrameworkCore") == true ||
                                d.ServiceType == typeof(ApplicationDbContext) ||
                                d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>))
                    .ToList();

                foreach (var descriptor in allEfDescriptors)
                {
                    services.Remove(descriptor);
                }

                // Register ONLY InMemory database for tests
                services.AddDbContext<ApplicationDbContext>(options =>
                    options.UseInMemoryDatabase($"WebTemplate_Test_{Guid.NewGuid()}"),
                    ServiceLifetime.Scoped);
            });
        }

        // Ensure DB is initialized once per test collection before any tests run
        public async Task InitializeAsync()
        {
            // Build the web application explicitly
            var app = this.CreateDefaultClient();
            await InitializeDatabaseAsync();
        }

        async Task IAsyncLifetime.DisposeAsync()
        {
            await base.DisposeAsync();
        }

        public async Task InitializeDatabaseAsync()
        {
            await _seedLock.WaitAsync();
            try
            {
                if (_seeded) return;

                using var scope = Services.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                // For InMemory, just ensure created (no physical database)
                await db.Database.EnsureCreatedAsync();

                // Seed UserTypes
                SeedUserTypes(db);

                // Seed admin user
                await SeedAdminUser(scope.ServiceProvider);

                _seeded = true;
            }
            finally
            {
                _seedLock.Release();
            }
        }

        private static void SeedUserTypes(ApplicationDbContext context)
        {
            if (context.UserTypes.Any())
                return;

            context.UserTypes.AddRange(
                new UserType { Id = 1, Name = "Admin", Description = "System Administrator with full access", IsActive = true, CreatedAt = DateTime.UtcNow, Permissions = "[\"user.create\",\"user.read\",\"user.update\",\"user.delete\",\"usertype.manage\",\"system.admin\"]" },
                new UserType { Id = 2, Name = "User", Description = "Regular user with basic access", IsActive = true, CreatedAt = DateTime.UtcNow, Permissions = "[\"profile.read\",\"profile.update\"]" },
                new UserType { Id = 3, Name = "Moderator", Description = "Moderator with limited admin access", IsActive = true, CreatedAt = DateTime.UtcNow, Permissions = "[\"user.read\",\"user.update\",\"profile.read\",\"profile.update\"]" }
            );

            context.SaveChanges();
        }

        private static async Task SeedAdminUser(IServiceProvider serviceProvider)
        {
            var config = serviceProvider.GetRequiredService<IConfiguration>();
            var adminPassword = config["Features:AdminSeed:Password"];
            if (string.IsNullOrWhiteSpace(adminPassword)) throw new InvalidOperationException("Admin seed password configuration missing.");
            var adminEmail = config["Features:AdminSeed:Email"] ?? throw new InvalidOperationException("Admin seed email configuration missing.");
            try
            {
                var db = serviceProvider.GetRequiredService<ApplicationDbContext>();
                var userManager = serviceProvider.GetRequiredService<Microsoft.AspNetCore.Identity.UserManager<ApplicationUser>>();
                var roleManager = serviceProvider.GetRequiredService<Microsoft.AspNetCore.Identity.RoleManager<Microsoft.AspNetCore.Identity.IdentityRole>>();

                // Ensure roles exist
                string[] roleNames = { "Admin", "User", "Moderator" };
                foreach (var roleName in roleNames)
                {
                    if (!await roleManager.RoleExistsAsync(roleName))
                    {
                        var result = await roleManager.CreateAsync(new Microsoft.AspNetCore.Identity.IdentityRole(roleName));
                        if (!result.Succeeded)
                        {
                            Console.WriteLine($"Warning: Failed to create role {roleName}");
                        }
                    }
                }

                var existingAdmin = await userManager.FindByEmailAsync(adminEmail);
                if (existingAdmin == null)
                {
                    var adminUser = new ApplicationUser
                    {
                        Id = Guid.NewGuid().ToString(),
                        UserName = adminEmail,
                        Email = adminEmail,
                        EmailConfirmed = true,
                        FirstName = config["Features:AdminSeed:FirstName"] ?? throw new InvalidOperationException("Admin seed first name missing."),
                        LastName = config["Features:AdminSeed:LastName"] ?? throw new InvalidOperationException("Admin seed last name missing."),
                        UserTypeId = 1,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow,
                        NormalizedUserName = adminEmail.ToUpperInvariant(),
                        NormalizedEmail = adminEmail.ToUpperInvariant(),
                        SecurityStamp = Guid.NewGuid().ToString(),
                        ConcurrencyStamp = Guid.NewGuid().ToString()
                    };

                    // Hash the password
                    var passwordHash = userManager.PasswordHasher.HashPassword(adminUser, adminPassword);
                    adminUser.PasswordHash = passwordHash;

                    // Add to database
                    db.Users.Add(adminUser);
                    await db.SaveChangesAsync();

                    // Add to Admin role
                    var roleResult = await userManager.AddToRoleAsync(adminUser, "Admin");
                    if (!roleResult.Succeeded)
                    {
                        Console.WriteLine("Warning: Failed to add Admin role to user");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Warning: Error seeding admin user: {ex.Message}");
            }
        }
    }
}

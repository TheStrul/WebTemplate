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

    public class TestWebAppFactory : WebApplicationFactory<WebTemplate.API.Program>
    {
        private const string TestDatabaseName = "CoreWebTemplateDb_IntegrationTests";
        private bool _seeded = false;
        private readonly SemaphoreSlim _seedLock = new SemaphoreSlim(1, 1);

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Testing");

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
                    
                    // Database (will be overridden to InMemory in ConfigureServices)
                    ["Database:ConnectionString"] = "InMemory",
                    ["ConnectionStrings:DefaultConnection"] = "InMemory",

                    // Auth settings
                    ["AuthSettings:User:RequireConfirmedEmail"] = "false",
                    ["AuthSettings:User:RequireConfirmedPhoneNumber"] = "false",
                    ["AuthSettings:User:RequireUniqueEmail"] = "true",
                    ["AuthSettings:User:AllowedUserNameCharacters"] = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+",
                    ["AuthSettings:User:SessionTimeoutMinutes"] = "480",
                    ["AuthSettings:Password:RequiredLength"] = "8",
                    ["AuthSettings:Password:RequireDigit"] = "true",
                    ["AuthSettings:Password:RequireLowercase"] = "true",
                    ["AuthSettings:Password:RequireUppercase"] = "true",
                    ["AuthSettings:Password:RequireNonAlphanumeric"] = "false",
                    ["AuthSettings:Password:RequiredUniqueChars"] = "1",
                    ["AuthSettings:Jwt:SecretKey"] = "ThisIsATemporaryTestSecretKeyThatMustBeAtLeast32CharactersLong!@#",
                    ["AuthSettings:Jwt:Issuer"] = "CoreWebApp.API",
                    ["AuthSettings:Jwt:Audience"] = "CoreWebApp.Client",
                    ["AuthSettings:Jwt:AccessTokenExpiryMinutes"] = "60",
                    ["AuthSettings:Jwt:RefreshTokenExpiryDays"] = "30",
                    ["AuthSettings:Jwt:ClockSkewMinutes"] = "5",
                    
                    // Admin seed configuration
                    ["Features:AdminSeed:Enabled"] = "true",
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
                    ["Features:EmailModule:Enabled"] = "false"
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

                // Create admin user
                var adminEmail = "admin@WebTemplate.com";
                var existingAdmin = await userManager.FindByEmailAsync(adminEmail);
                
                if (existingAdmin == null)
                {
                    var adminUser = new ApplicationUser
                    {
                        Id = Guid.NewGuid().ToString(),
                        UserName = adminEmail,
                        Email = adminEmail,
                        EmailConfirmed = true,
                        FirstName = "System",
                        LastName = "Administrator",
                        UserTypeId = 1,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow,
                        NormalizedUserName = adminEmail.ToUpper(),
                        NormalizedEmail = adminEmail.ToUpper(),
                        SecurityStamp = Guid.NewGuid().ToString(),
                        ConcurrencyStamp = Guid.NewGuid().ToString()
                    };

                    // Hash the password
                    var passwordHash = userManager.PasswordHasher.HashPassword(adminUser, "Admin123!");
                    adminUser.PasswordHash = passwordHash;

                    // Add to database
                    db.Users.Add(adminUser);
                    await db.SaveChangesAsync();

                    // Add to Admin role
                    var roleResult = await userManager.AddToRoleAsync(adminUser, "Admin");
                    if (!roleResult.Succeeded)
                    {
                        Console.WriteLine($"Warning: Failed to add Admin role to user");
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

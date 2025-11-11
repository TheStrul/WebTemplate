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
            builder.ConfigureAppConfiguration((ctx, configBuilder) =>
            {
                var overrides = new Dictionary<string, string?>
                {
                    ["ConnectionStrings:DefaultConnection"] = $"Server=(localdb)\\mssqllocaldb;Database={TestDatabaseName};Trusted_Connection=true;MultipleActiveResultSets=true;TrustServerCertificate=true",
                    ["AuthSettings:User:RequireConfirmedEmail"] = "false",
                    ["Features:AdminSeed:Enabled"] = "true",
                    ["Features:AdminSeed:Email"] = "admin@WebTemplate.com",
                    ["Features:AdminSeed:Password"] = "Admin123!",
                    ["Features:AdminSeed:FirstName"] = "System",
                    ["Features:AdminSeed:LastName"] = "Administrator"
                };
                configBuilder.AddInMemoryCollection(overrides!);
            });

            builder.ConfigureServices(services =>
            {
                // Remove the existing DbContext registration from UserModule
                var dbContextDescriptor = services.FirstOrDefault(
                    d => d.ServiceType == typeof(ApplicationDbContext));
                if (dbContextDescriptor != null)
                {
                    services.Remove(dbContextDescriptor);
                }

                var dbContextOptionsDescriptor = services.FirstOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));
                if (dbContextOptionsDescriptor != null)
                {
                    services.Remove(dbContextOptionsDescriptor);
                }

                // Re-register with test database connection string
                services.AddDbContext<ApplicationDbContext>(options =>
                    options.UseSqlServer(
                        $"Server=(localdb)\\mssqllocaldb;Database={TestDatabaseName};Trusted_Connection=true;MultipleActiveResultSets=true;TrustServerCertificate=true",
                        sqlOptions =>
                        {
                            sqlOptions.EnableRetryOnFailure(5, TimeSpan.FromSeconds(10), null);
                        }));
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

                // Recreate database for clean test state
                await db.Database.EnsureDeletedAsync();
                await db.Database.EnsureCreatedAsync();

                // Seed UserTypes (required for FK constraints)
                SeedUserTypes(db);

                // Seed admin user for tests
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
                new UserType
                {
                    Name = "Admin",
                    Description = "System Administrator with full access",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    Permissions = "[\"user.create\",\"user.read\",\"user.update\",\"user.delete\",\"usertype.manage\",\"system.admin\"]"
                },
                new UserType
                {
                    Name = "User",
                    Description = "Regular user with basic access",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    Permissions = "[\"profile.read\",\"profile.update\"]"
                },
                new UserType
                {
                    Name = "Moderator",
                    Description = "Moderator with limited admin access",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    Permissions = "[\"user.read\",\"user.update\",\"profile.read\",\"profile.update\"]"
                }
            );

            context.SaveChanges();
        }

        private static async Task SeedAdminUser(IServiceProvider serviceProvider)
        {
            var userManager = serviceProvider.GetRequiredService<Microsoft.AspNetCore.Identity.UserManager<ApplicationUser>>();
            var roleManager = serviceProvider.GetRequiredService<Microsoft.AspNetCore.Identity.RoleManager<Microsoft.AspNetCore.Identity.IdentityRole>>();

            // Ensure roles exist
            string[] roles = { "Admin", "User", "Moderator" };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new Microsoft.AspNetCore.Identity.IdentityRole(role));
                }
            }

            // Seed admin user with test credentials
            var adminEmail = "admin@WebTemplate.com";
            var admin = await userManager.FindByEmailAsync(adminEmail);
            if (admin == null)
            {
                admin = new ApplicationUser
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

                var result = await userManager.CreateAsync(admin, "Admin123!");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(admin, "Admin");
                }
            }
        }
    }
}

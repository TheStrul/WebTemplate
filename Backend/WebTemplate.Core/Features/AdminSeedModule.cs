namespace WebTemplate.Core.Features
{
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using WebTemplate.Core.Configuration.Features;
    using WebTemplate.Core.Entities;

    internal class AdminSeedModule : IFeatureModule
    {
        private readonly FeaturesOptions _options;
        public AdminSeedModule(FeaturesOptions options) => _options = options;
        public string Name => "AdminSeed";
        public void ConfigureOptions(IConfiguration configuration, IServiceCollection services)
        {
            services.Configure<FeaturesOptions>(configuration.GetSection(FeaturesOptions.SectionName));
        }
        public void ConfigureServices(IServiceCollection services) { }
        public void MapEndpoints(IApplicationBuilder app) { }
        public void ConfigurePipeline(IApplicationBuilder app)
        {
            if (!_options.AdminSeed.Enabled) return;

            // Validate required settings - NO FALLBACKS!
            if (string.IsNullOrWhiteSpace(_options.AdminSeed.Email))
                throw new InvalidOperationException("Features:AdminSeed:Email is required when Features:AdminSeed:Enabled is true. Please configure in appsettings.json or user secrets.");

            if (string.IsNullOrWhiteSpace(_options.AdminSeed.Password))
                throw new InvalidOperationException("Features:AdminSeed:Password is required when Features:AdminSeed:Enabled is true. Please configure in user secrets.");

            if (string.IsNullOrWhiteSpace(_options.AdminSeed.FirstName))
                throw new InvalidOperationException("Features:AdminSeed:FirstName is required when Features:AdminSeed:Enabled is true.");

            if (string.IsNullOrWhiteSpace(_options.AdminSeed.LastName))
                throw new InvalidOperationException("Features:AdminSeed:LastName is required when Features:AdminSeed:Enabled is true.");

            _ = Task.Run(async () =>
            {
                using var scope = app.ApplicationServices.CreateScope();
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
                var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
                var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("AdminSeed");

                try
                {
                    foreach (var role in new[] { "Admin", "User", "Moderator" })
                    {
                        if (!await roleManager.RoleExistsAsync(role))
                        {
                            await roleManager.CreateAsync(new IdentityRole(role));
                        }
                    }

                    var email = _options.AdminSeed.Email;
                    var admin = await userManager.FindByEmailAsync(email);
                    if (admin == null)
                    {
                        var user = new ApplicationUser
                        {
                            UserName = email,
                            Email = email,
                            EmailConfirmed = true,
                            FirstName = _options.AdminSeed.FirstName,
                            LastName = _options.AdminSeed.LastName,
                            UserTypeId = 1,
                            IsActive = true,
                            CreatedAt = DateTime.UtcNow
                        };

                        var result = await userManager.CreateAsync(user, _options.AdminSeed.Password);
                        if (result.Succeeded)
                        {
                            await userManager.AddToRoleAsync(user, "Admin");
                            logger.LogInformation("Admin user created: {email}", email);
                        }
                        else
                        {
                            logger.LogError("Failed to create admin: {errors}", string.Join(", ", result.Errors.Select(e => e.Description)));
                        }
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "AdminSeedModule failed");
                }
            });
        }
    }
}

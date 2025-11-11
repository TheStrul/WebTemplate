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

namespace WebTemplate.Core.Features
{
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
                            logger.LogInformation("Default admin created: {email}", email);
                        }
                        else
                        {
                            logger.LogWarning("Failed to create admin: {errors}", string.Join(", ", result.Errors.Select(e => e.Description)));
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

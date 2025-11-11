using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using WebTemplate.Core.Configuration;
using WebTemplate.Core.Entities;
using WebTemplate.Core.Interfaces;

namespace WebTemplate.API;

public static class AdminSeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        var logger = services.GetRequiredService<ILoggerFactory>().CreateLogger("AdminSeeder");
        var settings = services.GetRequiredService<IOptions<AdminSeedSettings>>().Value;
        if (!settings.Enabled)
        {
            logger.LogInformation("Admin seeding disabled.");
            return;
        }

        using var scope = services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userTypeRepo = scope.ServiceProvider.GetRequiredService<IUserTypeRepository>();

        // Ensure roles
        foreach (var role in new[] { "Admin", "User", "Moderator" })
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        // Ensure admin exists
        var admin = await userManager.FindByEmailAsync(settings.Email);
        if (admin != null)
        {
            logger.LogInformation("Admin user already exists: {Email}", settings.Email);
            return;
        }

        // Ensure UserType Admin
        var adminUserType = await userTypeRepo.GetByIdAsync(1) ?? await userTypeRepo.GetByIdAsync(1);

        admin = new ApplicationUser
        {
            UserName = settings.Email,
            Email = settings.Email,
            EmailConfirmed = true,
            FirstName = settings.FirstName,
            LastName = settings.LastName,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UserTypeId = adminUserType?.Id ?? 1
        };

        var result = await userManager.CreateAsync(admin, settings.Password);
        if (!result.Succeeded)
        {
            logger.LogError("Failed creating admin: {Errors}", string.Join(", ", result.Errors.Select(e => e.Description)));
            return;
        }

        await userManager.AddToRolesAsync(admin, new[] { "Admin" });
        logger.LogInformation("Admin user seeded: {Email}", settings.Email);
    }
}

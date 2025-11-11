namespace WebTemplate.API
{
    using Microsoft.AspNetCore.Identity;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using WebTemplate.Core.Configuration;
    using WebTemplate.Core.Entities;
    using WebTemplate.Core.Interfaces;

    public static class AdminSeeder
    {
        public static async Task SeedAsync(IServiceProvider services)
        {
            var logger = services.GetRequiredService<ILoggerFactory>().CreateLogger("AdminSeeder");
            var coreConfig = services.GetRequiredService<ICoreConfiguration>();
            var settings = coreConfig.AdminSeed;
            if (!settings.Enabled)
            {
                logger.LogInformation("Admin seeding disabled.");
                return;
            }

            logger.LogInformation("AdminSeed settings - Email: {Email}, Password length: {Length}, FirstName: {FirstName}, LastName: {LastName}",
                settings.Email,
                settings.Password?.Length ?? 0,
                settings.FirstName,
                settings.LastName);

            // Validate password before attempting to create user
            if (string.IsNullOrWhiteSpace(settings.Password))
            {
                logger.LogError("AdminSeed:Password is null or empty. Cannot create admin user.");
                return;
            }

            if (settings.Password.Length < 8)
            {
                logger.LogError("AdminSeed:Password is too short ({Length} characters). Minimum is 8 characters.", settings.Password.Length);
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
}

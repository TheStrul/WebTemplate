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

            // NOTE: Admin seeding is now handled by UserModule.InitializeUserModuleAsync()
            // which runs AFTER database migrations. This prevents the race condition where
            // AdminSeedModule tried to access tables before migrations were applied.
            // The validation above ensures configuration is valid before startup completes.
        }
    }
}

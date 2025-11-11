using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WebTemplate.Core.Configuration.Features;

namespace WebTemplate.Core.Features
{
    internal class RefreshTokensModule : IFeatureModule
    {
        private readonly FeaturesOptions _options;
        public RefreshTokensModule(FeaturesOptions options) => _options = options;
        public string Name => "RefreshTokens";
        public void ConfigureOptions(IConfiguration configuration, IServiceCollection services)
        {
            services.Configure<FeaturesOptions>(configuration.GetSection(FeaturesOptions.SectionName));
        }
        public void ConfigureServices(IServiceCollection services)
        {
            // Background cleanup service
            services.AddHostedService<WebTemplate.Core.Services.RefreshTokenCleanupService>();
        }
        public void MapEndpoints(IApplicationBuilder app) { }
        public void ConfigurePipeline(IApplicationBuilder app) { }
    }
}

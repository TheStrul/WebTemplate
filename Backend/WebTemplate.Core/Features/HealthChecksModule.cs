using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using WebTemplate.Core.Configuration.Features;

namespace WebTemplate.Core.Features
{
    internal class HealthChecksModule : IFeatureModule
    {
        private readonly FeaturesOptions _options;
        public HealthChecksModule(FeaturesOptions options) => _options = options;
        public string Name => "HealthChecks";
        public void ConfigureOptions(IConfiguration configuration, IServiceCollection services) { }
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddHealthChecks()
                .AddCheck("self", () => HealthCheckResult.Healthy());
        }
        public void MapEndpoints(IApplicationBuilder app)
        {
            var path = _options.HealthChecks.Path ?? "/health";
            app.UseEndpoints(endpoints => endpoints.MapHealthChecks(path));
        }
        public void ConfigurePipeline(IApplicationBuilder app) { }
    }
}

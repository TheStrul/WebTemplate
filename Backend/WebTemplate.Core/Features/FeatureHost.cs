using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WebTemplate.Core.Configuration.Features;

namespace WebTemplate.Core.Features
{
    public class FeatureHost
    {
        private readonly IConfiguration _configuration;
        private readonly List<IFeatureModule> _modules = new();

        public FeatureHost(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public FeatureHost Discover()
        {
            var options = _configuration.GetSection("Features").Get<WebTemplate.Core.Configuration.Features.FeaturesOptions>() ?? new();
            if (options.ExceptionHandling.Enabled) _modules.Add(new ExceptionHandlingModule(options));
            if (options.Serilog.Enabled) _modules.Add(new SerilogModule(options));
            if (options.Swagger.Enabled) _modules.Add(new SwaggerModule(options));
            if (options.Cors.Enabled) _modules.Add(new CorsModule(options));
            if (options.RateLimiting.Enabled) _modules.Add(new RateLimitingModule(options));
            if (options.SecurityHeaders.Enabled) _modules.Add(new SecurityHeadersModule(options));
            if (options.HealthChecks.Enabled) _modules.Add(new HealthChecksModule(options));
            if (options.RefreshTokens.Enabled) _modules.Add(new RefreshTokensModule(options));
            if (options.AdminSeed.Enabled) _modules.Add(new AdminSeedModule(options));
            // Always add Email module (it decides behavior by provider)
            _modules.Add(new EmailModule(_configuration));
            return this;
        }

        public void ConfigureOptions(IServiceCollection services)
        {
            foreach (var m in _modules) m.ConfigureOptions(_configuration, services);
        }

        public void ConfigureServices(IServiceCollection services)
        {
            foreach (var m in _modules) m.ConfigureServices(services);
        }

        public void MapEndpoints(IApplicationBuilder app)
        {
            foreach (var m in _modules) m.MapEndpoints(app);
        }

        public void ConfigurePipeline(IApplicationBuilder app)
        {
            foreach (var m in _modules) m.ConfigurePipeline(app);
        }
    }
}

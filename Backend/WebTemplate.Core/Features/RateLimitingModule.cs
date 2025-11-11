using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.RateLimiting;
using WebTemplate.Core.Configuration.Features;

namespace WebTemplate.Core.Features
{
    internal class RateLimitingModule : IFeatureModule
    {
        private readonly FeaturesOptions _options;
        public RateLimitingModule(FeaturesOptions options) => _options = options;
        public string Name => "RateLimiting";
        public void ConfigureOptions(IConfiguration configuration, IServiceCollection services) { }
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddRateLimiter(opt =>
            {
                var cfg = _options.RateLimiting;
                opt.AddPolicy("global", httpContext =>
                    RateLimitPartition.GetFixedWindowLimiter("global", _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = cfg.PermitLimit,
                        Window = TimeSpan.FromSeconds(cfg.WindowSeconds),
                        QueueLimit = cfg.QueueLimit,
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst
                    }));
            });
        }
        public void MapEndpoints(IApplicationBuilder app) { }
        public void ConfigurePipeline(IApplicationBuilder app) => app.UseRateLimiter();
    }
}

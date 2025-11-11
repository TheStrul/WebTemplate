namespace WebTemplate.Core.Features
{
    using Microsoft.AspNetCore.Builder;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using WebTemplate.Core.Configuration.Features;

    internal class SecurityHeadersModule : IFeatureModule
    {
        private readonly FeaturesOptions _options;
        public SecurityHeadersModule(FeaturesOptions options) => _options = options;
        public string Name => "SecurityHeaders";
        public void ConfigureOptions(IConfiguration configuration, IServiceCollection services) { }
        public void ConfigureServices(IServiceCollection services) { }
        public void MapEndpoints(IApplicationBuilder app) { }
        public void ConfigurePipeline(IApplicationBuilder app)
        {
            app.Use(async (ctx, next) =>
            {
                ctx.Response.Headers.TryAdd("X-Content-Type-Options", "nosniff");
                ctx.Response.Headers.TryAdd("X-Frame-Options", "DENY");
                ctx.Response.Headers.TryAdd("Referrer-Policy", "no-referrer");
                if (!string.IsNullOrWhiteSpace(_options.SecurityHeaders.ContentSecurityPolicy))
                {
                    ctx.Response.Headers["Content-Security-Policy"] = _options.SecurityHeaders.ContentSecurityPolicy;
                }
                await next();
            });
        }
    }
}

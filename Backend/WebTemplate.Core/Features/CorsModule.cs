using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WebTemplate.Core.Configuration.Features;

namespace WebTemplate.Core.Features
{
    internal class CorsModule : IFeatureModule
    {
        private readonly FeaturesOptions _options;
        public CorsModule(FeaturesOptions options) => _options = options;
        public string Name => "Cors";
        public void ConfigureOptions(IConfiguration configuration, IServiceCollection services) { }
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCors(opt =>
            {
                opt.AddPolicy("ConfiguredCors", p =>
                {
                    var origins = _options.Cors.AllowedOrigins ?? Array.Empty<string>();
                    if (origins.Length == 0)
                        p.AllowAnyOrigin();
                    else
                        p.WithOrigins(origins).AllowCredentials();
                    p.WithMethods(_options.Cors.AllowedMethods ?? Array.Empty<string>())
                     .WithHeaders(_options.Cors.AllowedHeaders ?? Array.Empty<string>());
                });
            });
        }
        public void MapEndpoints(IApplicationBuilder app) { }
        public void ConfigurePipeline(IApplicationBuilder app) => app.UseCors("ConfiguredCors");
    }
}

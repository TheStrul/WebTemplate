namespace WebTemplate.Core.Features
{
    using Microsoft.AspNetCore.Builder;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;

    public interface IFeatureModule
    {
        string Name { get; }
        void ConfigureOptions(IConfiguration configuration, IServiceCollection services);
        void ConfigureServices(IServiceCollection services);
        void MapEndpoints(IApplicationBuilder app);
        void ConfigurePipeline(IApplicationBuilder app);
    }
}

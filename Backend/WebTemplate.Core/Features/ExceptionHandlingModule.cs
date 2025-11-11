namespace WebTemplate.Core.Features
{
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Diagnostics;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using System.Text.Json;
    using WebTemplate.Core.Configuration.Features;

    internal class ExceptionHandlingModule : IFeatureModule
    {
        private readonly FeaturesOptions _options;
        public ExceptionHandlingModule(FeaturesOptions options) => _options = options;
        public string Name => "ExceptionHandling";
        public void ConfigureOptions(IConfiguration configuration, IServiceCollection services) { }
        public void ConfigureServices(IServiceCollection services) { }
        public void MapEndpoints(IApplicationBuilder app) { }
        public void ConfigurePipeline(IApplicationBuilder app)
        {
            app.UseExceptionHandler(handler =>
            {
                handler.Run(async ctx =>
                {
                    ctx.Response.StatusCode = 500;
                    ctx.Response.ContentType = "application/json";
                    var feature = ctx.Features.Get<IExceptionHandlerFeature>();
                    if (_options.ExceptionHandling.UseProblemDetails)
                    {
                        var payload = new { type = "https://httpstatuses.com/500", title = "Internal Server Error", status = 500, detail = feature?.Error.Message, traceId = ctx.TraceIdentifier };
                        await ctx.Response.WriteAsync(JsonSerializer.Serialize(payload));
                    }
                    else
                    {
                        var payload = new { success = false, message = "An error occurred.", errors = new[] { feature?.Error.Message } };
                        await ctx.Response.WriteAsync(JsonSerializer.Serialize(payload));
                    }
                });
            });
        }
    }
}

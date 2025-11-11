using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;
using WebTemplate.Core.Configuration.Features;

namespace WebTemplate.Core.Features
{
    internal class SerilogModule : IFeatureModule
    {
        private readonly FeaturesOptions _options;
        public SerilogModule(FeaturesOptions options) => _options = options;
        public string Name => "Serilog";
        public void ConfigureOptions(IConfiguration configuration, IServiceCollection services) { }
        public void ConfigureServices(IServiceCollection services)
        {
            if (!_options.Serilog.Enabled) return;
            var level = Enum.TryParse<LogEventLevel>(_options.Serilog.MinimumLevel, out var lvl) ? lvl : LogEventLevel.Information;
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Is(level)
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .CreateLogger();
        }
        public void MapEndpoints(IApplicationBuilder app) { }
        public void ConfigurePipeline(IApplicationBuilder app)
        {
            if (_options.Serilog.Enabled)
            {
                app.ApplicationServices.GetService<ILoggerFactory>()?.AddSerilog();
            }
        }
    }
}

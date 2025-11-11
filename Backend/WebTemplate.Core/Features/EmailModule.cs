using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WebTemplate.Core.Configuration;

namespace WebTemplate.Core.Features
{
    internal class EmailModule : IFeatureModule
    {
        private readonly IConfiguration _configuration;
        public EmailModule(IConfiguration configuration) => _configuration = configuration;
        public string Name => "Email";
        public void ConfigureOptions(IConfiguration configuration, IServiceCollection services)
        {
            services.Configure<EmailSettings>(configuration.GetSection(EmailSettings.SectionName));
        }
        public void ConfigureServices(IServiceCollection services)
        {
            var settings = new EmailSettings();
            _configuration.GetSection(EmailSettings.SectionName).Bind(settings);
            if (!string.Equals(settings.Provider, "Smtp", StringComparison.OrdinalIgnoreCase))
            {
                services.AddScoped<WebTemplate.Core.Interfaces.IEmailSender, WebTemplate.Core.Services.NoOpEmailSender>();
            }
            else
            {
                services.AddScoped<WebTemplate.Core.Interfaces.IEmailSender, WebTemplate.Core.Services.SmtpEmailSender>();
            }
        }
        public void MapEndpoints(IApplicationBuilder app) { }
        public void ConfigurePipeline(IApplicationBuilder app) { }
    }
}

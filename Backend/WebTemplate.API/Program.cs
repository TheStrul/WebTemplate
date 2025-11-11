using WebTemplate.UserModule;
using Microsoft.Extensions.DependencyInjection.Extensions;
using WebTemplate.Core.Features;
using WebTemplate.Core.Configuration.Features;

var builder = WebApplication.CreateBuilder(args);

// Bind and discover feature modules
builder.Services.Configure<FeaturesOptions>(builder.Configuration.GetSection(FeaturesOptions.SectionName));
var featureHost = new FeatureHost(builder.Configuration).Discover();
featureHost.ConfigureOptions(builder.Services);

// Core services
builder.Services.AddControllers();

// Existing options binding
builder.Services.Configure<WebTemplate.Core.Configuration.JwtSettings>(builder.Configuration.GetSection(WebTemplate.Core.Configuration.JwtSettings.SectionName));
builder.Services.Configure<WebTemplate.Core.Configuration.AuthSettings>(builder.Configuration.GetSection(WebTemplate.Core.Configuration.AuthSettings.SectionName));
builder.Services.Configure<WebTemplate.Core.Configuration.EmailSettings>(builder.Configuration.GetSection(WebTemplate.Core.Configuration.EmailSettings.SectionName));
builder.Services.Configure<WebTemplate.Core.Configuration.UserModuleFeatures>(builder.Configuration.GetSection("UserModule:Features"));
builder.Services.Configure<WebTemplate.Core.Configuration.ResponseMessages>(builder.Configuration.GetSection("ResponseMessages"));
builder.Services.Configure<WebTemplate.Core.Configuration.AppUrls>(builder.Configuration.GetSection(WebTemplate.Core.Configuration.AppUrls.SectionName));
builder.Services.Configure<WebTemplate.Core.Configuration.AdminSeedSettings>(builder.Configuration.GetSection(WebTemplate.Core.Configuration.AdminSeedSettings.SectionName));

// Register core services (TryAdd to avoid duplicates when modules register)
builder.Services.TryAddScoped<WebTemplate.Core.Interfaces.IRefreshTokenRepository, WebTemplate.Data.Repositories.RefreshTokenRepository>();
builder.Services.TryAddScoped<WebTemplate.Core.Interfaces.IUserTypeRepository, WebTemplate.Data.Repositories.UserTypeRepository>();
builder.Services.TryAddScoped<WebTemplate.Core.Interfaces.IEmailSender, WebTemplate.Core.Services.SmtpEmailSender>();
builder.Services.TryAddScoped<WebTemplate.Core.Interfaces.ITokenService, WebTemplate.Core.Services.TokenService>();
builder.Services.TryAddScoped<WebTemplate.Core.Interfaces.IAuthService, WebTemplate.Core.Services.AuthService>();
builder.Services.TryAddScoped<WebTemplate.Core.Interfaces.IUserService, WebTemplate.Core.Services.UserService>();
builder.Services.TryAddScoped<WebTemplate.Core.Interfaces.IUserTypeService, WebTemplate.Core.Services.UserTypeService>();

// Conditionally add Identity/Auth module
var features = builder.Configuration.GetSection(FeaturesOptions.SectionName).Get<FeaturesOptions>() ?? new FeaturesOptions();
if (features.IdentityAuth.Enabled)
{
    builder.Services.AddUserModule(builder.Configuration);
}

// Configure services from feature modules
featureHost.ConfigureServices(builder.Services);

var app = builder.Build();

// Pipeline - CRITICAL: UseRouting must come before UseEndpoints
app.UseHttpsRedirection();
app.UseRouting(); // Add routing middleware first
featureHost.ConfigurePipeline(app);

// Conditionally enable Identity/Auth pipeline
if (features.IdentityAuth.Enabled)
{
    app.UseUserModule();
}

// Endpoints - UseEndpoints is called by featureHost.MapEndpoints
app.MapControllers();
featureHost.MapEndpoints(app);

await app.RunAsync();

// Expose Program for tests
namespace WebTemplate.API { public class Program { } }

using Microsoft.Extensions.Configuration;
using WebTemplate.Core.Configuration.Features;
using WebTemplate.Data.Configuration;

namespace WebTemplate.API.Configuration;

/// <summary>
/// API layer configuration implementation that extends DataConfiguration
/// and adds API-specific settings.
/// This is the top-level singleton providing access to all configuration.
/// </summary>
public class ApiConfiguration : DataConfiguration, IApiConfiguration
{
    public FeaturesOptions Features { get; }
    public string AllowedHosts { get; }
    public LoggingSettings Logging { get; }

    public ApiConfiguration(IConfiguration configuration) : base(configuration)
    {
        // Get feature flags - required
        Features = configuration.GetSection(FeaturesOptions.SectionName).Get<FeaturesOptions>()
            ?? throw new InvalidOperationException($"Required configuration section '{FeaturesOptions.SectionName}' is missing or invalid.");

        ValidateFeatures(Features);

        // Get allowed hosts - required, NO FALLBACKS
        AllowedHosts = configuration["AllowedHosts"]
            ?? throw new InvalidOperationException("AllowedHosts is required but not configured.");

        if (string.IsNullOrWhiteSpace(AllowedHosts))
            throw new InvalidOperationException("AllowedHosts cannot be empty.");

        ValidateAllowedHosts(AllowedHosts);

        // Get logging settings (with defaults if not specified)
        Logging = configuration.GetSection(LoggingSettings.SectionName).Get<LoggingSettings>()
            ?? new LoggingSettings();
    }

    private static void ValidateFeatures(FeaturesOptions features)
    {
        // Validate critical features are configured properly
        if (features.IdentityAuth == null)
            throw new InvalidOperationException("Features:IdentityAuth section is missing.");

        if (features.Cors == null)
            throw new InvalidOperationException("Features:Cors section is missing.");

        if (features.Cors.Enabled && (features.Cors.AllowedOrigins == null || features.Cors.AllowedOrigins.Length == 0))
            throw new InvalidOperationException("Features:Cors:AllowedOrigins is required when CORS is enabled.");

        if (features.RateLimiting == null)
            throw new InvalidOperationException("Features:RateLimiting section is missing.");

        if (features.RateLimiting.Enabled && features.RateLimiting.PermitLimit <= 0)
            throw new InvalidOperationException("Features:RateLimiting:PermitLimit must be greater than 0 when rate limiting is enabled.");
    }

    private static void ValidateAllowedHosts(string allowedHosts)
    {
        // Warn if using wildcard in production-like environment
        if (allowedHosts == "*")
        {
            // This is acceptable for development but should be logged as a warning
            Console.WriteLine("WARNING: AllowedHosts is set to '*' which accepts all hosts. This should not be used in production.");
        }
    }
}

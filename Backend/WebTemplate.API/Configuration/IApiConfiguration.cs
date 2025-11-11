using WebTemplate.Core.Configuration.Features;
using WebTemplate.Data.Configuration;

namespace WebTemplate.API.Configuration;

/// <summary>
/// API layer configuration interface extending IDataConfiguration.
/// Provides access to all lower-level configurations plus API-specific settings.
/// This is the top level of the hierarchical configuration system.
/// </summary>
public interface IApiConfiguration : IDataConfiguration
{
    /// <summary>
    /// Feature flags controlling API behavior
    /// </summary>
    FeaturesOptions Features { get; }

    /// <summary>
    /// Allowed hosts for the application
    /// </summary>
    string AllowedHosts { get; }

    /// <summary>
    /// Logging level configuration
    /// </summary>
    LoggingSettings Logging { get; }
}

/// <summary>
/// Logging configuration settings
/// </summary>
public class LoggingSettings
{
    public const string SectionName = "Logging";

    public LogLevelSettings LogLevel { get; set; } = new();
}

/// <summary>
/// Log level configuration for different categories
/// </summary>
public class LogLevelSettings
{
    public string Default { get; set; } = "Information";
    public string MicrosoftAspNetCore { get; set; } = "Warning";
    public string MicrosoftEntityFrameworkCore { get; set; } = "Warning";
}

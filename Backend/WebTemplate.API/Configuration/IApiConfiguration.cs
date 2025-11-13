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

    /// <summary>
    /// Validates logging settings
    /// </summary>
    public Core.Common.Result Validate()
    {
        var errors = new List<Core.Common.Error>();

        if (LogLevel == null)
        {
            errors.Add(Core.Common.Errors.Configuration.RequiredFieldMissing($"{SectionName}:LogLevel"));
            return errors.Any() ? Core.Common.Result.Failure(errors) : Core.Common.Result.Success();
        }

        var logLevelResult = LogLevel.Validate();
        if (logLevelResult.IsFailure)
            errors.AddRange(logLevelResult.Errors);

        return errors.Any() ? Core.Common.Result.Failure(errors) : Core.Common.Result.Success();
    }
}

/// <summary>
/// Log level configuration for different categories
/// </summary>
public class LogLevelSettings
{
    private static readonly string[] ValidLogLevels = { "Trace", "Debug", "Information", "Warning", "Error", "Critical", "None" };

    public string Default { get; set; } = "Information";
    public string MicrosoftAspNetCore { get; set; } = "Warning";
    public string MicrosoftEntityFrameworkCore { get; set; } = "Warning";

    /// <summary>
    /// Validates log level settings
    /// </summary>
    public Core.Common.Result Validate()
    {
        var errors = new List<Core.Common.Error>();

        if (!ValidLogLevels.Contains(Default, StringComparer.OrdinalIgnoreCase))
            errors.Add(Core.Common.Errors.Configuration.InvalidFormat(
                $"{LoggingSettings.SectionName}:LogLevel:Default",
                $"'{Default}' is not a valid log level. Valid values: {string.Join(", ", ValidLogLevels)}"
            ));

        if (!ValidLogLevels.Contains(MicrosoftAspNetCore, StringComparer.OrdinalIgnoreCase))
            errors.Add(Core.Common.Errors.Configuration.InvalidFormat(
                $"{LoggingSettings.SectionName}:LogLevel:MicrosoftAspNetCore",
                $"'{MicrosoftAspNetCore}' is not a valid log level. Valid values: {string.Join(", ", ValidLogLevels)}"
            ));

        if (!ValidLogLevels.Contains(MicrosoftEntityFrameworkCore, StringComparer.OrdinalIgnoreCase))
            errors.Add(Core.Common.Errors.Configuration.InvalidFormat(
                $"{LoggingSettings.SectionName}:LogLevel:MicrosoftEntityFrameworkCore",
                $"'{MicrosoftEntityFrameworkCore}' is not a valid log level. Valid values: {string.Join(", ", ValidLogLevels)}"
            ));

        return errors.Any() ? Core.Common.Result.Failure(errors) : Core.Common.Result.Success();
    }
}

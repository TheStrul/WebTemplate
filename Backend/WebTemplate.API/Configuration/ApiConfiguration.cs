using Microsoft.Extensions.Configuration;
using WebTemplate.Core.Common;
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

        // Get allowed hosts - required, NO FALLBACKS
        AllowedHosts = configuration["AllowedHosts"]
            ?? throw new InvalidOperationException("AllowedHosts is required but not configured.");

        if (string.IsNullOrWhiteSpace(AllowedHosts))
            throw new InvalidOperationException("AllowedHosts cannot be empty.");

        // Get logging settings (with defaults if not specified)
        Logging = configuration.GetSection(LoggingSettings.SectionName).Get<LoggingSettings>()
            ?? new LoggingSettings();

        // Validate all settings (including base DataConfiguration settings)
        var validationResult = ValidateInternal();
        if (validationResult.IsFailure)
        {
            var errorMessages = validationResult.Errors.Select(e => e.Description);
            throw new InvalidOperationException(
                $"Configuration validation failed:{Environment.NewLine}{string.Join(Environment.NewLine, errorMessages)}"
            );
        }
    }

    public override Result Validate() => ValidateInternal();

    private Result ValidateInternal()
    {
        var errors = new List<Error>();

        // Validate base DataConfiguration (Core + Data)
        var baseResult = base.Validate();
        if (baseResult.IsFailure)
            errors.AddRange(baseResult.Errors);

        // Validate Features
        var featuresResult = Features.Validate();
        if (featuresResult.IsFailure)
            errors.AddRange(featuresResult.Errors);

        // Validate AllowedHosts
        if (string.IsNullOrWhiteSpace(AllowedHosts))
            errors.Add(Errors.Configuration.RequiredFieldMissing("AllowedHosts"));

        // Validate Logging
        var loggingResult = Logging.Validate();
        if (loggingResult.IsFailure)
            errors.AddRange(loggingResult.Errors);

        return errors.Any() ? Result.Failure(errors) : Result.Success();
    }
}

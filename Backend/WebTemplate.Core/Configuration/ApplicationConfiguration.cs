using Microsoft.Extensions.Configuration;
using WebTemplate.Core.Common;
using WebTemplate.Core.Configuration.Features;

namespace WebTemplate.Core.Configuration;

/// <summary>
/// Master application configuration - ONE CONFIGURATION TO RULE THEM ALL!
/// Extends CoreConfiguration with server and database settings.
/// Shared by API, E2E tests, and all other projects.
/// NO FALLBACKS - validates all required configuration on construction.
/// </summary>
public class ApplicationConfiguration : CoreConfiguration
{
    /// <summary>
    /// Server configuration (URL, port, health endpoint)
    /// </summary>
    public ServerSettings Server { get; }

    /// <summary>
    /// Database configuration
    /// </summary>
    public DatabaseSettings Database { get; }

    /// <summary>
    /// Feature flags
    /// </summary>
    public FeaturesOptions Features { get; }

    public ApplicationConfiguration(IConfiguration configuration) : base(configuration)
    {
        // Bind Server settings
        Server = configuration.GetSection(ServerSettings.SectionName).Get<ServerSettings>()
            ?? throw new InvalidOperationException($"Required configuration section '{ServerSettings.SectionName}' is missing or invalid.");

        // Bind Database settings
        Database = configuration.GetSection(DatabaseSettings.SectionName).Get<DatabaseSettings>()
            ?? throw new InvalidOperationException($"Required configuration section '{DatabaseSettings.SectionName}' is missing or invalid.");

        // Bind Features settings
        Features = configuration.GetSection(FeaturesOptions.SectionName).Get<FeaturesOptions>()
            ?? new FeaturesOptions(); // Use defaults if not configured

        // Validate all settings (including base CoreConfiguration settings)
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

        // Validate base CoreConfiguration (Auth + Email)
        var baseResult = base.Validate();
        if (baseResult.IsFailure)
            errors.AddRange(baseResult.Errors);

        // Validate Server settings
        var serverResult = ValidateServerSettings(Server);
        if (serverResult.IsFailure)
            errors.AddRange(serverResult.Errors);

        // Validate Database settings
        var databaseResult = ValidateDatabaseSettings(Database);
        if (databaseResult.IsFailure)
            errors.AddRange(databaseResult.Errors);

        // Validate Features settings
        var featuresResult = Features.Validate();
        if (featuresResult.IsFailure)
            errors.AddRange(featuresResult.Errors);

        return errors.Any() ? Result.Failure(errors) : Result.Success();
    }

    private static Result ValidateServerSettings(ServerSettings settings)
    {
        var errors = new List<Error>();

        if (string.IsNullOrWhiteSpace(settings.Url))
            errors.Add(Errors.Configuration.RequiredFieldMissing($"{ServerSettings.SectionName}:Url"));

        if (!string.IsNullOrWhiteSpace(settings.Url) && !Uri.TryCreate(settings.Url, UriKind.Absolute, out _))
            errors.Add(Errors.Configuration.InvalidFormat($"{ServerSettings.SectionName}:Url", "Must be a valid absolute URL"));

        if (settings.ConnectionTimeoutSeconds <= 0)
            errors.Add(Errors.Configuration.ValueOutOfRange($"{ServerSettings.SectionName}:ConnectionTimeoutSeconds", "must be greater than 0"));

        return errors.Any() ? Result.Failure(errors) : Result.Success();
    }

    private static Result ValidateDatabaseSettings(DatabaseSettings settings)
    {
        var errors = new List<Error>();

        if (string.IsNullOrWhiteSpace(settings.ConnectionString))
            errors.Add(Errors.Configuration.RequiredFieldMissing($"{DatabaseSettings.SectionName}:ConnectionString"));

        return errors.Any() ? Result.Failure(errors) : Result.Success();
    }
}

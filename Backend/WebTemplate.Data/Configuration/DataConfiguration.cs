using Microsoft.Extensions.Configuration;
using WebTemplate.Core.Common;
using WebTemplate.Core.Configuration;

namespace WebTemplate.Data.Configuration;

/// <summary>
/// Data layer configuration implementation that extends CoreConfiguration
/// and adds Data-specific settings.
/// </summary>
public class DataConfiguration : CoreConfiguration, IDataConfiguration
{
    public string ConnectionString { get; }
    public DatabaseRetrySettings DatabaseRetry { get; }

    public DataConfiguration(IConfiguration configuration) : base(configuration)
    {
        // Get connection string - required, NO FALLBACKS
        ConnectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException(
                "ConnectionStrings:DefaultConnection is required but not configured. " +
                "Please provide a valid connection string in appsettings.json or environment variables.");

        if (string.IsNullOrWhiteSpace(ConnectionString))
            throw new InvalidOperationException("ConnectionStrings:DefaultConnection cannot be empty.");

        // Get database retry settings (with defaults if not specified)
        DatabaseRetry = configuration.GetSection(DatabaseRetrySettings.SectionName).Get<DatabaseRetrySettings>()
            ?? new DatabaseRetrySettings();

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

        // Validate ConnectionString
        if (string.IsNullOrWhiteSpace(ConnectionString))
            errors.Add(Errors.Configuration.RequiredFieldMissing("ConnectionStrings:DefaultConnection"));

        // Validate DatabaseRetry settings
        var retryResult = DatabaseRetry.Validate();
        if (retryResult.IsFailure)
            errors.AddRange(retryResult.Errors);

        return errors.Any() ? Result.Failure(errors) : Result.Success();
    }
}

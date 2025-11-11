using Microsoft.Extensions.Configuration;
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

        ValidateDatabaseRetrySettings(DatabaseRetry);
    }

    private static void ValidateDatabaseRetrySettings(DatabaseRetrySettings settings)
    {
        if (settings.MaxRetryCount < 0)
            throw new InvalidOperationException("DatabaseRetry:MaxRetryCount cannot be negative.");

        if (settings.MaxRetryCount > 10)
            throw new InvalidOperationException("DatabaseRetry:MaxRetryCount cannot exceed 10 for safety.");

        if (settings.MaxRetryDelaySeconds < 1)
            throw new InvalidOperationException("DatabaseRetry:MaxRetryDelaySeconds must be at least 1 second.");

        if (settings.MaxRetryDelaySeconds > 300)
            throw new InvalidOperationException("DatabaseRetry:MaxRetryDelaySeconds cannot exceed 300 seconds (5 minutes).");
    }
}

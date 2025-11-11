using WebTemplate.Core.Configuration;

namespace WebTemplate.Data.Configuration;

/// <summary>
/// Data layer configuration interface extending ICoreConfiguration.
/// Provides access to Core configurations plus Data-specific settings.
/// </summary>
public interface IDataConfiguration : ICoreConfiguration
{
    /// <summary>
    /// Database connection string
    /// </summary>
    string ConnectionString { get; }

    /// <summary>
    /// Database retry policy settings
    /// </summary>
    DatabaseRetrySettings DatabaseRetry { get; }
}

/// <summary>
/// Database retry policy configuration
/// </summary>
public class DatabaseRetrySettings
{
    public const string SectionName = "DatabaseRetry";

    /// <summary>
    /// Maximum number of retry attempts (default: 5)
    /// </summary>
    public int MaxRetryCount { get; set; } = 5;

    /// <summary>
    /// Maximum delay between retries in seconds (default: 30)
    /// </summary>
    public int MaxRetryDelaySeconds { get; set; } = 30;
}

using WebTemplate.Core.Common;
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

    /// <summary>
    /// Validates database retry settings
    /// </summary>
    public Result Validate()
    {
        var errors = new List<Error>();

        if (MaxRetryCount < 0)
            errors.Add(Errors.Configuration.ValueOutOfRange($"{SectionName}:MaxRetryCount", "cannot be negative"));

        if (MaxRetryCount > 10)
            errors.Add(Errors.Configuration.ValueOutOfRange($"{SectionName}:MaxRetryCount", "cannot exceed 10 for safety"));

        if (MaxRetryDelaySeconds < 1)
            errors.Add(Errors.Configuration.ValueOutOfRange($"{SectionName}:MaxRetryDelaySeconds", "must be at least 1 second"));

        if (MaxRetryDelaySeconds > 300)
            errors.Add(Errors.Configuration.ValueOutOfRange($"{SectionName}:MaxRetryDelaySeconds", "cannot exceed 300 seconds (5 minutes)"));

        return errors.Any() ? Result.Failure(errors) : Result.Success();
    }
}

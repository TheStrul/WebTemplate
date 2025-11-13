namespace WebTemplate.Setup.Models;

/// <summary>
/// Database configuration settings
/// </summary>
public class DatabaseConfiguration
{
    /// <summary>
    /// Database connection string
    /// </summary>
    public string ConnectionString { get; set; } = string.Empty;

    /// <summary>
    /// Execute db-init.sql after project generation
    /// </summary>
    public bool ExecuteInitScript { get; set; } = true;

    /// <summary>
    /// Path to database initialization script (relative to template)
    /// </summary>
    public string InitScriptPath { get; set; } = "Backend/WebTemplate.Data/Migrations/db-init.sql";

    /// <summary>
    /// Test connection before generation
    /// </summary>
    public bool TestConnection { get; set; } = true;

    /// <summary>
    /// Create database if it doesn't exist
    /// </summary>
    public bool CreateDatabaseIfNotExists { get; set; } = true;

    public ValidationResult Validate()
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(ConnectionString))
            errors.Add("Database connection string is required");

        if (ExecuteInitScript && string.IsNullOrWhiteSpace(InitScriptPath))
            errors.Add("Init script path is required when ExecuteInitScript is enabled");

        return new ValidationResult { IsValid = errors.Count == 0, Errors = errors };
    }
}

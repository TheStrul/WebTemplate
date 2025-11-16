namespace WebTemplate.TemplateEngine.Models;

/// <summary>
/// Result from updating configuration files
/// </summary>
public class UpdateResult
{
    /// <summary>
    /// Whether the update operation succeeded
    /// </summary>
    public required bool Success { get; init; }

    /// <summary>
    /// Human-readable message about the operation
    /// </summary>
    public required string Message { get; init; }

    /// <summary>
    /// Number of files modified
    /// </summary>
    public int FilesModified { get; init; }

    /// <summary>
    /// Number of configuration sections updated
    /// </summary>
    public int SectionsUpdated { get; init; }

    /// <summary>
    /// Details of each file that was updated
    /// </summary>
    public List<FileUpdateResult> FileResults { get; init; } = [];

    /// <summary>
    /// Exception that occurred, if any
    /// </summary>
    public Exception? Exception { get; init; }
}

/// <summary>
/// Result from updating a single configuration file
/// </summary>
public class FileUpdateResult
{
    /// <summary>
    /// Path to the file that was updated
    /// </summary>
    public required string FilePath { get; init; }

    /// <summary>
    /// Type of file (appsettings, package.json, markdown, etc.)
    /// </summary>
    public required string FileType { get; init; }

    /// <summary>
    /// Whether this file was successfully updated
    /// </summary>
    public required bool Success { get; init; }

    /// <summary>
    /// Human-readable message about this file's update
    /// </summary>
    public required string Message { get; init; }

    /// <summary>
    /// Number of replacements made in this file
    /// </summary>
    public int ReplacementsCount { get; init; }

    /// <summary>
    /// Exception that occurred, if any
    /// </summary>
    public Exception? Exception { get; init; }
}

/// <summary>
/// Configuration for appsettings.json updates
/// </summary>
public class AppSettingsConfig
{
    /// <summary>
    /// Old connection string database name
    /// </summary>
    public required string OldDatabaseName { get; init; }

    /// <summary>
    /// New connection string database name
    /// </summary>
    public required string NewDatabaseName { get; init; }

    /// <summary>
    /// Old project name for JWT issuer
    /// </summary>
    public required string OldProjectName { get; init; }

    /// <summary>
    /// New project name for JWT issuer
    /// </summary>
    public required string NewProjectName { get; init; }
}

/// <summary>
/// Configuration for package.json updates
/// </summary>
public class PackageJsonConfig
{
    /// <summary>
    /// Old package name (kebab-case)
    /// </summary>
    public required string OldName { get; init; }

    /// <summary>
    /// New package name (kebab-case)
    /// </summary>
    public required string NewName { get; init; }

    /// <summary>
    /// Old description
    /// </summary>
    public string? OldDescription { get; init; }

    /// <summary>
    /// New description
    /// </summary>
    public string? NewDescription { get; init; }
}

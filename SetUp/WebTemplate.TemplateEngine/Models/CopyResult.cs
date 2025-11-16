namespace WebTemplate.TemplateEngine.Models;

/// <summary>
/// Result from copying template files
/// </summary>
public class CopyResult
{
    /// <summary>
    /// Whether the copy operation succeeded
    /// </summary>
    public required bool Success { get; init; }

    /// <summary>
    /// Human-readable message about the copy operation
    /// </summary>
    public required string Message { get; init; }

    /// <summary>
    /// Number of files copied
    /// </summary>
    public int FileCount { get; init; }

    /// <summary>
    /// Number of directories created
    /// </summary>
    public int DirectoryCount { get; init; }

    /// <summary>
    /// Total bytes copied
    /// </summary>
    public long TotalBytes { get; init; }

    /// <summary>
    /// Exception that occurred, if any
    /// </summary>
    public Exception? Exception { get; init; }
}

namespace WebTemplate.TemplateEngine.Models;

/// <summary>
/// Result from the file and directory rebranding operation
/// </summary>
public class RebrandResult
{
    /// <summary>
    /// Whether the rebranding operation succeeded
    /// </summary>
    public required bool Success { get; init; }

    /// <summary>
    /// Human-readable message about the operation
    /// </summary>
    public required string Message { get; init; }

    /// <summary>
    /// Number of files and directories renamed
    /// </summary>
    public int ItemsRenamed { get; init; }

    /// <summary>
    /// Number of files with content modified
    /// </summary>
    public int FilesModified { get; init; }

    /// <summary>
    /// Total bytes of content that was modified
    /// </summary>
    public long BytesModified { get; init; }

    /// <summary>
    /// Number of text files processed
    /// </summary>
    public int TextFilesProcessed { get; init; }

    /// <summary>
    /// Number of binary files skipped
    /// </summary>
    public int BinaryFilesSkipped { get; init; }

    /// <summary>
    /// Exception that occurred, if any
    /// </summary>
    public Exception? Exception { get; init; }
}

/// <summary>
/// Result from renaming directories and files
/// </summary>
public class RenameResult
{
    /// <summary>
    /// Whether the rename operation succeeded
    /// </summary>
    public required bool Success { get; init; }

    /// <summary>
    /// Number of items (files and directories) renamed
    /// </summary>
    public int ItemsRenamed { get; init; }

    /// <summary>
    /// Names of items that failed to rename
    /// </summary>
    public List<string> FailedItems { get; init; } = [];

    /// <summary>
    /// Exception that occurred, if any
    /// </summary>
    public Exception? Exception { get; init; }
}

/// <summary>
/// Result from updating file contents
/// </summary>
public class ContentUpdateResult
{
    /// <summary>
    /// Whether the content update operation succeeded
    /// </summary>
    public required bool Success { get; init; }

    /// <summary>
    /// Number of files modified
    /// </summary>
    public int FilesModified { get; init; }

    /// <summary>
    /// Total bytes modified across all files
    /// </summary>
    public long BytesModified { get; init; }

    /// <summary>
    /// Number of text files processed
    /// </summary>
    public int TextFilesProcessed { get; init; }

    /// <summary>
    /// Number of binary files skipped
    /// </summary>
    public int BinaryFilesSkipped { get; init; }

    /// <summary>
    /// Names of files that failed to update
    /// </summary>
    public List<string> FailedFiles { get; init; } = [];

    /// <summary>
    /// Exception that occurred, if any
    /// </summary>
    public Exception? Exception { get; init; }
}

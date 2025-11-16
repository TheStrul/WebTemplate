namespace WebTemplate.TemplateEngine.Models;

/// <summary>
/// Result from Git repository initialization
/// </summary>
public class GitResult
{
    /// <summary>
    /// Whether the git operation succeeded
    /// </summary>
    public required bool Success { get; init; }

    /// <summary>
    /// Human-readable message about the operation
    /// </summary>
    public required string Message { get; init; }

    /// <summary>
    /// Whether a git repository was initialized
    /// </summary>
    public bool RepositoryInitialized { get; init; }

    /// <summary>
    /// Whether an initial commit was created
    /// </summary>
    public bool InitialCommitCreated { get; init; }

    /// <summary>
    /// Output from git commands
    /// </summary>
    public string? CommandOutput { get; init; }

    /// <summary>
    /// Exception that occurred, if any
    /// </summary>
    public Exception? Exception { get; init; }

    /// <summary>
    /// Creates a successful git result
    /// </summary>
    public static GitResult Successful(string message) => new()
    {
        Success = true,
        Message = message,
        RepositoryInitialized = true
    };

    /// <summary>
    /// Creates a failed git result
    /// </summary>
    public static GitResult Failed(string message, Exception? ex = null) => new()
    {
        Success = false,
        Message = message,
        Exception = ex
    };
}

/// <summary>
/// Result from running a git command
/// </summary>
public class CommandResult
{
    /// <summary>
    /// Whether the command succeeded (exit code 0)
    /// </summary>
    public required bool Success { get; init; }

    /// <summary>
    /// Exit code from the process
    /// </summary>
    public required int ExitCode { get; init; }

    /// <summary>
    /// Standard output from the command
    /// </summary>
    public required string Output { get; init; }

    /// <summary>
    /// Standard error from the command
    /// </summary>
    public required string Error { get; init; }

    /// <summary>
    /// The git command that was executed
    /// </summary>
    public required string Command { get; init; }
}

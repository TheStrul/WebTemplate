using Microsoft.Extensions.Logging;
using System.Diagnostics;
using WebTemplate.TemplateEngine.Models;

namespace WebTemplate.TemplateEngine;

/// <summary>
/// Helper class for initializing git repositories in generated projects
/// </summary>
public class GitInitializer
{
    private readonly ILogger<GitInitializer> _logger;

    public GitInitializer(ILogger<GitInitializer> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Initializes a git repository in the target directory
    /// </summary>
    public async Task<GitResult> InitializeRepositoryAsync(
        string targetPath,
        string projectName,
        bool createInitialCommit,
        IProgress<string>? progress = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Initializing git repository in {TargetPath}", targetPath);

            // Check if git is available
            if (!await IsGitAvailableAsync(cancellationToken))
            {
                _logger.LogWarning("Git is not available or not in PATH");
                return GitResult.Failed("Git is not available or not installed");
            }

            progress?.Report("Initializing git repository...");

            // Initialize repository
            var initResult = await RunGitCommandAsync("init", targetPath, cancellationToken);

            if (!initResult.Success)
            {
                _logger.LogError("Git init failed with exit code {ExitCode}: {Error}",
                    initResult.ExitCode, initResult.Error);
                return GitResult.Failed($"Git init failed: {initResult.Error}");
            }

            _logger.LogInformation("Git repository initialized successfully");

            // Create initial commit if requested
            if (createInitialCommit)
            {
                progress?.Report("Creating initial commit...");

                var addResult = await RunGitCommandAsync("add .", targetPath, cancellationToken);

                if (!addResult.Success)
                {
                    _logger.LogWarning("Git add failed, continuing without commit");
                    return GitResult.Successful("Repository initialized, but commit creation failed");
                }

                var commitResult = await RunGitCommandAsync(
                    $"commit -m \"Initial commit for {projectName}\"",
                    targetPath,
                    cancellationToken);

                if (!commitResult.Success)
                {
                    _logger.LogWarning("Git commit failed: {Error}", commitResult.Error);
                    return GitResult.Successful("Repository initialized, but initial commit failed");
                }

                _logger.LogInformation("Initial commit created successfully");

                return new GitResult
                {
                    Success = true,
                    Message = "Git repository initialized with initial commit",
                    RepositoryInitialized = true,
                    InitialCommitCreated = true
                };
            }

            return new GitResult
            {
                Success = true,
                Message = "Git repository initialized successfully",
                RepositoryInitialized = true,
                InitialCommitCreated = false
            };
        }
        catch (OperationCanceledException ex)
        {
            _logger.LogWarning(ex, "Git initialization was cancelled");
            return GitResult.Failed("Git initialization was cancelled", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Git initialization failed with exception");
            return GitResult.Failed($"Git initialization failed: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Checks if git is available in the PATH
    /// </summary>
    private async Task<bool> IsGitAvailableAsync(CancellationToken cancellationToken)
    {
        try
        {
            var result = await RunGitCommandAsync("--version", "", cancellationToken);
            return result.Success;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Runs a git command and returns the result
    /// </summary>
    private async Task<CommandResult> RunGitCommandAsync(
        string arguments,
        string workingDirectory,
        CancellationToken cancellationToken)
    {
        var tcs = new TaskCompletionSource<CommandResult>();

        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = "git",
                Arguments = arguments,
                WorkingDirectory = workingDirectory,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            _logger.LogDebug("Running git command: git {Arguments} in {WorkingDirectory}",
                arguments, workingDirectory);

            var process = new Process { StartInfo = psi };

            // Create a task that completes when the process exits
            var processExited = new TaskCompletionSource<bool>();

            process.EnableRaisingEvents = true;
            process.Exited += (s, e) => processExited.TrySetResult(true);

            var output = new System.Collections.Generic.List<string>();
            var error = new System.Collections.Generic.List<string>();

            process.OutputDataReceived += (s, e) =>
            {
                if (e.Data != null)
                {
                    output.Add(e.Data);
                }
            };

            process.ErrorDataReceived += (s, e) =>
            {
                if (e.Data != null)
                {
                    error.Add(e.Data);
                }
            };

            if (!process.Start())
            {
                return new CommandResult
                {
                    Success = false,
                    ExitCode = -1,
                    Output = "",
                    Error = "Failed to start git process",
                    Command = arguments
                };
            }

            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            // Wait for process to exit with cancellation support
            using (cancellationToken.Register(() => 
            {
                try
                {
                    if (!process.HasExited)
                    {
                        process.Kill();
                    }
                }
                catch { }
            }))
            {
                await processExited.Task;
            }

            cancellationToken.ThrowIfCancellationRequested();

            var exitCode = process.ExitCode;
            var outputString = string.Join("\n", output);
            var errorString = string.Join("\n", error);

            _logger.LogDebug("Git command completed with exit code {ExitCode}", exitCode);

            return new CommandResult
            {
                Success = exitCode == 0,
                ExitCode = exitCode,
                Output = outputString,
                Error = errorString,
                Command = arguments
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to run git command: {Arguments}", arguments);
            return new CommandResult
            {
                Success = false,
                ExitCode = -1,
                Output = "",
                Error = ex.Message,
                Command = arguments
            };
        }
    }
}

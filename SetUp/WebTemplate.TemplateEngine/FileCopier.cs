using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;
using WebTemplate.TemplateEngine.Models;

namespace WebTemplate.TemplateEngine;

/// <summary>
/// Helper class for copying template files with exclusion support
/// </summary>
public class FileCopier
{
    private readonly ILogger<FileCopier> _logger;

    public FileCopier(ILogger<FileCopier> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Copies template files from source to target, respecting inclusion and exclusion filters
    /// </summary>
    public async Task<CopyResult> CopyTemplateAsync(
        string sourcePath,
        string targetPath,
        List<string> includeFolders,
        List<string> excludeDirs,
        List<string> excludeFiles,
        IProgress<string>? progress = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Validate source path exists
            if (!Directory.Exists(sourcePath))
            {
                _logger.LogError("Source path does not exist: {SourcePath}", sourcePath);
                return new CopyResult
                {
                    Success = false,
                    Message = $"Source path does not exist: {sourcePath}"
                };
            }

            _logger.LogInformation("Starting template copy from {SourcePath} to {TargetPath}", sourcePath, targetPath);

            // Create target directory
            Directory.CreateDirectory(targetPath);
            _logger.LogDebug("Created target directory: {TargetPath}", targetPath);

            int fileCount = 0;
            int dirCount = 0;
            long totalBytes = 0;

            // Copy each included folder
            foreach (var folder in includeFolders)
            {
                var sourceFolderPath = Path.Combine(sourcePath, folder);
                if (!Directory.Exists(sourceFolderPath))
                {
                    _logger.LogWarning("Included folder not found, skipping: {FolderPath}", sourceFolderPath);
                    continue;
                }

                _logger.LogInformation("Copying included folder: {Folder}", folder);
                progress?.Report($"Copying {folder}...");

                var result = await CopyDirectoryRecursiveAsync(
                    sourceFolderPath,
                    Path.Combine(targetPath, folder),
                    excludeDirs,
                    excludeFiles,
                    progress,
                    cancellationToken);

                fileCount += result.FilesCopied;
                dirCount += result.DirectoriesCreated;
                totalBytes += result.BytesCopied;
            }

            var message = $"Copied {fileCount} files in {dirCount} directories ({FormatBytes(totalBytes)})";
            _logger.LogInformation("Template copy completed: {Message}", message);

            return new CopyResult
            {
                Success = true,
                Message = message,
                FileCount = fileCount,
                DirectoryCount = dirCount,
                TotalBytes = totalBytes
            };
        }
        catch (OperationCanceledException ex)
        {
            _logger.LogWarning(ex, "Template copy was cancelled");
            return new CopyResult
            {
                Success = false,
                Message = "Copy operation was cancelled",
                Exception = ex
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Template copy failed with exception");
            return new CopyResult
            {
                Success = false,
                Message = $"Copy operation failed: {ex.Message}",
                Exception = ex
            };
        }
    }

    /// <summary>
    /// Recursively copies a directory and its contents, applying exclusion filters
    /// </summary>
    private async Task<(int FilesCopied, int DirectoriesCreated, long BytesCopied)> CopyDirectoryRecursiveAsync(
        string sourceDir,
        string targetDir,
        List<string> excludeDirs,
        List<string> excludeFiles,
        IProgress<string>? progress,
        CancellationToken cancellationToken)
    {
        int fileCount = 0;
        int dirCount = 0;
        long bytesCount = 0;

        try
        {
            // Create target directory
            Directory.CreateDirectory(targetDir);
            dirCount++;

            // Get source directory info
            var sourceInfo = new DirectoryInfo(sourceDir);

            // Copy files
            foreach (var file in sourceInfo.GetFiles())
            {
                cancellationToken.ThrowIfCancellationRequested();

                // Check if file should be excluded
                if (ShouldExcludeFile(file.Name, excludeFiles))
                {
                    _logger.LogDebug("Excluding file: {FileName}", file.Name);
                    continue;
                }

                var targetFilePath = Path.Combine(targetDir, file.Name);

                try
                {
                    File.Copy(file.FullName, targetFilePath, overwrite: true);
                    fileCount++;
                    bytesCount += file.Length;
                    _logger.LogDebug("Copied file: {FileName} ({Size} bytes)", file.Name, file.Length);
                }
                catch (IOException ex)
                {
                    _logger.LogWarning(ex, "Failed to copy file, retrying: {FilePath}", file.FullName);
                    // Retry once
                    await Task.Delay(100, cancellationToken);
                    File.Copy(file.FullName, targetFilePath, overwrite: true);
                    fileCount++;
                    bytesCount += file.Length;
                }
            }

            // Copy subdirectories recursively
            foreach (var dir in sourceInfo.GetDirectories())
            {
                cancellationToken.ThrowIfCancellationRequested();

                // Check if directory should be excluded
                if (ShouldExcludeDirectory(dir.Name, excludeDirs))
                {
                    _logger.LogDebug("Excluding directory: {DirectoryName}", dir.Name);
                    continue;
                }

                var targetSubDir = Path.Combine(targetDir, dir.Name);
                var subResult = await CopyDirectoryRecursiveAsync(
                    dir.FullName,
                    targetSubDir,
                    excludeDirs,
                    excludeFiles,
                    progress,
                    cancellationToken);

                fileCount += subResult.FilesCopied;
                dirCount += subResult.DirectoriesCreated;
                bytesCount += subResult.BytesCopied;
            }

            return (fileCount, dirCount, bytesCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error copying directory: {SourceDir}", sourceDir);
            throw;
        }
    }

    /// <summary>
    /// Determines if a file should be excluded based on patterns
    /// </summary>
    private bool ShouldExcludeFile(string fileName, List<string> excludePatterns)
    {
        return excludePatterns.Any(pattern =>
            Regex.IsMatch(fileName, ConvertPatternToRegex(pattern), RegexOptions.IgnoreCase));
    }

    /// <summary>
    /// Determines if a directory should be excluded
    /// </summary>
    private bool ShouldExcludeDirectory(string dirName, List<string> excludeDirs)
    {
        return excludeDirs.Any(excluded =>
            excluded.Equals(dirName, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Converts a wildcard pattern (e.g., "*.log") to a regex pattern
    /// </summary>
    private static string ConvertPatternToRegex(string pattern)
    {
        var regexPattern = "^" + Regex.Escape(pattern)
            .Replace("\\*", ".*")
            .Replace("\\?", ".")
            + "$";
        return regexPattern;
    }

    /// <summary>
    /// Formats bytes as human-readable string
    /// </summary>
    private static string FormatBytes(long bytes)
    {
        string[] sizes = { "B", "KB", "MB", "GB", "TB" };
        double len = bytes;
        int order = 0;

        while (len >= 1024 && order < sizes.Length - 1)
        {
            order++;
            len = len / 1024;
        }

        return $"{len:F2} {sizes[order]}";
    }
}

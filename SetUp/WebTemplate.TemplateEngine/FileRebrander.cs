using Microsoft.Extensions.Logging;
using System.Text;
using WebTemplate.TemplateEngine.Models;

namespace WebTemplate.TemplateEngine;

/// <summary>
/// Helper class for rebranding files, directories, and file contents
/// </summary>
public class FileRebrander
{
    private readonly ILogger<FileRebrander> _logger;

    // Binary file extensions that should not have content updated
    private static readonly HashSet<string> BinaryExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".dll", ".exe", ".obj", ".o",
        ".png", ".jpg", ".jpeg", ".gif", ".bmp", ".ico", ".svg", ".webp",
        ".mp3", ".mp4", ".wav", ".avi", ".mkv", ".mov", ".flv",
        ".pdf", ".zip", ".rar", ".7z", ".tar", ".gz", ".iso",
        ".bin", ".hex", ".dat", ".db", ".sqlite", ".mdb",
        ".class", ".jar", ".pyc", ".so"
    };

    public FileRebrander(ILogger<FileRebrander> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Rebrands all files, directories, and file contents from old name to new name
    /// </summary>
    public async Task<RebrandResult> RebrandAsync(
        string targetPath,
        string oldName,
        string newName,
        IProgress<string>? progress = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Starting rebranding: {OldName} -> {NewName} in {TargetPath}",
                oldName, newName, targetPath);

            int itemsRenamed = 0;
            int filesModified = 0;
            long bytesModified = 0;

            // Step 1: Rename files and directories (deepest first to avoid parent path issues)
            _logger.LogInformation("Phase 1: Renaming files and directories");
            progress?.Report("Renaming files and directories...");

            var renameResult = await RenameStructureAsync(
                targetPath, oldName, newName, progress, cancellationToken);

            if (!renameResult.Success)
            {
                _logger.LogError("File/directory rename failed with {FailedCount} failures", renameResult.FailedItems.Count);
                return new RebrandResult
                {
                    Success = false,
                    Message = $"Rename failed: {string.Join(", ", renameResult.FailedItems.Take(5))}...",
                    Exception = renameResult.Exception
                };
            }

            itemsRenamed = renameResult.ItemsRenamed;

            // Step 2: Update file contents
            _logger.LogInformation("Phase 2: Updating file contents");
            progress?.Report("Updating file contents...");

            var contentResult = await UpdateFileContentsAsync(
                targetPath, oldName, newName, progress, cancellationToken);

            if (!contentResult.Success)
            {
                _logger.LogError("Content update failed with {FailedCount} failures", contentResult.FailedFiles.Count);
                return new RebrandResult
                {
                    Success = false,
                    Message = $"Content update failed: {string.Join(", ", contentResult.FailedFiles.Take(5))}...",
                    Exception = contentResult.Exception
                };
            }

            filesModified = contentResult.FilesModified;
            bytesModified = contentResult.BytesModified;

            var message = $"Rebranded {itemsRenamed} items, modified {filesModified} files ({bytesModified} bytes)";
            _logger.LogInformation("Rebranding completed: {Message}", message);

            return new RebrandResult
            {
                Success = true,
                Message = message,
                ItemsRenamed = itemsRenamed,
                FilesModified = filesModified,
                BytesModified = bytesModified,
                TextFilesProcessed = contentResult.TextFilesProcessed,
                BinaryFilesSkipped = contentResult.BinaryFilesSkipped
            };
        }
        catch (OperationCanceledException ex)
        {
            _logger.LogWarning(ex, "Rebranding was cancelled");
            return new RebrandResult
            {
                Success = false,
                Message = "Rebranding was cancelled",
                Exception = ex
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Rebranding failed with exception");
            return new RebrandResult
            {
                Success = false,
                Message = $"Rebranding failed: {ex.Message}",
                Exception = ex
            };
        }
    }

    /// <summary>
    /// Renames files and directories from old name to new name
    /// Processes deepest items first to avoid parent path changes affecting child paths
    /// </summary>
    private async Task<RenameResult> RenameStructureAsync(
        string targetPath,
        string oldName,
        string newName,
        IProgress<string>? progress,
        CancellationToken cancellationToken)
    {
        try
        {
            int renamedCount = 0;
            var failedItems = new List<string>();
            var processedPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            // Get all files and directories recursively
            var allItems = new List<(string Path, bool IsDirectory, int Depth)>();

            CollectItemsRecursive(targetPath, targetPath, ref allItems);

            // Sort by depth descending (deepest first), then by path for consistency
            var sortedItems = allItems
                .OrderByDescending(x => x.Depth)
                .ThenByDescending(x => x.Path.Length)
                .ToList();

            _logger.LogDebug("Found {ItemCount} items to potentially rename", sortedItems.Count);

            foreach (var (itemPath, isDirectory, _) in sortedItems)
            {
                cancellationToken.ThrowIfCancellationRequested();

                // Skip if already processed (in case of renames affecting the path)
                if (processedPaths.Contains(itemPath))
                {
                    continue;
                }

                var itemName = Path.GetFileName(itemPath);
                if (!itemName.Contains(oldName, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                var newItemName = itemName.Replace(oldName, newName, StringComparison.OrdinalIgnoreCase);
                var parentDir = Path.GetDirectoryName(itemPath);

                if (parentDir == null)
                {
                    continue;
                }

                var newPath = Path.Combine(parentDir, newItemName);

                try
                {
                    if (isDirectory)
                    {
                        // Check if destination exists to avoid accidental overwrites
                        if (Directory.Exists(newPath))
                        {
                            _logger.LogWarning("Destination directory already exists: {NewPath}", newPath);
                            failedItems.Add($"{itemName} (destination exists)");
                            continue;
                        }
                        
                        Directory.Move(itemPath, newPath);
                        _logger.LogDebug("Renamed directory: {OldPath} -> {NewPath}", itemPath, newPath);
                    }
                    else
                    {
                        // Check if destination exists
                        if (File.Exists(newPath))
                        {
                            _logger.LogWarning("Destination file already exists: {NewPath}", newPath);
                            failedItems.Add($"{itemName} (destination exists)");
                            continue;
                        }
                        
                        File.Move(itemPath, newPath);
                        _logger.LogDebug("Renamed file: {OldPath} -> {NewPath}", itemPath, newPath);
                    }

                    renamedCount++;
                    processedPaths.Add(newPath);
                }
                catch (IOException ex)
                {
                    _logger.LogWarning(ex, "Failed to rename {ItemType}: {ItemPath}",
                        isDirectory ? "directory" : "file", itemPath);
                    failedItems.Add($"{itemName} ({ex.Message})");
                }
            }

            _logger.LogInformation("Renamed {Count} items", renamedCount);

            return new RenameResult
            {
                Success = failedItems.Count == 0,
                ItemsRenamed = renamedCount,
                FailedItems = failedItems
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Rename structure operation failed");
            return new RenameResult
            {
                Success = false,
                ItemsRenamed = 0,
                FailedItems = [ex.Message],
                Exception = ex
            };
        }
    }

    /// <summary>
    /// Recursively collects all files and directories with their depth information
    /// </summary>
    private void CollectItemsRecursive(
        string currentPath,
        string basePath,
        ref List<(string Path, bool IsDirectory, int Depth)> items)
    {
        try
        {
            var dirInfo = new DirectoryInfo(currentPath);
            var depth = currentPath.Count(c => c == Path.DirectorySeparatorChar) -
                       basePath.Count(c => c == Path.DirectorySeparatorChar);

            // Collect files
            foreach (var file in dirInfo.GetFiles())
            {
                items.Add((file.FullName, false, depth + 1));
            }

            // Collect directories and recurse
            foreach (var dir in dirInfo.GetDirectories())
            {
                items.Add((dir.FullName, true, depth + 1));
                CollectItemsRecursive(dir.FullName, basePath, ref items);
            }
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Access denied while collecting items in {Path}", currentPath);
        }
    }

    /// <summary>
    /// Updates file contents, replacing old name with new name
    /// Skips binary files based on extension
    /// </summary>
    private async Task<ContentUpdateResult> UpdateFileContentsAsync(
        string targetPath,
        string oldName,
        string newName,
        IProgress<string>? progress,
        CancellationToken cancellationToken)
    {
        try
        {
            int filesModified = 0;
            int textFilesProcessed = 0;
            int binaryFilesSkipped = 0;
            long bytesModified = 0;
            var failedFiles = new List<string>();

            // Get all files recursively
            var allFiles = Directory.GetFiles(targetPath, "*", SearchOption.AllDirectories);

            _logger.LogDebug("Found {FileCount} files to process for content updates", allFiles.Length);

            foreach (var filePath in allFiles)
            {
                cancellationToken.ThrowIfCancellationRequested();

                // Skip binary files
                if (IsBinaryFile(filePath))
                {
                    binaryFilesSkipped++;
                    continue;
                }

                textFilesProcessed++;

                try
                {
                    var originalContent = await File.ReadAllTextAsync(filePath, Encoding.UTF8, cancellationToken);

                    // Only update if content contains the old name
                    if (!originalContent.Contains(oldName, StringComparison.Ordinal))
                    {
                        continue;
                    }

                    var updatedContent = originalContent.Replace(oldName, newName);

                    // Only write if content actually changed
                    if (originalContent.Equals(updatedContent))
                    {
                        continue;
                    }

                    var originalBytes = Encoding.UTF8.GetByteCount(originalContent);
                    var updatedBytes = Encoding.UTF8.GetByteCount(updatedContent);
                    var bytesDiff = Math.Abs(updatedBytes - originalBytes);

                    await File.WriteAllTextAsync(filePath, updatedContent, Encoding.UTF8, cancellationToken);

                    filesModified++;
                    bytesModified += bytesDiff;

                    _logger.LogDebug("Updated file: {FilePath} ({BytesDiff} bytes)", filePath, bytesDiff);
                }
                catch (IOException ex)
                {
                    _logger.LogWarning(ex, "Failed to update file content: {FilePath}", filePath);
                    failedFiles.Add($"{Path.GetFileName(filePath)} ({ex.Message})");
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Unexpected error updating file: {FilePath}", filePath);
                    failedFiles.Add($"{Path.GetFileName(filePath)} ({ex.GetType().Name})");
                }
            }

            _logger.LogInformation("Updated {ModifiedCount} files, skipped {SkippedCount} binary files, " +
                "processed {ProcessedCount} text files ({BytesModified} bytes)",
                filesModified, binaryFilesSkipped, textFilesProcessed, bytesModified);

            return new ContentUpdateResult
            {
                Success = failedFiles.Count == 0,
                FilesModified = filesModified,
                BytesModified = bytesModified,
                TextFilesProcessed = textFilesProcessed,
                BinaryFilesSkipped = binaryFilesSkipped,
                FailedFiles = failedFiles
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Content update operation failed");
            return new ContentUpdateResult
            {
                Success = false,
                FilesModified = 0,
                FailedFiles = [ex.Message]
            };
        }
    }

    /// <summary>
    /// Determines if a file is binary based on its extension
    /// </summary>
    private bool IsBinaryFile(string filePath)
    {
        var extension = Path.GetExtension(filePath);
        return BinaryExtensions.Contains(extension);
    }
}

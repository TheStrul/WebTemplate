using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using WebTemplate.TemplateEngine.Models;

namespace WebTemplate.TemplateEngine;

/// <summary>
/// Helper class for updating configuration files after rebranding
/// </summary>
public class ConfigurationUpdater
{
    private readonly ILogger<ConfigurationUpdater> _logger;

    // JSON serializer options for consistent formatting
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public ConfigurationUpdater(ILogger<ConfigurationUpdater> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Updates all configuration files in the generated project
    /// </summary>
    public async Task<UpdateResult> UpdateConfigurationsAsync(
        TemplateContext context,
        IProgress<string>? progress = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Starting configuration updates in {TargetPath}", context.TargetPath);

            var fileResults = new List<FileUpdateResult>();
            var appSettingsConfig = new AppSettingsConfig
            {
                OldDatabaseName = "WebTemplateDb",
                NewDatabaseName = context.DatabaseName,
                OldProjectName = context.OldProjectName,
                NewProjectName = context.NewProjectName
            };

            var packageJsonConfig = new PackageJsonConfig
            {
                OldName = "webtemplate-frontend",
                NewName = context.NewProjectName.ToLower() + "-frontend",
                OldDescription = "WebTemplate Frontend Application",
                NewDescription = $"{context.NewProjectName} Frontend Application"
            };

            // Update appsettings.json files
            progress?.Report("Updating appsettings.json files...");
            var appSettingsResults = await UpdateAppSettingsFilesAsync(
                context.TargetPath, appSettingsConfig, cancellationToken);
            fileResults.AddRange(appSettingsResults);

            // Update package.json files
            progress?.Report("Updating package.json files...");
            var packageJsonResults = await UpdatePackageJsonFilesAsync(
                context.TargetPath, packageJsonConfig, cancellationToken);
            fileResults.AddRange(packageJsonResults);

            // Update README.md files
            progress?.Report("Updating README files...");
            var readmeResults = await UpdateReadmeFilesAsync(
                context.TargetPath, context.NewProjectName, cancellationToken);
            fileResults.AddRange(readmeResults);

            // Update copilot-instructions.md if present
            progress?.Report("Updating copilot-instructions.md...");
            var copilotResults = await UpdateCopilotInstructionsAsync(
                context.TargetPath, context.OldProjectName, context.NewProjectName, cancellationToken);
            fileResults.AddRange(copilotResults);

            var successCount = fileResults.Count(r => r.Success);
            var failureCount = fileResults.Count(r => !r.Success);

            if (failureCount > 0)
            {
                _logger.LogWarning("Configuration updates completed with {FailureCount} failures", failureCount);
            }

            var message = $"Updated {successCount} files, {failureCount} failures";
            _logger.LogInformation("Configuration updates completed: {Message}", message);

            return new UpdateResult
            {
                Success = failureCount == 0,
                Message = message,
                FilesModified = successCount,
                SectionsUpdated = fileResults.Sum(r => r.ReplacementsCount),
                FileResults = fileResults
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Configuration update operation failed");
            return new UpdateResult
            {
                Success = false,
                Message = $"Configuration update failed: {ex.Message}",
                Exception = ex
            };
        }
    }

    /// <summary>
    /// Updates appsettings.json files with new connection strings and JWT settings
    /// </summary>
    private async Task<List<FileUpdateResult>> UpdateAppSettingsFilesAsync(
        string targetPath,
        AppSettingsConfig config,
        CancellationToken cancellationToken)
    {
        var results = new List<FileUpdateResult>();

        try
        {
            var appSettingsFiles = Directory.GetFiles(
                targetPath, "appsettings*.json", SearchOption.AllDirectories);

            _logger.LogDebug("Found {FileCount} appsettings files to update", appSettingsFiles.Length);

            foreach (var filePath in appSettingsFiles)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var result = await UpdateAppSettingsFileAsync(filePath, config, cancellationToken);
                results.Add(result);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating appsettings files");
            results.Add(new FileUpdateResult
            {
                FilePath = Path.Combine(targetPath, "appsettings.json"),
                FileType = "appsettings",
                Success = false,
                Message = $"Error: {ex.Message}",
                Exception = ex
            });
        }

        return results;
    }

    /// <summary>
    /// Updates a single appsettings file
    /// </summary>
    private async Task<FileUpdateResult> UpdateAppSettingsFileAsync(
        string filePath,
        AppSettingsConfig config,
        CancellationToken cancellationToken)
    {
        try
        {
            var content = await File.ReadAllTextAsync(filePath, Encoding.UTF8, cancellationToken);
            var node = JsonNode.Parse(content);

            if (node == null)
            {
                return new FileUpdateResult
                {
                    FilePath = filePath,
                    FileType = "appsettings",
                    Success = false,
                    Message = "Invalid JSON structure"
                };
            }

            int replacements = 0;

            // Update ConnectionStrings
            var connStrings = node["ConnectionStrings"];
            if (connStrings != null)
            {
                var defaultConn = connStrings["DefaultConnection"];
                if (defaultConn != null)
                {
                    var oldValue = defaultConn.GetValue<string>();
                    if (oldValue != null)
                    {
                        // Replace database name with project-specific name
                        var newValue = oldValue
                            .Replace("WebTemplateDb_Dev", $"{config.NewDatabaseName}_Dev")
                            .Replace("WebTemplateDb", config.NewDatabaseName);

                        if (oldValue != newValue)
                        {
                            connStrings["DefaultConnection"] = newValue;
                            replacements++;
                            _logger.LogDebug("Updated DefaultConnection in {FilePath}", filePath);
                        }
                    }
                }
            }

            // Update JWT settings
            var jwt = node["Jwt"];
            if (jwt != null)
            {
                var issuer = jwt["Issuer"];
                if (issuer != null)
                {
                    var oldValue = issuer.GetValue<string>();
                    if (oldValue?.Contains(config.OldProjectName) == true)
                    {
                        var newValue = oldValue.Replace(config.OldProjectName, config.NewProjectName);
                        jwt["Issuer"] = newValue;
                        replacements++;
                        _logger.LogDebug("Updated JWT Issuer in {FilePath}", filePath);
                    }
                }
            }

            // Write back if changes were made
            if (replacements > 0)
            {
                var options = new JsonSerializerOptions { WriteIndented = true };
                var updatedContent = node.ToJsonString(options);
                await File.WriteAllTextAsync(filePath, updatedContent, new UTF8Encoding(false), cancellationToken);

                return new FileUpdateResult
                {
                    FilePath = filePath,
                    FileType = "appsettings",
                    Success = true,
                    Message = $"Updated {replacements} settings",
                    ReplacementsCount = replacements
                };
            }

            return new FileUpdateResult
            {
                FilePath = filePath,
                FileType = "appsettings",
                Success = true,
                Message = "No changes needed",
                ReplacementsCount = 0
            };
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to update appsettings file: {FilePath}", filePath);
            return new FileUpdateResult
            {
                FilePath = filePath,
                FileType = "appsettings",
                Success = false,
                Message = $"Error: {ex.Message}",
                Exception = ex
            };
        }
    }

    /// <summary>
    /// Updates package.json files with new project name
    /// </summary>
    private async Task<List<FileUpdateResult>> UpdatePackageJsonFilesAsync(
        string targetPath,
        PackageJsonConfig config,
        CancellationToken cancellationToken)
    {
        var results = new List<FileUpdateResult>();

        try
        {
            var packageJsonFiles = Directory.GetFiles(
                targetPath, "package.json", SearchOption.AllDirectories);

            _logger.LogDebug("Found {FileCount} package.json files to update", packageJsonFiles.Length);

            foreach (var filePath in packageJsonFiles)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var result = await UpdatePackageJsonFileAsync(filePath, config, cancellationToken);
                results.Add(result);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating package.json files");
            results.Add(new FileUpdateResult
            {
                FilePath = Path.Combine(targetPath, "package.json"),
                FileType = "package.json",
                Success = false,
                Message = $"Error: {ex.Message}",
                Exception = ex
            });
        }

        return results;
    }

    /// <summary>
    /// Updates a single package.json file
    /// </summary>
    private async Task<FileUpdateResult> UpdatePackageJsonFileAsync(
        string filePath,
        PackageJsonConfig config,
        CancellationToken cancellationToken)
    {
        try
        {
            var content = await File.ReadAllTextAsync(filePath, Encoding.UTF8, cancellationToken);
            var node = JsonNode.Parse(content);

            if (node == null)
            {
                return new FileUpdateResult
                {
                    FilePath = filePath,
                    FileType = "package.json",
                    Success = false,
                    Message = "Invalid JSON structure"
                };
            }

            int replacements = 0;

            // Update name
            var name = node["name"];
            if (name != null && name.GetValue<string>() == config.OldName)
            {
                node["name"] = config.NewName;
                replacements++;
                _logger.LogDebug("Updated package name in {FilePath}", filePath);
            }

            // Update description if configured
            if (!string.IsNullOrEmpty(config.NewDescription))
            {
                var description = node["description"];
                if (description != null && description.GetValue<string>()?.Contains(config.OldName) == true)
                {
                    node["description"] = config.NewDescription;
                    replacements++;
                    _logger.LogDebug("Updated package description in {FilePath}", filePath);
                }
            }

            // Write back if changes were made
            if (replacements > 0)
            {
                var options = new JsonSerializerOptions { WriteIndented = true };
                var updatedContent = node.ToJsonString(options);
                await File.WriteAllTextAsync(filePath, updatedContent, new UTF8Encoding(false), cancellationToken);

                return new FileUpdateResult
                {
                    FilePath = filePath,
                    FileType = "package.json",
                    Success = true,
                    Message = $"Updated {replacements} properties",
                    ReplacementsCount = replacements
                };
            }

            return new FileUpdateResult
            {
                FilePath = filePath,
                FileType = "package.json",
                Success = true,
                Message = "No changes needed",
                ReplacementsCount = 0
            };
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to update package.json: {FilePath}", filePath);
            return new FileUpdateResult
            {
                FilePath = filePath,
                FileType = "package.json",
                Success = false,
                Message = $"Error: {ex.Message}",
                Exception = ex
            };
        }
    }

    /// <summary>
    /// Updates README.md files with project name and creation date
    /// </summary>
    private async Task<List<FileUpdateResult>> UpdateReadmeFilesAsync(
        string targetPath,
        string projectName,
        CancellationToken cancellationToken)
    {
        var results = new List<FileUpdateResult>();

        try
        {
            var readmeFiles = Directory.GetFiles(
                targetPath, "README.md", SearchOption.AllDirectories);

            _logger.LogDebug("Found {FileCount} README files to update", readmeFiles.Length);

            foreach (var filePath in readmeFiles)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var result = await UpdateReadmeFileAsync(filePath, projectName, cancellationToken);
                results.Add(result);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating README files");
        }

        return results;
    }

    /// <summary>
    /// Updates a single README.md file with project metadata
    /// </summary>
    private async Task<FileUpdateResult> UpdateReadmeFileAsync(
        string filePath,
        string projectName,
        CancellationToken cancellationToken)
    {
        try
        {
            var content = await File.ReadAllTextAsync(filePath, Encoding.UTF8, cancellationToken);

            // Add project header if not present
            var header = $"# {projectName}\n\n**Generated:** {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC\n\n";

            if (!content.StartsWith("#"))
            {
                content = header + content;
            }

            await File.WriteAllTextAsync(filePath, content, new UTF8Encoding(false), cancellationToken);

            return new FileUpdateResult
            {
                FilePath = filePath,
                FileType = "markdown",
                Success = true,
                Message = "Added project header with timestamp",
                ReplacementsCount = 1
            };
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to update README: {FilePath}", filePath);
            return new FileUpdateResult
            {
                FilePath = filePath,
                FileType = "markdown",
                Success = false,
                Message = $"Error: {ex.Message}",
                Exception = ex
            };
        }
    }

    /// <summary>
    /// Updates .github/copilot-instructions.md with project name
    /// </summary>
    private async Task<List<FileUpdateResult>> UpdateCopilotInstructionsAsync(
        string targetPath,
        string oldName,
        string newName,
        CancellationToken cancellationToken)
    {
        var results = new List<FileUpdateResult>();

        try
        {
            var copilotFile = Path.Combine(targetPath, ".github", "copilot-instructions.md");

            if (!File.Exists(copilotFile))
            {
                _logger.LogDebug("copilot-instructions.md not found at {Path}", copilotFile);
                return results;
            }

            var content = await File.ReadAllTextAsync(copilotFile, Encoding.UTF8, cancellationToken);
            var updatedContent = content.Replace(oldName, newName, StringComparison.OrdinalIgnoreCase);

            if (content != updatedContent)
            {
                await File.WriteAllTextAsync(copilotFile, updatedContent, new UTF8Encoding(false), cancellationToken);

                results.Add(new FileUpdateResult
                {
                    FilePath = copilotFile,
                    FileType = "markdown",
                    Success = true,
                    Message = "Updated project references",
                    ReplacementsCount = 1
                });

                _logger.LogDebug("Updated copilot-instructions.md");
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to update copilot-instructions.md");
            results.Add(new FileUpdateResult
            {
                FilePath = Path.Combine(targetPath, ".github", "copilot-instructions.md"),
                FileType = "markdown",
                Success = false,
                Message = $"Error: {ex.Message}",
                Exception = ex
            });
        }

        return results;
    }
}

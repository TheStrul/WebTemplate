using WebTemplate.Setup.Models;
using WebTemplate.TemplateEngine.Interfaces;

namespace WebTemplate.Setup.Services;

/// <summary>
/// Orchestrates project generation using the C# Template Engine
/// instead of PowerShell scripts
/// </summary>
public class ProjectGenerationService
{
    private readonly ITemplateEngine _templateEngine;

    public ProjectGenerationService(ITemplateEngine templateEngine)
    {
        _templateEngine = templateEngine;
    }

    /// <summary>
    /// Generates a new project workspace from configuration
    /// </summary>
    public async Task<(bool Success, string Message, string? GeneratedPath)> GenerateProjectAsync(
        WebTemplate.Setup.Models.WorkspaceConfiguration config,
        IProgress<string>? progress = null)
    {
        try
        {
            // Validate configuration
            progress?.Report("Validating configuration...");
            var validation = config.Validate();
            if (!validation.IsValid)
            {
                return (false, $"Configuration validation failed:\n{string.Join("\n", validation.Errors)}", null);
            }

            // Ensure target directory doesn't exist
            var targetPath = Path.GetFullPath(config.Project.TargetPath);
            if (Directory.Exists(targetPath))
            {
                return (false, $"Target directory already exists: {targetPath}", null);
            }

            // Convert WorkspaceConfiguration to TemplateEngine's WorkspaceConfiguration
            var templateConfig = ConvertConfiguration(config);

            // Run the template engine (C# implementation, no PowerShell!)
            progress?.Report("Generating project from template...");
            var result = await _templateEngine.GenerateAsync(templateConfig, progress);

            if (!result.Success)
            {
                return (false, result.Message, null);
            }

            // Initialize database (if configured)
            if (config.Database.ExecuteInitScript)
            {
                progress?.Report("Checking if database exists...");
                var dbService = new DatabaseService();
                
                // Check if database already exists
                var databaseExists = await dbService.DatabaseExistsAsync(config.Database.ConnectionString);

                if (!databaseExists)
                {
                    // Database doesn't exist - check if we should create it
                    if (!config.Database.CreateDatabaseIfNotExists)
                    {
                        return (false, 
                            $"Database does not exist and CreateDatabaseIfNotExists is disabled. " +
                            $"Connection string: {config.Database.ConnectionString}", 
                            targetPath);
                    }

                    progress?.Report("Database does not exist - creating database...");
                }
                else
                {
                    progress?.Report("Database exists - proceeding with initialization...");
                }

                // Execute the init script
                progress?.Report("Initializing database...");
                var dbResult = await dbService.ExecuteInitScriptAsync(
                    config.Database.ConnectionString,
                    Path.Combine(targetPath, "Backend", $"{config.Project.ProjectName}.Data", "Migrations", "db-init.sql"));

                if (!dbResult.Success)
                {
                    return (false, $"Database initialization failed: {dbResult.Message}", targetPath);
                }
            }

            progress?.Report($"Project generated successfully in {result.Duration.TotalSeconds:F1}s");
            return (true, "Project generation completed successfully", result.GeneratedPath);
        }
        catch (Exception ex)
        {
            return (false, $"Unexpected error during project generation: {ex.Message}", null);
        }
    }

    /// <summary>
    /// Converts Setup.Models.WorkspaceConfiguration to TemplateEngine.Models.WorkspaceConfiguration
    /// </summary>
    private static WebTemplate.TemplateEngine.Models.WorkspaceConfiguration ConvertConfiguration(
        WebTemplate.Setup.Models.WorkspaceConfiguration setupConfig)
    {
        return new WebTemplate.TemplateEngine.Models.WorkspaceConfiguration
        {
            Project = new WebTemplate.TemplateEngine.Models.ProjectConfiguration
            {
                ProjectName = setupConfig.Project.ProjectName,
                TargetPath = setupConfig.Project.TargetPath,
                InitializeGit = setupConfig.Project.InitializeGit,
                CreateInitialCommit = setupConfig.Project.CreateInitialCommit,
                RunValidation = setupConfig.Project.RunValidation
            },
            Database = new WebTemplate.TemplateEngine.Models.DatabaseConfiguration
            {
                ConnectionString = setupConfig.Database.ConnectionString,
                ExecuteInitScript = setupConfig.Database.ExecuteInitScript,
                InitScriptPath = setupConfig.Database.InitScriptPath
            }
        };
    }
}

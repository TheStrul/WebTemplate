using WebTemplate.Setup.Models;
using WebTemplate.TemplateEngine.Interfaces;
using Microsoft.Data.SqlClient;

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
            // Validate inputs
            if (config == null)
            {
                return (false, "Configuration cannot be null", null);
            }

            if (progress == null)
            {
                progress = new Progress<string>(msg => { /* no-op */ });
            }

            // Validate configuration
            progress.Report("Validating configuration...");
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
            progress.Report("Generating project from template...");
            var result = await _templateEngine.GenerateAsync(templateConfig, progress);

            if (!result.Success)
            {
                return (false, result.Message, null);
            }

            // Initialize database (if configured)
            if (config.Database.ExecuteInitScript)
            {
                progress.Report("Checking if database exists...");
                var dbService = new DatabaseService();

                // Validate required database configuration
                if (string.IsNullOrWhiteSpace(config.Database.ConnectionString))
                {
                    return (false, "Database connection string is required for initialization", targetPath);
                }

                if (string.IsNullOrWhiteSpace(config.Project?.ProjectName))
                {
                    return (false, "Project name is required for database initialization", targetPath);
                }

                // Check if base database exists
                var databaseExists = await dbService.DatabaseExistsAsync(config.Database.ConnectionString);

                // Also check if _Dev database exists
                var builder = new SqlConnectionStringBuilder(config.Database.ConnectionString);
                var devDbName = $"{builder.InitialCatalog}_Dev";
                var devConnectionString = new SqlConnectionStringBuilder(config.Database.ConnectionString)
                {
                    InitialCatalog = devDbName
                }.ConnectionString;
                var devDatabaseExists = await dbService.DatabaseExistsAsync(devConnectionString);

                if (!databaseExists || !devDatabaseExists)
                {
                    // At least one database doesn't exist - check if we should create them
                    if (!config.Database.CreateDatabaseIfNotExists)
                    {
                        var missingDbs = new List<string>();
                        if (!databaseExists) missingDbs.Add(builder.InitialCatalog);
                        if (!devDatabaseExists) missingDbs.Add(devDbName);

                        return (false,
                            $"Database(s) '{string.Join("', '", missingDbs)}' do not exist and CreateDatabaseIfNotExists is disabled.\n\n" +
                            $"Options:\n" +
                            $"1. Enable 'CreateDatabaseIfNotExists' in database configuration\n" +
                            $"2. Create the database(s) manually first\n\n" +
                            $"Connection string: {config.Database.ConnectionString}",
                            targetPath);
                    }

                    // Create the databases
                    progress.Report("Creating databases...");
                    var createDbResult = await dbService.CreateDatabaseIfNotExistsAsync(config.Database.ConnectionString);

                    if (!createDbResult.Success)
                    {
                        return (false, $"Database creation failed:\n\n{createDbResult.Message}", targetPath);
                    }

                    progress.Report(createDbResult.Message);
                }
                else
                {
                    progress.Report("Databases exist - proceeding with initialization...");
                }

                // Build the script path safely
                var scriptPath = Path.Combine(
                    targetPath,
                    "Backend",
                    $"{config.Project.ProjectName}.Data",
                    "Migrations",
                    "db-init.sql");

                // Execute the init script
                progress.Report("Initializing database...");
                var dbResult = await dbService.ExecuteInitScriptAsync(
                    config.Database.ConnectionString,
                    scriptPath);

                if (!dbResult.Success)
                {
                    return (false, $"Database initialization failed:\n\n{dbResult.Message}", targetPath);
                }
            }

            progress.Report($"Project generated successfully in {result.Duration.TotalSeconds:F1}s");
            return (true, "Project generation completed successfully", result.GeneratedPath);
        }
        catch (ArgumentNullException ex)
        {
            return (false, $"Null argument error: {ex.Message}", null);
        }
        catch (ArgumentException ex)
        {
            return (false, $"Invalid argument error: {ex.Message}", null);
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

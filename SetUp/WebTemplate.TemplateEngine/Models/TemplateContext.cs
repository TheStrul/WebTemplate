namespace WebTemplate.TemplateEngine.Models;

/// <summary>
/// Contains all context needed during template generation process
/// </summary>
public class TemplateContext
{
    /// <summary>
    /// Original workspace configuration from the UI
    /// </summary>
    public required WorkspaceConfiguration Configuration { get; init; }

    /// <summary>
    /// Full path to the template root directory (where WebTemplate.sln is located)
    /// </summary>
    public required string TemplatePath { get; init; }

    /// <summary>
    /// Full path to the target directory where the new project will be created
    /// </summary>
    public required string TargetPath { get; init; }

    /// <summary>
    /// Old project name to be replaced (typically "WebTemplate")
    /// </summary>
    public required string OldProjectName { get; init; }

    /// <summary>
    /// New project name from configuration
    /// </summary>
    public required string NewProjectName { get; init; }

    /// <summary>
    /// Database name (typically {ProjectName}Db)
    /// </summary>
    public required string DatabaseName { get; init; }

    /// <summary>
    /// Database connection string
    /// </summary>
    public required string ConnectionString { get; init; }

    /// <summary>
    /// Top-level folders to include in the copy (e.g., Backend, Frontend)
    /// </summary>
    public List<string> IncludeFolders { get; init; } = ["Backend", "Frontend"];

    /// <summary>
    /// Directory names to exclude during copy (e.g., bin, obj, node_modules)
    /// </summary>
    public List<string> ExcludeDirectories { get; init; } =
    [
        ".git", ".vs", "bin", "obj", "node_modules",
        "build", "dist", ".vscode", "TestResults"
    ];

    /// <summary>
    /// File patterns to exclude during copy (e.g., *.user, *.log)
    /// </summary>
    public List<string> ExcludeFiles { get; init; } =
    [
        "*.user", "*.suo", "*.log", "package-lock.json"
    ];

    /// <summary>
    /// Shared state dictionary for passing data between steps
    /// </summary>
    public Dictionary<string, object> State { get; } = new();
}

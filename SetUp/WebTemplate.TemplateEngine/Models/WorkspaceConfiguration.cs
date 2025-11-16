namespace WebTemplate.TemplateEngine.Models;

/// <summary>
/// Configuration for generating a workspace from the template
/// </summary>
public class WorkspaceConfiguration
{
    public ProjectConfiguration Project { get; set; } = new();
    public DatabaseConfiguration Database { get; set; } = new();
}

public class ProjectConfiguration
{
    public string ProjectName { get; set; } = string.Empty;
    public string TargetPath { get; set; } = string.Empty;
    public bool InitializeGit { get; set; } = true;
    public bool CreateInitialCommit { get; set; } = true;
    public bool RunValidation { get; set; } = true;
}

public class DatabaseConfiguration
{
    public string ConnectionString { get; set; } = string.Empty;
    public bool ExecuteInitScript { get; set; }
    public string InitScriptPath { get; set; } = string.Empty;
}

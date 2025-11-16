namespace WebTemplate.Setup.Models;

/// <summary>
/// Project generation settings - where and what to create
/// </summary>
public class ProjectSettings
{
    /// <summary>
    /// New project name (e.g., "MyECommerceApp")
    /// </summary>
    public string ProjectName { get; set; } = string.Empty;

    /// <summary>
    /// Target directory where project will be created
    /// </summary>
    public string TargetPath { get; set; } = string.Empty;

    /// <summary>
    /// Company/organization name (for namespaces, etc.)
    /// </summary>
    public string CompanyName { get; set; } = string.Empty;

    /// <summary>
    /// Author name
    /// </summary>
    public string AuthorName { get; set; } = string.Empty;

    /// <summary>
    /// Initialize Git repository after generation
    /// </summary>
    public bool InitializeGit { get; set; } = true;

    /// <summary>
    /// Create initial Git commit
    /// </summary>
    public bool CreateInitialCommit { get; set; } = true;

    /// <summary>
    /// Run validation after project generation
    /// </summary>
    public bool RunValidation { get; set; } = true;

    public ValidationResult Validate()
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(ProjectName))
            errors.Add("Project name is required");
        else if (!IsValidProjectName(ProjectName))
            errors.Add("Project name must be a valid C# identifier (alphanumeric, no spaces)");

        if (string.IsNullOrWhiteSpace(TargetPath))
            errors.Add("Target path is required");

        return new ValidationResult { IsValid = errors.Count == 0, Errors = errors };
    }

    private static bool IsValidProjectName(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) return false;
        if (char.IsDigit(name[0])) return false; // Can't start with digit
        return name.All(c => char.IsLetterOrDigit(c) || c == '_');
    }
}

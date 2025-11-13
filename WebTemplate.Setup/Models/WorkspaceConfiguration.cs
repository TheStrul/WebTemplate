namespace WebTemplate.Setup.Models;

/// <summary>
/// Master configuration for a workspace that can be saved, loaded, and used to generate projects.
/// This is the root configuration object that contains all sub-configurations.
/// </summary>
public class WorkspaceConfiguration
{
    /// <summary>
    /// Unique identifier for this configuration (used as folder name)
    /// </summary>
    public string ConfigurationId { get; set; } = string.Empty;

    /// <summary>
    /// Display name for this configuration
    /// </summary>
    public string ConfigurationName { get; set; } = string.Empty;

    /// <summary>
    /// Description of what this configuration is for
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// When this configuration was created
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// When this configuration was last modified
    /// </summary>
    public DateTime ModifiedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Project generation settings
    /// </summary>
    public ProjectSettings Project { get; set; } = new();

    /// <summary>
    /// Features configuration (which features to enable)
    /// </summary>
    public FeaturesConfiguration Features { get; set; } = new();

    /// <summary>
    /// Secrets management strategy
    /// </summary>
    public SecretsConfiguration Secrets { get; set; } = new();

    /// <summary>
    /// Database configuration
    /// </summary>
    public DatabaseConfiguration Database { get; set; } = new();

    /// <summary>
    /// Server/hosting configuration
    /// </summary>
    public ServerConfiguration Server { get; set; } = new();

    /// <summary>
    /// Email configuration (SMTP settings)
    /// </summary>
    public EmailConfiguration Email { get; set; } = new();

    /// <summary>
    /// Authentication/JWT configuration
    /// </summary>
    public AuthConfiguration Auth { get; set; } = new();

    /// <summary>
    /// Validates the entire configuration
    /// </summary>
    public ValidationResult Validate()
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(ConfigurationId))
            errors.Add("Configuration ID is required");

        if (string.IsNullOrWhiteSpace(ConfigurationName))
            errors.Add("Configuration name is required");

        // Validate sub-configurations
        errors.AddRange(Project.Validate().Errors);
        errors.AddRange(Features.Validate().Errors);
        errors.AddRange(Secrets.Validate().Errors);
        errors.AddRange(Database.Validate().Errors);
        errors.AddRange(Server.Validate().Errors);
        errors.AddRange(Email.Validate().Errors);
        errors.AddRange(Auth.Validate().Errors);

        return new ValidationResult { IsValid = errors.Count == 0, Errors = errors };
    }
}

/// <summary>
/// Validation result for configurations
/// </summary>
public class ValidationResult
{
    public bool IsValid { get; set; }
    public List<string> Errors { get; set; } = new();
}

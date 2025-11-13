namespace WebTemplate.Core.Configuration;

/// <summary>
/// Core configuration interface providing access to all configuration objects
/// needed by the WebTemplate.Core project.
/// This is the base level of the hierarchical configuration system.
/// </summary>
public interface ICoreConfiguration : IBaseConfiguration
{
    /// <summary>
    /// Authentication and authorization settings including password requirements
    /// </summary>
    AuthSettings Auth { get; }

    /// <summary>
    /// Email service configuration (SMTP, providers, etc.)
    /// </summary>
    EmailSettings Email { get; }

    /// <summary>
    /// User module feature flags
    /// </summary>
    UserModuleFeatures UserModuleFeatures { get; }

    /// <summary>
    /// Configurable response messages
    /// </summary>
    ResponseMessages ResponseMessages { get; }
}

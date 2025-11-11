namespace WebTemplate.Core.Configuration;

/// <summary>
/// Core configuration interface providing access to all configuration objects
/// needed by the WebTemplate.Core project.
/// This is the base level of the hierarchical configuration system.
/// </summary>
public interface ICoreConfiguration
{
    /// <summary>
    /// JWT authentication and token configuration
    /// </summary>
    JwtSettings Jwt { get; }

    /// <summary>
    /// Authentication and authorization settings including password requirements
    /// </summary>
    AuthSettings Auth { get; }

    /// <summary>
    /// Email service configuration (SMTP, providers, etc.)
    /// </summary>
    EmailSettings Email { get; }

    /// <summary>
    /// Application URLs for frontend integration
    /// </summary>
    AppUrls AppUrls { get; }

    /// <summary>
    /// Admin user seeding configuration
    /// </summary>
    AdminSeedSettings AdminSeed { get; }

    /// <summary>
    /// User module feature flags
    /// </summary>
    UserModuleFeatures UserModuleFeatures { get; }

    /// <summary>
    /// Standard response messages for API responses
    /// </summary>
    ResponseMessages ResponseMessages { get; }
}

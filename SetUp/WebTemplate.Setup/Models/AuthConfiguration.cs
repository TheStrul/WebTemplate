namespace WebTemplate.Setup.Models;

/// <summary>
/// Authentication/JWT configuration settings
/// </summary>
public class AuthConfiguration
{
    /// <summary>
    /// JWT secret key (should be stored in secrets)
    /// </summary>
    public string SecretKey { get; set; } = string.Empty;

    /// <summary>
    /// JWT issuer
    /// </summary>
    public string Issuer { get; set; } = "WebTemplateAPI";

    /// <summary>
    /// JWT audience
    /// </summary>
    public string Audience { get; set; } = "WebTemplateClient";

    /// <summary>
    /// Token expiration in minutes
    /// </summary>
    public int ExpirationMinutes { get; set; } = 60;

    /// <summary>
    /// Refresh token expiration in days
    /// </summary>
    public int RefreshTokenExpirationDays { get; set; } = 7;

    /// <summary>
    /// Frontend URL for email confirmation links
    /// </summary>
    public string FrontendUrl { get; set; } = "http://localhost:3000";

    public ValidationResult Validate()
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(SecretKey))
            errors.Add("JWT secret key is required");
        else if (SecretKey.Length < 32)
            errors.Add("JWT secret key must be at least 32 characters long");

        if (string.IsNullOrWhiteSpace(Issuer))
            errors.Add("JWT issuer is required");

        if (string.IsNullOrWhiteSpace(Audience))
            errors.Add("JWT audience is required");

        if (ExpirationMinutes <= 0)
            errors.Add("Token expiration must be greater than 0");

        if (RefreshTokenExpirationDays <= 0)
            errors.Add("Refresh token expiration must be greater than 0");

        if (string.IsNullOrWhiteSpace(FrontendUrl))
            errors.Add("Frontend URL is required");
        else if (!Uri.TryCreate(FrontendUrl, UriKind.Absolute, out _))
            errors.Add("Frontend URL must be a valid absolute URL");

        return new ValidationResult { IsValid = errors.Count == 0, Errors = errors };
    }
}

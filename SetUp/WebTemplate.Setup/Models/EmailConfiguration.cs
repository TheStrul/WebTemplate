namespace WebTemplate.Setup.Models;

/// <summary>
/// Email/SMTP configuration settings
/// </summary>
public class EmailConfiguration
{
    /// <summary>
    /// SMTP server host
    /// </summary>
    public string SmtpHost { get; set; } = string.Empty;

    /// <summary>
    /// SMTP server port
    /// </summary>
    public int SmtpPort { get; set; } = 587;

    /// <summary>
    /// SMTP username
    /// </summary>
    public string SmtpUsername { get; set; } = string.Empty;

    /// <summary>
    /// SMTP password (should be stored in secrets)
    /// </summary>
    public string SmtpPassword { get; set; } = string.Empty;

    /// <summary>
    /// Enable SSL for SMTP
    /// </summary>
    public bool EnableSsl { get; set; } = true;

    /// <summary>
    /// From email address
    /// </summary>
    public string FromEmail { get; set; } = string.Empty;

    /// <summary>
    /// From display name
    /// </summary>
    public string FromName { get; set; } = string.Empty;

    /// <summary>
    /// Whether email is enabled/required for this project
    /// </summary>
    public bool Enabled { get; set; } = false;

    public ValidationResult Validate()
    {
        var errors = new List<string>();

        if (!Enabled)
            return new ValidationResult { IsValid = true, Errors = errors };

        if (string.IsNullOrWhiteSpace(SmtpHost))
            errors.Add("SMTP host is required when email is enabled");

        if (SmtpPort <= 0 || SmtpPort > 65535)
            errors.Add("SMTP port must be between 1 and 65535");

        if (string.IsNullOrWhiteSpace(FromEmail))
            errors.Add("From email address is required when email is enabled");

        return new ValidationResult { IsValid = errors.Count == 0, Errors = errors };
    }
}

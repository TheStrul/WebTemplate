namespace WebTemplate.Setup.Models;

/// <summary>
/// Server/hosting configuration settings
/// </summary>
public class ServerConfiguration
{
    /// <summary>
    /// Server URL (e.g., http://localhost:5000)
    /// </summary>
    public string Url { get; set; } = "http://localhost:5000";

    /// <summary>
    /// HTTPS URL (e.g., https://localhost:5001)
    /// </summary>
    public string? HttpsUrl { get; set; }

    /// <summary>
    /// Connection timeout in seconds
    /// </summary>
    public int ConnectionTimeoutSeconds { get; set; } = 30;

    /// <summary>
    /// Enable HTTPS redirection
    /// </summary>
    public bool UseHttpsRedirection { get; set; } = true;

    public ValidationResult Validate()
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(Url))
            errors.Add("Server URL is required");
        else if (!Uri.TryCreate(Url, UriKind.Absolute, out _))
            errors.Add("Server URL must be a valid absolute URL");

        if (!string.IsNullOrWhiteSpace(HttpsUrl) && !Uri.TryCreate(HttpsUrl, UriKind.Absolute, out _))
            errors.Add("HTTPS URL must be a valid absolute URL");

        if (ConnectionTimeoutSeconds <= 0)
            errors.Add("Connection timeout must be greater than 0");

        return new ValidationResult { IsValid = errors.Count == 0, Errors = errors };
    }
}

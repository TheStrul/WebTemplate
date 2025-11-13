namespace WebTemplate.Core.Configuration;

/// <summary>
/// Server configuration including URL and connection settings.
/// Used to configure where the API server listens.
/// </summary>
public class ServerSettings
{
    public const string SectionName = "Server";

    /// <summary>
    /// The base URL where the server listens (e.g., "http://localhost:5294")
    /// </summary>
    public string Url { get; set; } = string.Empty;

    /// <summary>
    /// Health check endpoint path
    /// </summary>
    public string HealthEndpoint { get; set; } = "/health";

    /// <summary>
    /// Connection timeout in seconds for health checks
    /// </summary>
    public int ConnectionTimeoutSeconds { get; set; } = 2;
}

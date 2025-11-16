namespace WebTemplate.Setup.Models;

/// <summary>
/// Secrets management configuration - determines how secrets are stored and accessed
/// </summary>
public class SecretsConfiguration
{
    /// <summary>
    /// Strategy for managing secrets
    /// </summary>
    public SecretsStrategy Strategy { get; set; } = SecretsStrategy.UserSecrets;

    /// <summary>
    /// Azure Key Vault configuration (when Strategy = AzureKeyVault)
    /// </summary>
    public AzureKeyVaultSettings? AzureKeyVault { get; set; }

    /// <summary>
    /// User secrets to configure (when Strategy = UserSecrets)
    /// Key-value pairs that will be stored in user secrets
    /// </summary>
    public Dictionary<string, string> UserSecretsValues { get; set; } = new();

    /// <summary>
    /// Environment variables configuration (when Strategy = EnvironmentVariables)
    /// </summary>
    public Dictionary<string, string> EnvironmentVariables { get; set; } = new();

    public ValidationResult Validate()
    {
        var errors = new List<string>();

        switch (Strategy)
        {
            case SecretsStrategy.AzureKeyVault:
                if (AzureKeyVault == null)
                    errors.Add("Azure Key Vault settings are required when using Key Vault strategy");
                else
                {
                    if (string.IsNullOrWhiteSpace(AzureKeyVault.VaultUri))
                        errors.Add("Azure Key Vault URI is required");
                    if (string.IsNullOrWhiteSpace(AzureKeyVault.TenantId))
                        errors.Add("Azure Key Vault Tenant ID is required");
                }
                break;

            case SecretsStrategy.UserSecrets:
                // User secrets are optional - defaults will be used
                break;

            case SecretsStrategy.EnvironmentVariables:
                // Environment variables are optional
                break;

            case SecretsStrategy.Mixed:
                // CLARIFICATION NEEDED: How should mixed strategy validation work?
                // For now, just ensure both UserSecrets and KeyVault settings are provided
                if (AzureKeyVault == null)
                    errors.Add("Azure Key Vault settings are required for mixed strategy");
                break;

            default:
                errors.Add($"Unknown secrets strategy: {Strategy}");
                break;
        }

        return new ValidationResult { IsValid = errors.Count == 0, Errors = errors };
    }
}

/// <summary>
/// Strategy for managing application secrets
/// </summary>
public enum SecretsStrategy
{
    /// <summary>
    /// Use .NET User Secrets (for development)
    /// </summary>
    UserSecrets,

    /// <summary>
    /// Use Azure Key Vault (for production)
    /// </summary>
    AzureKeyVault,

    /// <summary>
    /// Use environment variables
    /// </summary>
    EnvironmentVariables,

    /// <summary>
    /// Use User Secrets for development, Azure Key Vault for production
    /// CLARIFICATION NEEDED: How exactly should this work?
    /// </summary>
    Mixed
}

/// <summary>
/// Azure Key Vault configuration settings
/// </summary>
public class AzureKeyVaultSettings
{
    /// <summary>
    /// Key Vault URI (e.g., https://mykeyvault.vault.azure.net/)
    /// </summary>
    public string VaultUri { get; set; } = string.Empty;

    /// <summary>
    /// Azure AD Tenant ID
    /// </summary>
    public string TenantId { get; set; } = string.Empty;

    /// <summary>
    /// Client ID (Application ID) for authentication
    /// </summary>
    public string ClientId { get; set; } = string.Empty;

    /// <summary>
    /// Whether to use managed identity (if true, ClientId/ClientSecret not needed)
    /// </summary>
    public bool UseManagedIdentity { get; set; } = false;

    /// <summary>
    /// Upload secrets to Key Vault during project generation (requires Azure CLI installed and logged in)
    /// </summary>
    public bool UploadSecretsNow { get; set; } = false;

    /// <summary>
    /// Secret prefix in Key Vault (optional, e.g., "MyApp--")
    /// </summary>
    public string? SecretPrefix { get; set; }
}

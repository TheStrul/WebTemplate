using Azure.Core;
using Azure.Security.KeyVault.Secrets;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using WebTemplate.Core.Interfaces;

namespace WebTemplate.Core.Services;

/// <summary>
/// Services Key Vault service implementation that provides secure access to secrets
/// in a generic, solution-agnostic way (no dependency on solution-specific enums or types).
/// Exceptions from Azure KeyVault SDK are allowed to propagate for proper error handling by callers.
/// </summary>
public sealed class KeyVaultService : IKeyVaultService
{
    private readonly SecretClient _secretClient;
    private readonly ConcurrentDictionary<string, string> _secrets = new();

    /// <summary>
    /// Initializes a new instance of the KeyVaultService
    /// </summary>
    /// <param name="keyVaultEndpoint">The Key Vault endpoint URI</param>
    /// <param name="credential">The token credential for authentication</param>
    /// <param name="logger">Logger instance (unused - kept for backward compatibility)</param>
    public KeyVaultService(string keyVaultEndpoint, TokenCredential credential, ILogger logger)
    {
        _secretClient = new SecretClient(new Uri(keyVaultEndpoint), credential);
        // Logger parameter kept for backward compatibility but not used
        // Exceptions propagate naturally with full Azure SDK error details
    }

    /// <summary>
    /// Asynchronously retrieves the value of a secret from Azure Key Vault.
    /// Caches secrets in memory for subsequent requests.
    /// </summary>
    /// <param name="secretName">The name of the secret to retrieve from KeyVault</param>
    /// <returns>The secret value as a string</returns>
    /// <exception cref="ArgumentException">Thrown when secretName is null or empty</exception>
    /// <exception cref="Azure.RequestFailedException">
    /// Thrown when KeyVault operation fails. Common scenarios:
    /// - Status 404: Secret not found
    /// - Status 401/403: Authentication or authorization failure
    /// - Network errors: Connection timeout, DNS resolution failure
    /// Callers should catch and handle these exceptions appropriately.
    /// </exception>
    public async Task<string> GetSecretAsync(string secretName)
    {
        if (string.IsNullOrWhiteSpace(secretName))
        {
            throw new ArgumentException("Secret name cannot be null or empty", nameof(secretName));
        }

        // Return cached secret if available
        if (_secrets.ContainsKey(secretName))
        {
            return _secrets[secretName];
        }

        // Retrieve from KeyVault - let exceptions propagate with full error details
        var response = await _secretClient.GetSecretAsync(secretName);
        var secretValue = response.Value.Value;

        // Cache the secret for future requests
        _secrets[secretName] = secretValue;

        return secretValue;
    }

    /// <inheritdoc/>
    public async Task<T> GetSecretAsync<T>(string secretName, Func<string, T> parser)
    {
        string s = await GetSecretAsync(secretName);
        return parser(s);
    }
}

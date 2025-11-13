namespace WebTemplate.Core.Interfaces;

/// <summary>
/// Service for retrieving secrets from Azure Key Vault.
/// </summary>
public interface IKeyVaultService
{
    /// <summary>
    /// Asynchronously retrieves the value of a secret from Azure Key Vault.
    /// </summary>
    /// <param name="secretName">The name (or identifier) of the secret to fetch.</param>
    /// <returns>A task that resolves to the secret value as a string.</returns>
    Task<string> GetSecretAsync(string secretName);

    /// <summary>
    /// Asynchronously retrieves a typed setting by fetching the named secret
    /// and converting it with the supplied parser.
    /// </summary>
    /// <typeparam name="T">
    /// The type to convert the secret's raw string into.
    /// </typeparam>
    /// <param name="secretName">
    /// The name (or identifier) of the secret to fetch.
    /// </param>
    /// <param name="parser">
    /// A function that transforms the raw secret string into T.
    /// </param>
    /// <returns>
    /// A task that resolves to the parsed setting value.
    /// </returns>
    Task<T> GetSecretAsync<T>(string secretName, Func<string, T> parser);
}

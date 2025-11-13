using System.Diagnostics;
using System.Text;
using WebTemplate.Setup.Models;

namespace WebTemplate.Setup.Services;

/// <summary>
/// Service for configuring secrets using different strategies
/// </summary>
public class SecretsService
{
    /// <summary>
    /// Configures secrets for a generated project based on strategy
    /// </summary>
    public async Task<(bool Success, string Message)> ConfigureSecretsAsync(
        string projectPath,
        WorkspaceConfiguration config)
    {
        ISecretsStrategy strategy = config.Secrets.Strategy switch
        {
            SecretsStrategy.UserSecrets => new UserSecretsStrategy(),
            SecretsStrategy.AzureKeyVault => new AzureKeyVaultStrategy(),
            SecretsStrategy.EnvironmentVariables => new EnvironmentVariablesStrategy(),
            SecretsStrategy.Mixed => new MixedSecretsStrategy(),
            _ => throw new InvalidOperationException($"Unknown secrets strategy: {config.Secrets.Strategy}")
        };

        return await strategy.ConfigureAsync(projectPath, config);
    }
}

/// <summary>
/// Interface for secrets configuration strategies
/// </summary>
public interface ISecretsStrategy
{
    Task<(bool Success, string Message)> ConfigureAsync(string projectPath, WorkspaceConfiguration config);
}

/// <summary>
/// User Secrets strategy - uses dotnet user-secrets
/// </summary>
public class UserSecretsStrategy : ISecretsStrategy
{
    public async Task<(bool Success, string Message)> ConfigureAsync(string projectPath, WorkspaceConfiguration config)
    {
        try
        {
            var apiProjectPath = FindApiProject(projectPath, config.Project.ProjectName);
            if (apiProjectPath == null)
            {
                return (false, "Could not find API project");
            }

            // Initialize user secrets
            var initResult = await RunDotnetCommandAsync($"user-secrets init --project \"{apiProjectPath}\"");
            if (initResult.ExitCode != 0)
            {
                return (false, $"Failed to initialize user secrets: {initResult.Error}");
            }

            // Set required secrets
            var secrets = BuildSecretsDictionary(config);
            foreach (var (key, value) in secrets)
            {
                var setResult = await RunDotnetCommandAsync(
                    $"user-secrets set --project \"{apiProjectPath}\" \"{key}\" \"{value}\""
                );
                if (setResult.ExitCode != 0)
                {
                    return (false, $"Failed to set secret '{key}': {setResult.Error}");
                }
            }

            // Add any custom user secrets from configuration
            foreach (var (key, value) in config.Secrets.UserSecretsValues)
            {
                var setResult = await RunDotnetCommandAsync(
                    $"user-secrets set --project \"{apiProjectPath}\" \"{key}\" \"{value}\""
                );
                if (setResult.ExitCode != 0)
                {
                    return (false, $"Failed to set custom secret '{key}': {setResult.Error}");
                }
            }

            return (true, $"User secrets configured successfully. {secrets.Count + config.Secrets.UserSecretsValues.Count} secrets set.");
        }
        catch (Exception ex)
        {
            return (false, $"Error configuring user secrets: {ex.Message}");
        }
    }

    private Dictionary<string, string> BuildSecretsDictionary(WorkspaceConfiguration config)
    {
        var secrets = new Dictionary<string, string>();

        // JWT Secret
        if (!string.IsNullOrWhiteSpace(config.Auth.SecretKey))
        {
            secrets["JwtSettings:SecretKey"] = config.Auth.SecretKey;
        }

        // Admin Seed Password
        if (config.Features.AdminSeed.Enabled && !string.IsNullOrWhiteSpace(config.Features.AdminSeed.Password))
        {
            secrets["AdminSeed:Password"] = config.Features.AdminSeed.Password;
        }

        // Email Password
        if (config.Email.Enabled && !string.IsNullOrWhiteSpace(config.Email.SmtpPassword))
        {
            secrets["EmailSettings:SmtpPassword"] = config.Email.SmtpPassword;
        }

        return secrets;
    }

    private static string? FindApiProject(string projectPath, string projectName)
    {
        var apiProjectPath = Path.Combine(projectPath, "Backend", $"{projectName}.API", $"{projectName}.API.csproj");
        return File.Exists(apiProjectPath) ? apiProjectPath : null;
    }

    private async Task<(int ExitCode, string Output, string Error)> RunDotnetCommandAsync(string arguments)
    {
        var psi = new ProcessStartInfo
        {
            FileName = "dotnet",
            Arguments = arguments,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };

        using var process = new Process { StartInfo = psi };
        var output = new StringBuilder();
        var error = new StringBuilder();

        process.OutputDataReceived += (sender, e) => { if (e.Data != null) output.AppendLine(e.Data); };
        process.ErrorDataReceived += (sender, e) => { if (e.Data != null) error.AppendLine(e.Data); };

        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();
        await process.WaitForExitAsync();

        return (process.ExitCode, output.ToString(), error.ToString());
    }
}

/// <summary>
/// Azure Key Vault strategy - configures application to use Azure Key Vault for secrets
/// </summary>
public class AzureKeyVaultStrategy : ISecretsStrategy
{
    public async Task<(bool Success, string Message)> ConfigureAsync(string projectPath, WorkspaceConfiguration config)
    {
        try
        {
            var kvSettings = config.Secrets.AzureKeyVault;
            if (kvSettings == null || string.IsNullOrWhiteSpace(kvSettings.VaultUri))
            {
                return (false, "Azure Key Vault settings are not configured. VaultUri is required.");
            }

            // Step 1: Generate appsettings.Production.json with Key Vault configuration
            var productionSettingsPath = Path.Combine(projectPath, "Backend", $"{config.Project.ProjectName}.API", "appsettings.Production.json");

            var productionConfig = new
            {
                KeyVault = new
                {
                    VaultUri = kvSettings.VaultUri,
                    TenantId = kvSettings.TenantId,
                    ClientId = kvSettings.ClientId,
                    UseManagedIdentity = kvSettings.UseManagedIdentity
                },
                // Reference secrets from Key Vault using naming convention
                JwtSettings = new
                {
                    Issuer = config.Auth.Issuer,
                    Audience = config.Auth.Audience,
                    ExpirationMinutes = config.Auth.ExpirationMinutes
                    // SecretKey will be loaded from Key Vault at runtime
                },
                AdminSeed = new
                {
                    Email = config.Features.AdminSeed.Email,
                    FirstName = config.Features.AdminSeed.FirstName,
                    LastName = config.Features.AdminSeed.LastName
                    // Password will be loaded from Key Vault at runtime
                },
                EmailSettings = new
                {
                    SmtpHost = config.Email.SmtpHost,
                    SmtpPort = config.Email.SmtpPort,
                    SmtpUsername = config.Email.SmtpUsername,
                    EnableSsl = config.Email.EnableSsl,
                    FromEmail = config.Email.FromEmail,
                    FromName = config.Email.FromName
                    // SmtpPassword will be loaded from Key Vault at runtime
                }
            };

            var json = System.Text.Json.JsonSerializer.Serialize(productionConfig, new System.Text.Json.JsonSerializerOptions
            {
                WriteIndented = true
            });

            await File.WriteAllTextAsync(productionSettingsPath, json);

            // Step 2: Optionally upload secrets to Key Vault using Azure CLI
            var uploadMessage = "";
            if (kvSettings.UploadSecretsNow)
            {
                var uploadResult = await UploadSecretsToKeyVault(config, kvSettings.VaultUri);
                uploadMessage = uploadResult.Success
                    ? $"\n✅ Secrets uploaded to Key Vault: {uploadResult.Message}"
                    : $"\n⚠️ Warning: Failed to upload secrets: {uploadResult.Message}";
            }

            return (true, $"Azure Key Vault configured successfully.\n" +
                         $"VaultUri: {kvSettings.VaultUri}\n" +
                         $"Configuration written to: {productionSettingsPath}{uploadMessage}\n\n" +
                         $"NOTE: Ensure your generated project includes Azure.Extensions.AspNetCore.Configuration.Secrets package.");
        }
        catch (Exception ex)
        {
            return (false, $"Error configuring Azure Key Vault: {ex.Message}");
        }
    }

    private async Task<(bool Success, string Message)> UploadSecretsToKeyVault(WorkspaceConfiguration config, string vaultUri)
    {
        try
        {
            var vaultName = new Uri(vaultUri).Host.Split('.')[0];
            var secrets = new Dictionary<string, string>();

            // Build secrets to upload
            if (!string.IsNullOrWhiteSpace(config.Auth.SecretKey))
                secrets["JwtSecretKey"] = config.Auth.SecretKey;

            if (config.Features.AdminSeed.Enabled && !string.IsNullOrWhiteSpace(config.Features.AdminSeed.Password))
                secrets["AdminPassword"] = config.Features.AdminSeed.Password;

            if (config.Email.Enabled && !string.IsNullOrWhiteSpace(config.Email.SmtpPassword))
                secrets["SmtpPassword"] = config.Email.SmtpPassword;

            var uploadedCount = 0;
            var failedSecrets = new List<string>();

            foreach (var (secretName, secretValue) in secrets)
            {
                var result = await RunAzureCliCommand($"keyvault secret set --vault-name {vaultName} --name {secretName} --value \"{secretValue}\"");
                if (result.ExitCode == 0)
                    uploadedCount++;
                else
                    failedSecrets.Add($"{secretName} ({result.Error})");
            }

            if (failedSecrets.Any())
            {
                return (false, $"Uploaded {uploadedCount}/{secrets.Count} secrets. Failed: {string.Join(", ", failedSecrets)}");
            }

            return (true, $"{uploadedCount} secrets uploaded successfully");
        }
        catch (Exception ex)
        {
            return (false, $"Failed to upload secrets: {ex.Message}");
        }
    }

    private async Task<(int ExitCode, string Output, string Error)> RunAzureCliCommand(string arguments)
    {
        var psi = new ProcessStartInfo
        {
            FileName = "az",
            Arguments = arguments,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };

        using var process = new Process { StartInfo = psi };
        var output = new StringBuilder();
        var error = new StringBuilder();

        process.OutputDataReceived += (sender, e) => { if (e.Data != null) output.AppendLine(e.Data); };
        process.ErrorDataReceived += (sender, e) => { if (e.Data != null) error.AppendLine(e.Data); };

        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();
        await process.WaitForExitAsync();

        return (process.ExitCode, output.ToString(), error.ToString());
    }
}

/// <summary>
/// Environment Variables strategy
/// </summary>
public class EnvironmentVariablesStrategy : ISecretsStrategy
{
    public Task<(bool Success, string Message)> ConfigureAsync(string projectPath, WorkspaceConfiguration config)
    {
        try
        {
            var secrets = new Dictionary<string, string>();

            // Build secrets dictionary (same as UserSecrets)
            if (!string.IsNullOrWhiteSpace(config.Auth.SecretKey))
                secrets["JwtSettings__SecretKey"] = config.Auth.SecretKey;

            if (config.Features.AdminSeed.Enabled && !string.IsNullOrWhiteSpace(config.Features.AdminSeed.Password))
                secrets["AdminSeed__Password"] = config.Features.AdminSeed.Password;

            if (config.Email.Enabled && !string.IsNullOrWhiteSpace(config.Email.SmtpPassword))
                secrets["EmailSettings__SmtpPassword"] = config.Email.SmtpPassword;

            // Add custom environment variables
            foreach (var (key, value) in config.Secrets.EnvironmentVariables)
            {
                secrets[key] = value;
            }

            // Generate a .env file or script for setting environment variables
            var envFilePath = Path.Combine(projectPath, "secrets.env");
            var envContent = new StringBuilder();
            envContent.AppendLine("# Environment Variables for Application Secrets");
            envContent.AppendLine("# Source this file or set these variables in your environment");
            envContent.AppendLine();

            foreach (var (key, value) in secrets)
            {
                envContent.AppendLine($"{key}={value}");
            }

            File.WriteAllText(envFilePath, envContent.ToString());

            return Task.FromResult((
                true,
                $"Environment variables configuration saved to {envFilePath}. " +
                $"{secrets.Count} variables configured. Set these in your environment before running the application."
            ));
        }
        catch (Exception ex)
        {
            return Task.FromResult((false, $"Error configuring environment variables: {ex.Message}"));
        }
    }
}

/// <summary>
/// Mixed strategy - User Secrets for dev, Azure Key Vault for production
/// CLARIFICATION NEEDED: How should this be implemented exactly?
/// </summary>
public class MixedSecretsStrategy : ISecretsStrategy
{
    public async Task<(bool Success, string Message)> ConfigureAsync(string projectPath, WorkspaceConfiguration config)
    {
        // Configure user secrets for development
        var userSecretsStrategy = new UserSecretsStrategy();
        var userSecretsResult = await userSecretsStrategy.ConfigureAsync(projectPath, config);

        if (!userSecretsResult.Success)
        {
            return userSecretsResult;
        }

        // Configure Azure Key Vault for production
        var keyVaultStrategy = new AzureKeyVaultStrategy();
        var keyVaultResult = await keyVaultStrategy.ConfigureAsync(projectPath, config);

        // For now, just configure user secrets since Key Vault isn't implemented
        return (
            true,
            "Mixed strategy: User secrets configured for development. " +
            "Azure Key Vault configuration pending implementation."
        );
    }
}

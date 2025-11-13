using System.Diagnostics;
using System.Text;
using WebTemplate.Setup.Models;

namespace WebTemplate.Setup.Services;

/// <summary>
/// Orchestrates project generation by wrapping the PowerShell template script
/// and applying workspace configuration settings
/// </summary>
public class ProjectGenerationService
{
    private readonly string _templateRoot;
    private readonly string _templateScriptPath;

    public ProjectGenerationService(string? templateRoot = null)
    {
        _templateRoot = templateRoot ?? GetTemplateRoot();
        _templateScriptPath = Path.Combine(_templateRoot, "template-scripts", "New-ProjectFromTemplate.ps1");
    }

    /// <summary>
    /// Generates a new project workspace from configuration
    /// </summary>
    public async Task<(bool Success, string Message, string? GeneratedPath)> GenerateProjectAsync(
        WorkspaceConfiguration config,
        IProgress<string>? progress = null)
    {
        try
        {
            // Validate configuration
            progress?.Report("Validating configuration...");
            var validation = config.Validate();
            if (!validation.IsValid)
            {
                return (false, $"Configuration validation failed:\n{string.Join("\n", validation.Errors)}", null);
            }

            // Ensure target directory doesn't exist
            var targetPath = Path.GetFullPath(config.Project.TargetPath);
            if (Directory.Exists(targetPath))
            {
                return (false, $"Target directory already exists: {targetPath}", null);
            }

            // Step 1: Run PowerShell template script
            progress?.Report("Running project template script...");
            var scriptResult = await RunTemplateScriptAsync(config, progress);
            if (!scriptResult.Success)
            {
                return (false, scriptResult.Message, null);
            }

            // Step 2: Generate configuration files
            progress?.Report("Generating configuration files...");
            var configResult = await GenerateConfigurationFilesAsync(targetPath, config);
            if (!configResult.Success)
            {
                return (false, configResult.Message, targetPath);
            }

            // Step 3: Configure secrets
            progress?.Report("Configuring secrets...");
            var secretsResult = await ConfigureSecretsAsync(targetPath, config);
            if (!secretsResult.Success)
            {
                return (false, secretsResult.Message, targetPath);
            }

            // Step 4: Initialize database (if configured)
            if (config.Database.ExecuteInitScript)
            {
                progress?.Report("Initializing database...");
                var dbService = new DatabaseService();
                var dbResult = await dbService.ExecuteInitScriptAsync(
                    config.Database.ConnectionString,
                    Path.Combine(targetPath, config.Database.InitScriptPath)
                );
                if (!dbResult.Success)
                {
                    return (false, $"Database initialization failed: {dbResult.Message}", targetPath);
                }
            }

            progress?.Report("Project generation completed successfully!");
            return (true, "Project generated successfully", targetPath);
        }
        catch (Exception ex)
        {
            return (false, $"Unexpected error during project generation: {ex.Message}", null);
        }
    }

    private async Task<(bool Success, string Message)> RunTemplateScriptAsync(
        WorkspaceConfiguration config,
        IProgress<string>? progress)
    {
        try
        {
            if (!File.Exists(_templateScriptPath))
            {
                return (false, $"Template script not found: {_templateScriptPath}");
            }

            var arguments = BuildScriptArguments(config);
            var result = await RunPowerShellAsync(_templateScriptPath, arguments, progress);

            if (result.ExitCode != 0)
            {
                return (false, $"Template script failed with exit code {result.ExitCode}:\n{result.Error}");
            }

            return (true, "Template script executed successfully");
        }
        catch (Exception ex)
        {
            return (false, $"Error running template script: {ex.Message}");
        }
    }

    private string BuildScriptArguments(WorkspaceConfiguration config)
    {
        var args = new List<string>
        {
            "-NonInteractive",
            $"-ProjectName '{config.Project.ProjectName}'",
            $"-TargetPath '{config.Project.TargetPath}'"
        };

        if (!config.Project.InitializeGit)
        {
            args.Add("-SkipGit");
        }

        if (!config.Project.RunValidation)
        {
            args.Add("-SkipValidation");
        }

        return string.Join(" ", args);
    }

    private async Task<(bool Success, string Message)> GenerateConfigurationFilesAsync(
        string targetPath,
        WorkspaceConfiguration config)
    {
        try
        {
            // Generate appsettings.Local.json for development
            var localSettings = GenerateLocalSettings(config);
            var localSettingsPath = Path.Combine(targetPath, "Backend", $"{config.Project.ProjectName}.API", "appsettings.Local.json");
            await File.WriteAllTextAsync(localSettingsPath, localSettings);

            // Generate appsettings.Production.json template
            var prodSettings = GenerateProductionSettings(config);
            var prodSettingsPath = Path.Combine(targetPath, "Backend", $"{config.Project.ProjectName}.API", "appsettings.Production.json");
            await File.WriteAllTextAsync(prodSettingsPath, prodSettings);

            return (true, "Configuration files generated successfully");
        }
        catch (Exception ex)
        {
            return (false, $"Error generating configuration files: {ex.Message}");
        }
    }

    private string GenerateLocalSettings(WorkspaceConfiguration config)
    {
        // CLARIFICATION NEEDED: Should this generate full settings or minimal overrides?
        // For now, generating comprehensive settings
        return System.Text.Json.JsonSerializer.Serialize(new
        {
            Server = new
            {
                Url = config.Server.Url,
                ConnectionTimeoutSeconds = config.Server.ConnectionTimeoutSeconds
            },
            Database = new
            {
                ConnectionString = config.Database.ConnectionString
            },
            JwtSettings = new
            {
                Issuer = config.Auth.Issuer,
                Audience = config.Auth.Audience,
                ExpirationMinutes = config.Auth.ExpirationMinutes
            },
            AppUrls = new
            {
                FrontendUrl = config.Auth.FrontendUrl
            },
            EmailSettings = config.Email.Enabled ? new
            {
                SmtpHost = config.Email.SmtpHost,
                SmtpPort = config.Email.SmtpPort,
                SmtpUsername = config.Email.SmtpUsername,
                EnableSsl = config.Email.EnableSsl,
                FromEmail = config.Email.FromEmail,
                FromName = config.Email.FromName
            } : null,
            Features = config.Features
        }, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
    }

    private string GenerateProductionSettings(WorkspaceConfiguration config)
    {
        // Generate production template with placeholders for sensitive data
        return System.Text.Json.JsonSerializer.Serialize(new
        {
            Server = new
            {
                Url = "https://yourdomain.com",
                ConnectionTimeoutSeconds = config.Server.ConnectionTimeoutSeconds
            },
            Database = new
            {
                ConnectionString = "REPLACE_WITH_PRODUCTION_CONNECTION_STRING"
            },
            JwtSettings = new
            {
                Issuer = config.Auth.Issuer,
                Audience = config.Auth.Audience,
                ExpirationMinutes = config.Auth.ExpirationMinutes
            },
            AppUrls = new
            {
                FrontendUrl = "https://yourdomain.com"
            },
            Features = config.Features
        }, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
    }

    private async Task<(bool Success, string Message)> ConfigureSecretsAsync(
        string targetPath,
        WorkspaceConfiguration config)
    {
        try
        {
            var secretsService = new SecretsService();
            var result = await secretsService.ConfigureSecretsAsync(targetPath, config);
            return result;
        }
        catch (Exception ex)
        {
            return (false, $"Error configuring secrets: {ex.Message}");
        }
    }

    private async Task<(int ExitCode, string Output, string Error)> RunPowerShellAsync(
        string scriptPath,
        string arguments,
        IProgress<string>? progress)
    {
        var psi = new ProcessStartInfo
        {
            FileName = "pwsh",
            Arguments = $"-NoProfile -ExecutionPolicy Bypass -File \"{scriptPath}\" {arguments}",
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true,
            WorkingDirectory = Path.GetDirectoryName(scriptPath)
        };

        using var process = new Process { StartInfo = psi };
        var output = new StringBuilder();
        var error = new StringBuilder();

        process.OutputDataReceived += (sender, e) =>
        {
            if (e.Data != null)
            {
                output.AppendLine(e.Data);
                progress?.Report(e.Data);
            }
        };

        process.ErrorDataReceived += (sender, e) =>
        {
            if (e.Data != null)
            {
                error.AppendLine(e.Data);
                progress?.Report($"ERROR: {e.Data}");
            }
        };

        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();
        await process.WaitForExitAsync();

        return (process.ExitCode, output.ToString(), error.ToString());
    }

    private static string GetTemplateRoot()
    {
        var dir = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);
        while (dir != null)
        {
            if (File.Exists(Path.Combine(dir.FullName, "WebTemplate.sln")))
            {
                return dir.FullName;
            }
            dir = dir.Parent;
        }
        return AppDomain.CurrentDomain.BaseDirectory;
    }
}

using System.Text.Json;
using System.Text.Json.Serialization;
using WebTemplate.Setup.Models;

namespace WebTemplate.Setup.Services;

/// <summary>
/// Service for saving, loading, and managing workspace configurations.
/// Configurations are stored in the Configurations/ folder, each in its own subdirectory.
/// </summary>
public class ConfigurationPersistenceService
{
    private readonly string _configurationsRoot;
    private readonly JsonSerializerOptions _jsonOptions;

    public ConfigurationPersistenceService(string? configurationsRoot = null)
    {
        // Default to Configurations/ folder in the template root
        // CLARIFICATION NEEDED: Should this be relative to template root or user's AppData?
        // For now, using template root so configurations travel with the template
        _configurationsRoot = configurationsRoot ?? Path.Combine(
            GetTemplateRoot(),
            "Configurations"
        );

        _jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            Converters = { new JsonStringEnumConverter() }
        };

        EnsureConfigurationsDirectoryExists();
    }

    /// <summary>
    /// Saves a workspace configuration to disk
    /// </summary>
    public async Task<(bool Success, string Message)> SaveConfigurationAsync(WorkspaceConfiguration config)
    {
        try
        {
            // Validate before saving
            var validation = config.Validate();
            if (!validation.IsValid)
            {
                return (false, $"Configuration validation failed:\n{string.Join("\n", validation.Errors)}");
            }

            // Update modified timestamp
            config.ModifiedAt = DateTime.UtcNow;

            // Create configuration directory
            var configDir = GetConfigurationDirectory(config.ConfigurationId);
            Directory.CreateDirectory(configDir);

            // Save main configuration file
            var configPath = Path.Combine(configDir, "workspace-config.json");
            var json = JsonSerializer.Serialize(config, _jsonOptions);
            await File.WriteAllTextAsync(configPath, json);

            return (true, $"Configuration saved successfully to {configDir}");
        }
        catch (Exception ex)
        {
            return (false, $"Error saving configuration: {ex.Message}");
        }
    }

    /// <summary>
    /// Loads a workspace configuration from disk
    /// </summary>
    public async Task<(bool Success, WorkspaceConfiguration? Config, string Message)> LoadConfigurationAsync(string configurationId)
    {
        try
        {
            var configPath = Path.Combine(GetConfigurationDirectory(configurationId), "workspace-config.json");

            if (!File.Exists(configPath))
            {
                return (false, null, $"Configuration '{configurationId}' not found");
            }

            var json = await File.ReadAllTextAsync(configPath);
            var config = JsonSerializer.Deserialize<WorkspaceConfiguration>(json, _jsonOptions);

            if (config == null)
            {
                return (false, null, "Failed to deserialize configuration");
            }

            return (true, config, "Configuration loaded successfully");
        }
        catch (Exception ex)
        {
            return (false, null, $"Error loading configuration: {ex.Message}");
        }
    }

    /// <summary>
    /// Lists all saved configurations
    /// </summary>
    public List<ConfigurationSummary> ListConfigurations()
    {
        var summaries = new List<ConfigurationSummary>();

        try
        {
            if (!Directory.Exists(_configurationsRoot))
            {
                return summaries;
            }

            var configDirs = Directory.GetDirectories(_configurationsRoot);

            foreach (var configDir in configDirs)
            {
                var configPath = Path.Combine(configDir, "workspace-config.json");
                if (File.Exists(configPath))
                {
                    try
                    {
                        var json = File.ReadAllText(configPath);
                        var config = JsonSerializer.Deserialize<WorkspaceConfiguration>(json, _jsonOptions);

                        if (config != null)
                        {
                            summaries.Add(new ConfigurationSummary
                            {
                                ConfigurationId = config.ConfigurationId,
                                ConfigurationName = config.ConfigurationName,
                                Description = config.Description,
                                ProjectName = config.Project.ProjectName,
                                CreatedAt = config.CreatedAt,
                                ModifiedAt = config.ModifiedAt
                            });
                        }
                    }
                    catch
                    {
                        // Skip invalid configurations
                    }
                }
            }
        }
        catch
        {
            // Return empty list on error
        }

        return summaries.OrderByDescending(s => s.ModifiedAt).ToList();
    }

    /// <summary>
    /// Deletes a configuration
    /// </summary>
    public (bool Success, string Message) DeleteConfiguration(string configurationId)
    {
        try
        {
            var configDir = GetConfigurationDirectory(configurationId);

            if (!Directory.Exists(configDir))
            {
                return (false, $"Configuration '{configurationId}' not found");
            }

            Directory.Delete(configDir, recursive: true);
            return (true, "Configuration deleted successfully");
        }
        catch (Exception ex)
        {
            return (false, $"Error deleting configuration: {ex.Message}");
        }
    }

    /// <summary>
    /// Checks if a configuration exists
    /// </summary>
    public bool ConfigurationExists(string configurationId)
    {
        var configPath = Path.Combine(GetConfigurationDirectory(configurationId), "workspace-config.json");
        return File.Exists(configPath);
    }

    /// <summary>
    /// Generates a unique configuration ID from a name
    /// </summary>
    public string GenerateConfigurationId(string name)
    {
        // Convert to valid directory name
        var id = new string(name
            .Where(c => char.IsLetterOrDigit(c) || c == '-' || c == '_')
            .ToArray());

        if (string.IsNullOrWhiteSpace(id))
        {
            id = "config";
        }

        // Ensure uniqueness
        var baseId = id;
        var counter = 1;
        while (ConfigurationExists(id))
        {
            id = $"{baseId}-{counter}";
            counter++;
        }

        return id;
    }

    private string GetConfigurationDirectory(string configurationId)
    {
        return Path.Combine(_configurationsRoot, configurationId);
    }

    private void EnsureConfigurationsDirectoryExists()
    {
        if (!Directory.Exists(_configurationsRoot))
        {
            Directory.CreateDirectory(_configurationsRoot);
        }
    }

    private static string GetTemplateRoot()
    {
        // Walk up from current directory to find WebTemplate.sln
        var dir = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);
        while (dir != null)
        {
            if (File.Exists(Path.Combine(dir.FullName, "WebTemplate.sln")))
            {
                return dir.FullName;
            }
            dir = dir.Parent;
        }

        // Fallback to current directory
        return AppDomain.CurrentDomain.BaseDirectory;
    }
}

/// <summary>
/// Summary information about a saved configuration
/// </summary>
public class ConfigurationSummary
{
    public string ConfigurationId { get; set; } = string.Empty;
    public string ConfigurationName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ProjectName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime ModifiedAt { get; set; }
}

using System.Diagnostics;
using Microsoft.Extensions.Logging;
using WebTemplate.TemplateEngine.Interfaces;
using WebTemplate.TemplateEngine.Models;
using WebTemplate.TemplateEngine.Steps;

namespace WebTemplate.TemplateEngine;

/// <summary>
/// Main template engine that orchestrates the project generation process
/// </summary>
public class TemplateEngine : ITemplateEngine
{
    private readonly ILogger<TemplateEngine> _logger;
    private readonly IServiceProvider _serviceProvider;

    public TemplateEngine(
        ILogger<TemplateEngine> logger,
        IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    /// <summary>
    /// Generates a new project from the template using the provided configuration
    /// </summary>
    public async Task<GenerationResult> GenerateAsync(
        WorkspaceConfiguration config,
        IProgress<string>? progress = null,
        CancellationToken cancellationToken = default)
    {
        var sw = Stopwatch.StartNew();
        var stepResults = new List<StepResult>();

        try
        {
            _logger.LogInformation("Starting template generation for project: {ProjectName}", config.Project.ProjectName);

            // Build the context for all steps
            var context = BuildContext(config);

            // Get all generation steps in execution order
            var steps = GetGenerationSteps();

            // Execute each step sequentially
            foreach (var step in steps)
            {
                _logger.LogInformation("Executing step {StepNumber}: {StepName}", step.StepNumber, step.StepName);

                var result = await step.ExecuteAsync(context, progress, cancellationToken);
                stepResults.Add(result);

                _logger.LogInformation("Step {StepNumber} result: {Success} - {Message}", step.StepNumber, result.Success, result.Message);

                // Stop on first failure
                if (!result.Success)
                {
                    sw.Stop();

                    return new GenerationResult
                    {
                        Success = false,
                        Message = $"Step {step.StepNumber} failed: {result.Message}",
                        StepResults = stepResults,
                        Duration = sw.Elapsed,
                        Exception = result.Exception
                    };
                }
            }

            sw.Stop();

            _logger.LogInformation("Template generation completed successfully in {Duration:F2}s", sw.Elapsed.TotalSeconds);

            return new GenerationResult
            {
                Success = true,
                Message = "Project generated successfully",
                GeneratedPath = context.TargetPath,
                StepResults = stepResults,
                Duration = sw.Elapsed
            };
        }
        catch (OperationCanceledException ex)
        {
            sw.Stop();
            _logger.LogWarning(ex, "Template generation was cancelled");

            return new GenerationResult
            {
                Success = false,
                Message = "Template generation was cancelled by user",
                StepResults = stepResults,
                Duration = sw.Elapsed,
                Exception = ex
            };
        }
        catch (Exception ex)
        {
            sw.Stop();
            _logger.LogError(ex, "Template generation failed with exception");

            return new GenerationResult
            {
                Success = false,
                Message = $"Template generation failed: {ex.Message}",
                StepResults = stepResults,
                Duration = sw.Elapsed,
                Exception = ex
            };
        }
    }

    /// <summary>
    /// Builds the template context from the configuration
    /// </summary>
    private TemplateContext BuildContext(WorkspaceConfiguration config)
    {
        var templateRoot = GetTemplateRoot();
        var targetPath = Path.GetFullPath(config.Project.TargetPath);
        var databaseName = $"{config.Project.ProjectName}Db";
        var connectionString = BuildConnectionString(databaseName);

        _logger.LogDebug("Template root: {TemplatePath}", templateRoot);
        _logger.LogDebug("Target path: {TargetPath}", targetPath);
        _logger.LogDebug("Database name: {DatabaseName}", databaseName);

        return new TemplateContext
        {
            Configuration = config,
            TemplatePath = templateRoot,
            TargetPath = targetPath,
            OldProjectName = "WebTemplate",
            NewProjectName = config.Project.ProjectName,
            DatabaseName = databaseName,
            ConnectionString = connectionString
        };
    }

    /// <summary>
    /// Gets all generation steps in execution order from the DI container
    /// </summary>
    private IEnumerable<IGenerationStep> GetGenerationSteps()
    {
        _logger.LogDebug("Retrieving generation steps from DI container");

        // Steps are resolved from DI by type
        // This list defines the execution order (must match StepNumber)
        var stepTypes = new[]
        {
            typeof(ValidateTemplateStep),
            typeof(CopyFilesStep),
            typeof(GenerateSolutionFileStep),
            typeof(RebrandProjectStep),
            typeof(UpdateConfigurationsStep),
            typeof(InitializeGitStep),
            typeof(ValidateProjectStep)
        };

        var steps = new List<IGenerationStep>();

        foreach (var stepType in stepTypes)
        {
            try
            {
                var step = _serviceProvider.GetService(stepType) as IGenerationStep;
                if (step != null)
                {
                    steps.Add(step);
                    _logger.LogDebug("Loaded step: {StepName} (Step {StepNumber})", step.StepName, step.StepNumber);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to resolve step: {StepType}", stepType.Name);
            }
        }

        return steps.OrderBy(s => s.StepNumber);
    }

    /// <summary>
    /// Locates the template root directory by walking up the directory tree looking for WebTemplate.sln
    /// </summary>
    private static string GetTemplateRoot()
    {
        var currentDir = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);

        while (currentDir != null)
        {
            var solutionFile = Path.Combine(currentDir.FullName, "WebTemplate.sln");
            if (File.Exists(solutionFile))
            {
                return currentDir.FullName;
            }

            currentDir = currentDir.Parent;
        }

        // Fallback to current directory if WebTemplate.sln not found
        return AppDomain.CurrentDomain.BaseDirectory;
    }

    /// <summary>
    /// Builds a SQL Server LocalDB connection string for the new database
    /// </summary>
    private static string BuildConnectionString(string databaseName)
    {
        return $"Server=(localdb)\\mssqllocaldb;Database={databaseName};" +
               "Trusted_Connection=true;MultipleActiveResultSets=true;TrustServerCertificate=true";
    }
}

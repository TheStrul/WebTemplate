using Microsoft.Extensions.Logging;
using WebTemplate.TemplateEngine.Interfaces;
using WebTemplate.TemplateEngine.Models;

namespace WebTemplate.TemplateEngine.Steps;

/// <summary>
/// Step 6: Updates configuration files (appsettings.json, package.json, README, etc.)
/// </summary>
public class UpdateConfigurationsStep : GenerationStepBase
{
    public override string StepName => "Updating Configurations";
    public override int StepNumber => 6;

    private readonly ConfigurationUpdater _updater;

    public UpdateConfigurationsStep(ILogger<UpdateConfigurationsStep> logger, ConfigurationUpdater updater)
        : base(logger)
    {
        _updater = updater;
    }

    public override async Task<StepResult> ExecuteAsync(
        TemplateContext context,
        IProgress<string>? progress = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            ReportStepProgress(progress);

            Logger.LogInformation("Updating configuration files at {TargetPath}", context.TargetPath);

            var result = await _updater.UpdateConfigurationsAsync(
                context, progress, cancellationToken);

            if (!result.Success)
            {
                Logger.LogError("Configuration update failed: {Message}", result.Message);
                return new StepResult(false, result.Message, result.Exception);
            }

            var message = $"Updated {result.FilesModified} configuration files " +
                         $"({result.SectionsUpdated} sections)";
            Logger.LogInformation("Configuration update successful: {Message}", message);

            return new StepResult(true, message);
        }
        catch (OperationCanceledException ex)
        {
            Logger.LogWarning(ex, "Configuration update operation was cancelled");
            return new StepResult(false, "Configuration update operation was cancelled", ex);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Configuration update operation failed with unexpected error");
            return new StepResult(false, $"Configuration update operation failed: {ex.Message}", ex);
        }
    }
}

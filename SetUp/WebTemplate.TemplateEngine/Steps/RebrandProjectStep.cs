using Microsoft.Extensions.Logging;
using WebTemplate.TemplateEngine.Interfaces;
using WebTemplate.TemplateEngine.Models;

namespace WebTemplate.TemplateEngine.Steps;

/// <summary>
/// Step 5: Rebrands all files, directories, and file contents from old project name to new project name
/// </summary>
public class RebrandProjectStep : GenerationStepBase
{
    public override string StepName => "Rebranding Project";
    public override int StepNumber => 5;

    private readonly FileRebrander _rebrander;

    public RebrandProjectStep(ILogger<RebrandProjectStep> logger, FileRebrander rebrander)
        : base(logger)
    {
        _rebrander = rebrander;
    }

    public override async Task<StepResult> ExecuteAsync(
        TemplateContext context,
        IProgress<string>? progress = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            ReportStepProgress(progress);

            Logger.LogInformation("Rebranding project from {OldName} to {NewName} at {TargetPath}",
                context.OldProjectName, context.NewProjectName, context.TargetPath);

            var result = await _rebrander.RebrandAsync(
                context.TargetPath,
                context.OldProjectName,
                context.NewProjectName,
                progress,
                cancellationToken);

            if (!result.Success)
            {
                Logger.LogError("Rebranding failed: {Message}", result.Message);
                return new StepResult(false, result.Message, result.Exception);
            }

            var message = $"Rebranded {result.ItemsRenamed} items, modified {result.FilesModified} files " +
                         $"({result.TextFilesProcessed} text, {result.BinaryFilesSkipped} binary skipped)";
            Logger.LogInformation("Rebranding successful: {Message}", message);

            return new StepResult(true, message);
        }
        catch (OperationCanceledException ex)
        {
            Logger.LogWarning(ex, "Rebranding operation was cancelled");
            return new StepResult(false, "Rebranding operation was cancelled", ex);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Rebranding operation failed with unexpected error");
            return new StepResult(false, $"Rebranding operation failed: {ex.Message}", ex);
        }
    }
}

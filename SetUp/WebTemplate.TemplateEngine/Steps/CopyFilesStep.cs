using Microsoft.Extensions.Logging;
using WebTemplate.TemplateEngine.Interfaces;
using WebTemplate.TemplateEngine.Models;

namespace WebTemplate.TemplateEngine.Steps;

/// <summary>
/// Step 3: Copies template files from source to target directory with exclusion filters
/// </summary>
public class CopyFilesStep : GenerationStepBase
{
    public override string StepName => "Copying Template Files";
    public override int StepNumber => 3;

    private readonly FileCopier _copier;

    public CopyFilesStep(ILogger<CopyFilesStep> logger, FileCopier copier)
        : base(logger)
    {
        _copier = copier;
    }

    public override async Task<StepResult> ExecuteAsync(
        TemplateContext context,
        IProgress<string>? progress = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            ReportStepProgress(progress);

            Logger.LogInformation("Copying template files from {TemplatePath} to {TargetPath}",
                context.TemplatePath, context.TargetPath);

            var result = await _copier.CopyTemplateAsync(
                context.TemplatePath,
                context.TargetPath,
                context.IncludeFolders,
                context.ExcludeDirectories,
                context.ExcludeFiles,
                progress,
                cancellationToken);

            if (!result.Success)
            {
                Logger.LogError("Copy operation failed: {Message}", result.Message);
                return new StepResult(false, result.Message, result.Exception);
            }

            var message = $"Copied {result.FileCount} files, {result.DirectoryCount} directories ({result.TotalBytes} bytes)";
            Logger.LogInformation("Copy operation successful: {Message}", message);

            return new StepResult(true, message);
        }
        catch (OperationCanceledException ex)
        {
            Logger.LogWarning(ex, "Copy operation was cancelled");
            return new StepResult(false, "Copy operation was cancelled", ex);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Copy operation failed with unexpected error");
            return new StepResult(false, $"Copy operation failed: {ex.Message}", ex);
        }
    }
}

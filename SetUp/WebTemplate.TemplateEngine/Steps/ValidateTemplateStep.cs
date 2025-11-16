using Microsoft.Extensions.Logging;
using WebTemplate.TemplateEngine.Interfaces;
using WebTemplate.TemplateEngine.Models;

namespace WebTemplate.TemplateEngine.Steps;

/// <summary>
/// Step 2: Validates that the template structure is intact and contains all required components
/// </summary>
public class ValidateTemplateStep : GenerationStepBase
{
    public override string StepName => "Validating Template";
    public override int StepNumber => 2;

    // Required paths that must exist in the template
    private static readonly string[] RequiredPaths =
    [
        "Backend",
        "Backend/WebTemplate.API",
        "Backend/WebTemplate.Core",
        "Backend/WebTemplate.Data",
        "Frontend"
    ];

    public ValidateTemplateStep(ILogger<ValidateTemplateStep> logger)
        : base(logger)
    {
    }

    public override Task<StepResult> ExecuteAsync(
        TemplateContext context,
        IProgress<string>? progress = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            ReportStepProgress(progress);

            Logger.LogInformation("Validating template structure at {TemplatePath}", context.TemplatePath);

            var missingPaths = new List<string>();

            foreach (var requiredPath in RequiredPaths)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var fullPath = Path.Combine(context.TemplatePath, requiredPath);

                if (!Directory.Exists(fullPath))
                {
                    Logger.LogWarning("Required path not found: {Path}", fullPath);
                    missingPaths.Add(requiredPath);
                }
                else
                {
                    Logger.LogDebug("Found required path: {Path}", requiredPath);
                }
            }

            if (missingPaths.Count > 0)
            {
                var message = $"Template validation failed. Missing paths: {string.Join(", ", missingPaths)}";
                Logger.LogError(message);
                return Task.FromResult(new StepResult(false, message));
            }

            var successMessage = "Template structure validated successfully";
            Logger.LogInformation(successMessage);
            return Task.FromResult(new StepResult(true, successMessage));
        }
        catch (OperationCanceledException ex)
        {
            Logger.LogWarning(ex, "Template validation was cancelled");
            return Task.FromResult(new StepResult(false, "Template validation was cancelled", ex));
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Template validation failed with unexpected error");
            return Task.FromResult(new StepResult(false, $"Template validation failed: {ex.Message}", ex));
        }
    }
}

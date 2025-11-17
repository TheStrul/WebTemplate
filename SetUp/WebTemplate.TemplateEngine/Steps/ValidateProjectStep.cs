using Microsoft.Extensions.Logging;
using WebTemplate.TemplateEngine.Interfaces;
using WebTemplate.TemplateEngine.Models;

namespace WebTemplate.TemplateEngine.Steps;

/// <summary>
/// Step 8: Validates that the newly generated project has the correct structure and all files were renamed properly
/// </summary>
public class ValidateProjectStep : GenerationStepBase
{
    public override string StepName => "Validating Generated Project";
    public override int StepNumber => 8;

    public ValidateProjectStep(ILogger<ValidateProjectStep> logger)
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
            // Skip if not requested
            if (!context.Configuration.Project.RunValidation)
            {
                ReportProgress(progress, "Skipping Validation");
                Logger.LogInformation("Project validation skipped by configuration");
                return Task.FromResult(new StepResult(true, "Validation skipped"));
            }

            ReportStepProgress(progress);

            Logger.LogInformation("Validating generated project at {TargetPath}", context.TargetPath);

            var issues = new List<string>();

            // 1. Check that Backend folder exists and has been renamed
            var backendPath = Path.Combine(
                context.TargetPath,
                "Backend",
                $"{context.NewProjectName}.API");

            if (!Directory.Exists(backendPath))
            {
                var msg = $"Backend API project not found at: {backendPath}";
                Logger.LogWarning(msg);
                issues.Add(msg);
            }
            else
            {
                Logger.LogDebug("Backend API project found: {Path}", backendPath);
            }

            // 2. Check that Frontend folder exists and has been renamed
            var frontendPath = Path.Combine(
                context.TargetPath,
                "Frontend",
                $"{context.NewProjectName}-frontend");

            if (!Directory.Exists(frontendPath))
            {
                var msg = $"Frontend project not found at: {frontendPath}";
                Logger.LogWarning(msg);
                issues.Add(msg);
            }
            else
            {
                Logger.LogDebug("Frontend project found: {Path}", frontendPath);
            }

            // 3. Check for any remaining "WebTemplate" references in solution files
            var solutionFiles = Directory.GetFiles(
                context.TargetPath,
                "*.sln",
                SearchOption.TopDirectoryOnly);

            foreach (var slnFile in solutionFiles)
            {
                cancellationToken.ThrowIfCancellationRequested();

                try
                {
                    var content = File.ReadAllText(slnFile);
                    if (content.Contains("WebTemplate") && !content.Contains(context.NewProjectName))
                    {
                        var msg = $"Solution file still contains 'WebTemplate' reference: {Path.GetFileName(slnFile)}";
                        Logger.LogWarning(msg);
                        issues.Add(msg);
                    }
                    else
                    {
                        Logger.LogDebug("Solution file properly rebranded: {FileName}", Path.GetFileName(slnFile));
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogWarning(ex, "Could not read solution file: {Path}", slnFile);
                }
            }

            if (issues.Count > 0)
            {
                var message = $"Validation found {issues.Count} issue(s):\n" + string.Join("\n", issues);
                Logger.LogError("Project validation failed with {IssueCount} issues", issues.Count);
                return Task.FromResult(new StepResult(false, message));
            }

            var successMessage = "Project validation passed successfully";
            Logger.LogInformation(successMessage);
            return Task.FromResult(new StepResult(true, successMessage));
        }
        catch (OperationCanceledException ex)
        {
            Logger.LogWarning(ex, "Project validation was cancelled");
            return Task.FromResult(new StepResult(false, "Project validation was cancelled", ex));
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Project validation failed with unexpected error");
            return Task.FromResult(new StepResult(false, $"Project validation failed: {ex.Message}", ex));
        }
    }
}

using Microsoft.Extensions.Logging;
using WebTemplate.TemplateEngine.Interfaces;
using WebTemplate.TemplateEngine.Models;

namespace WebTemplate.TemplateEngine.Steps;

/// <summary>
/// Step 7: Initializes a Git repository with an initial commit (optional)
/// </summary>
public class InitializeGitStep : GenerationStepBase
{
    public override string StepName => "Initializing Git Repository";
    public override int StepNumber => 7;

    private readonly GitInitializer _gitInitializer;

    public InitializeGitStep(ILogger<InitializeGitStep> logger, GitInitializer gitInitializer)
        : base(logger)
    {
        _gitInitializer = gitInitializer;
    }

    public override async Task<StepResult> ExecuteAsync(
        TemplateContext context,
        IProgress<string>? progress = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Skip if not requested
            if (!context.Configuration.Project.InitializeGit)
            {
                ReportProgress(progress, "Skipping Git Initialization");
                Logger.LogInformation("Git initialization skipped by configuration");
                return new StepResult(true, "Git initialization skipped");
            }

            ReportStepProgress(progress);

            Logger.LogInformation("Initializing git repository for {ProjectName} at {TargetPath}",
                context.NewProjectName, context.TargetPath);

            var result = await _gitInitializer.InitializeRepositoryAsync(
                context.TargetPath,
                context.NewProjectName,
                context.Configuration.Project.CreateInitialCommit,
                progress,
                cancellationToken);

            if (!result.Success)
            {
                Logger.LogError("Git initialization failed: {Message}", result.Message);
                return new StepResult(false, result.Message, result.Exception);
            }

            var message = result.InitialCommitCreated
                ? "Git repository initialized with initial commit"
                : "Git repository initialized (no commit)";

            Logger.LogInformation("Git initialization successful: {Message}", message);

            return new StepResult(true, message);
        }
        catch (OperationCanceledException ex)
        {
            Logger.LogWarning(ex, "Git initialization operation was cancelled");
            return new StepResult(false, "Git initialization operation was cancelled", ex);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Git initialization operation failed with unexpected error");
            return new StepResult(false, $"Git initialization operation failed: {ex.Message}", ex);
        }
    }
}

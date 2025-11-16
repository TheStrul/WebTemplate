using Microsoft.Extensions.Logging;
using WebTemplate.TemplateEngine.Interfaces;
using WebTemplate.TemplateEngine.Models;

namespace WebTemplate.TemplateEngine.Steps;

/// <summary>
/// Step 6: Initializes git repository and optionally creates initial commit
/// [PLACEHOLDER - Implementation in Phase 5]
/// </summary>
public class InitializeGitStep : GenerationStepBase
{
    public override string StepName => "Initializing Git Repository";
    public override int StepNumber => 6;

    public InitializeGitStep(ILogger<InitializeGitStep> logger)
        : base(logger)
    {
    }

    public override Task<StepResult> ExecuteAsync(
        TemplateContext context,
        IProgress<string>? progress = null,
        CancellationToken cancellationToken = default)
    {
        // Skip if not requested
        if (!context.Configuration.Project.InitializeGit)
        {
            ReportProgress(progress, "Skipping Git Initialization");
            Logger.LogInformation("Git initialization skipped by configuration");
            return Task.FromResult(new StepResult(true, "Git initialization skipped"));
        }

        ReportStepProgress(progress);
        Logger.LogInformation("[PLACEHOLDER] Step {StepNumber}: {StepName}", StepNumber, StepName);
        return Task.FromResult(new StepResult(true, "Placeholder - Phase 5 implementation pending"));
    }
}

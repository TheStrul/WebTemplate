using Microsoft.Extensions.Logging;
using WebTemplate.TemplateEngine.Interfaces;
using WebTemplate.TemplateEngine.Models;

namespace WebTemplate.TemplateEngine.Steps;

/// <summary>
/// Step 4: Rebrands all files, directories, and file contents from old project name to new project name
/// [PLACEHOLDER - Implementation in Phase 3]
/// </summary>
public class RebrandProjectStep : GenerationStepBase
{
    public override string StepName => "Rebranding Project";
    public override int StepNumber => 4;

    public RebrandProjectStep(ILogger<RebrandProjectStep> logger)
        : base(logger)
    {
    }

    public override Task<StepResult> ExecuteAsync(
        TemplateContext context,
        IProgress<string>? progress = null,
        CancellationToken cancellationToken = default)
    {
        ReportStepProgress(progress);
        Logger.LogInformation("[PLACEHOLDER] Step {StepNumber}: {StepName}", StepNumber, StepName);
        return Task.FromResult(new StepResult(true, "Placeholder - Phase 3 implementation pending"));
    }
}

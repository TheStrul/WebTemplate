using Microsoft.Extensions.Logging;
using WebTemplate.TemplateEngine.Interfaces;
using WebTemplate.TemplateEngine.Models;

namespace WebTemplate.TemplateEngine.Steps;

/// <summary>
/// Step 5: Updates configuration files (appsettings.json, package.json, copilot-instructions.md, README files)
/// [PLACEHOLDER - Implementation in Phase 4]
/// </summary>
public class UpdateConfigurationsStep : GenerationStepBase
{
    public override string StepName => "Updating Configurations";
    public override int StepNumber => 5;

    public UpdateConfigurationsStep(ILogger<UpdateConfigurationsStep> logger)
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
        return Task.FromResult(new StepResult(true, "Placeholder - Phase 4 implementation pending"));
    }
}

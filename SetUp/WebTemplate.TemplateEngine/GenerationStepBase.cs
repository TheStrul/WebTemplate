using Microsoft.Extensions.Logging;
using WebTemplate.TemplateEngine.Interfaces;
using WebTemplate.TemplateEngine.Models;

namespace WebTemplate.TemplateEngine;

/// <summary>
/// Base class for implementation steps, providing common functionality
/// </summary>
public abstract class GenerationStepBase : IGenerationStep
{
    protected readonly ILogger Logger;

    public abstract string StepName { get; }
    public abstract int StepNumber { get; }

    protected GenerationStepBase(ILogger logger)
    {
        Logger = logger;
    }

    public abstract Task<StepResult> ExecuteAsync(
        TemplateContext context,
        IProgress<string>? progress = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Reports progress with formatted step information
    /// </summary>
    protected void ReportProgress(IProgress<string>? progress, string message)
    {
        progress?.Report($"Step {StepNumber}/{7}: {message}");
        Logger.LogInformation("Step {StepNumber}: {Message}", StepNumber, message);
    }

    /// <summary>
    /// Reports progress with step name and custom message
    /// </summary>
    protected void ReportStepProgress(IProgress<string>? progress)
    {
        ReportProgress(progress, StepName);
    }
}

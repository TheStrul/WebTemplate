using WebTemplate.TemplateEngine.Models;

namespace WebTemplate.TemplateEngine.Interfaces;

/// <summary>
/// Defines a single step in the template generation process
/// </summary>
public interface IGenerationStep
{
    /// <summary>
    /// Display name of the step
    /// </summary>
    string StepName { get; }

    /// <summary>
    /// Step number in the generation sequence (1-based)
    /// </summary>
    int StepNumber { get; }

    /// <summary>
    /// Executes the generation step
    /// </summary>
    /// <param name="context">Template generation context containing all required information</param>
    /// <param name="progress">Optional progress reporter for UI updates</param>
    /// <param name="cancellationToken">Cancellation token for cooperative cancellation</param>
    /// <returns>Result of the step execution</returns>
    Task<StepResult> ExecuteAsync(
        TemplateContext context,
        IProgress<string>? progress = null,
        CancellationToken cancellationToken = default);
}

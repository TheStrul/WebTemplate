using WebTemplate.TemplateEngine.Models;

namespace WebTemplate.TemplateEngine.Interfaces;

/// <summary>
/// Main template engine interface for generating projects from the WebTemplate
/// </summary>
public interface ITemplateEngine
{
    /// <summary>
    /// Generates a new project from the template using the provided configuration
    /// </summary>
    /// <param name="config">Workspace configuration containing all project settings</param>
    /// <param name="progress">Optional progress reporter for UI updates</param>
    /// <param name="cancellationToken">Cancellation token for cooperative cancellation</param>
    /// <returns>Result of the generation process including success status and generated path</returns>
    Task<GenerationResult> GenerateAsync(
        WorkspaceConfiguration config,
        IProgress<string>? progress = null,
        CancellationToken cancellationToken = default);
}

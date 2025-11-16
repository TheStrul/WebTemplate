namespace WebTemplate.TemplateEngine.Models;

/// <summary>
/// Final result of the complete template generation process
/// </summary>
public class GenerationResult
{
    /// <summary>
    /// Whether the generation completed successfully
    /// </summary>
    public required bool Success { get; init; }

    /// <summary>
    /// Human-readable message describing the overall result
    /// </summary>
    public required string Message { get; init; }

    /// <summary>
    /// Full path to the generated project directory (null if generation failed)
    /// </summary>
    public string? GeneratedPath { get; init; }

    /// <summary>
    /// Results from each individual step
    /// </summary>
    public List<StepResult> StepResults { get; init; } = [];

    /// <summary>
    /// Total duration of the generation process
    /// </summary>
    public TimeSpan Duration { get; init; }

    /// <summary>
    /// Exception that caused generation to fail, if any
    /// </summary>
    public Exception? Exception { get; init; }
}

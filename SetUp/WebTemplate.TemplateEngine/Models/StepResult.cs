namespace WebTemplate.TemplateEngine.Models;

/// <summary>
/// Result from executing a single generation step
/// </summary>
/// <param name="Success">Whether the step completed successfully</param>
/// <param name="Message">Human-readable message describing the result</param>
/// <param name="Exception">Exception that occurred, if any</param>
public record StepResult(
    bool Success,
    string Message,
    Exception? Exception = null);

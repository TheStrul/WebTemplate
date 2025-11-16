using Microsoft.Extensions.Logging;
using WebTemplate.TemplateEngine.Interfaces;

namespace WebTemplate.TemplateEngine;

/// <summary>
/// Factory for creating and managing generation steps
/// </summary>
public class GenerationStepFactory
{
    private readonly ILogger<GenerationStepFactory> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly List<Type> _registeredSteps = [];

    public GenerationStepFactory(
        ILogger<GenerationStepFactory> logger,
        IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    /// <summary>
    /// Registers a step type for use during generation
    /// </summary>
    public void RegisterStep(Type stepType)
    {
        if (!typeof(IGenerationStep).IsAssignableFrom(stepType))
        {
            throw new ArgumentException(
                $"Step type '{stepType.Name}' must implement IGenerationStep",
                nameof(stepType));
        }

        _registeredSteps.Add(stepType);
        _logger.LogDebug("Registered generation step: {StepName}", stepType.Name);
    }

    /// <summary>
    /// Gets all registered steps in order, resolved from DI
    /// </summary>
    public IEnumerable<IGenerationStep> GetAllSteps()
    {
        return _registeredSteps
            .Select(stepType => _serviceProvider.GetService(stepType) as IGenerationStep)
            .Where(step => step != null)
            .OrderBy(step => step!.StepNumber)
            .Cast<IGenerationStep>();
    }

    /// <summary>
    /// Gets a specific step by type
    /// </summary>
    public T? GetStep<T>() where T : class, IGenerationStep
    {
        return _serviceProvider.GetService(typeof(T)) as T;
    }
}

using Microsoft.Extensions.DependencyInjection;
using WebTemplate.TemplateEngine.Interfaces;

namespace WebTemplate.TemplateEngine;

/// <summary>
/// Extension methods for registering template engine services with DI
/// </summary>
public static class TemplateEngineServiceCollectionExtensions
{
    /// <summary>
    /// Registers the template engine and all generation steps
    /// </summary>
    /// <remarks>
    /// Call this in your DI configuration to register:
    /// - ITemplateEngine and TemplateEngine implementation
    /// - GenerationStepFactory
    /// - All individual generation steps (as they are implemented)
    /// </remarks>
    public static IServiceCollection AddTemplateEngine(this IServiceCollection services)
    {
        // Core engine
        services.AddSingleton<ITemplateEngine, TemplateEngine>();
        services.AddSingleton<GenerationStepFactory>();

        // Generation steps will be registered here as they are implemented
        // For Phase 1, this is the foundation - steps will be added in later phases

        return services;
    }

    /// <summary>
    /// Registers a generation step with the DI container
    /// </summary>
    /// <typeparam name="TStep">The step implementation type</typeparam>
    public static IServiceCollection AddGenerationStep<TStep>(this IServiceCollection services)
        where TStep : class, IGenerationStep
    {
        services.AddTransient<TStep>();
        return services;
    }
}

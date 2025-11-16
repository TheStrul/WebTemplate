using Microsoft.Extensions.DependencyInjection;
using WebTemplate.TemplateEngine.Interfaces;
using WebTemplate.TemplateEngine.Steps;

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
    /// - All individual generation steps
    /// - Helper services (FileCopier, etc.)
    /// </remarks>
    public static IServiceCollection AddTemplateEngine(this IServiceCollection services)
    {
        // Core engine
        services.AddSingleton<ITemplateEngine, TemplateEngine>();
        services.AddSingleton<GenerationStepFactory>();

        // Helper services
        services.AddSingleton<FileCopier>();

        // Generation steps (in execution order)
        services.AddTransient<ValidateTemplateStep>();
        services.AddTransient<CopyFilesStep>();
        services.AddTransient<RebrandProjectStep>();
        services.AddTransient<UpdateConfigurationsStep>();
        services.AddTransient<InitializeGitStep>();
        services.AddTransient<ValidateProjectStep>();

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

using Microsoft.Extensions.DependencyInjection;
using WebTemplate.Setup.Services;
using WebTemplate.Setup.UI;

namespace WebTemplate.Setup;

static class Program
{
    /// <summary>
    ///  The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main()
    {
        // To customize application configuration such as set high DPI settings or default font,
        // see https://aka.ms/applicationconfiguration.
        ApplicationConfiguration.Initialize();

        var services = new ServiceCollection();
        ConfigureServices(services);

        using var provider = services.BuildServiceProvider(validateScopes: true);

        var mainForm = provider.GetRequiredService<MainForm>();
        UITheme.Apply(mainForm);
        Application.Run(mainForm);
    }

    private static void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<INotificationService, NotificationService>();

        // Services used by MainForm
        services.AddSingleton<ConfigurationPersistenceService>();
        services.AddSingleton<ProjectGenerationService>();

        // Forms
        services.AddTransient<MainForm>();
    }
}

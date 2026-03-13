using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using VehicleVision.Pleasanter.ExtensionsTools.Common.Configuration;
using VehicleVision.Pleasanter.ExtensionsTools.Common.Services;
using VehicleVision.Pleasanter.ExtensionsTools.DiffViewer.ViewModels;
using VehicleVision.Pleasanter.ExtensionsTools.DiffViewer.Views;

namespace VehicleVision.Pleasanter.ExtensionsTools.DiffViewer;

public class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var services = ConfigureServices();
            var mainVm = services.GetRequiredService<MainWindowViewModel>();
            desktop.MainWindow = new MainWindow
            {
                DataContext = mainVm,
            };
        }

        base.OnFrameworkInitializationCompleted();
    }

    private static ServiceProvider ConfigureServices()
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
            .AddJsonFile("local.config.json", optional: true, reloadOnChange: false)
            .AddEnvironmentVariables(prefix: "EXTENSIONS_SYNC_")
            .Build();

        var settings = new AppSettings
        {
            BaseUrl = configuration["BaseUrl"] ?? string.Empty,
            ApiKey = configuration["ApiKey"] ?? string.Empty,
            ParametersPath = configuration["ParametersPath"] ?? string.Empty,
        };

        var services = new ServiceCollection();
        services.AddSingleton(settings);
        services.AddHttpClient<IPleasanterApiClient, PleasanterApiClient>();
        services.AddSingleton<IExtensionsFileService, ExtensionsFileService>();
        services.AddSingleton<IDiffService, DiffService>();
        services.AddSingleton<SyncService>();
        services.AddTransient<MainWindowViewModel>();

        return services.BuildServiceProvider();
    }
}

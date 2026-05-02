using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.UI.Xaml;
using Serilog;
using System;
using System.Net.Http;
using TaskManager.WinUI.ViewModels;
using TaskManager.WinUI.Localization;
using TaskManager.WinUI.Services;

namespace TaskManager.WinUI;

public partial class App : Application
{
    public static IServiceProvider Services { get; private set; } = null!;

    public App()
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.Console()
            .WriteTo.File("logs/ui-.log", rollingInterval: Serilog.RollingInterval.Day)
            .CreateLogger();

        Services = BuildServiceProvider();
        InitializeComponent();
        RequestedTheme = ApplicationTheme.Light;
    }

    protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
    {
        var window = Services.GetRequiredService<MainWindow>();
        window.Activate();
    }

    private static IServiceProvider BuildServiceProvider()
    {
        var services = new ServiceCollection();

        // LOgger
        services.AddLogging(builder =>
        {
            builder.ClearProviders();
            builder.AddSerilog(Log.Logger, dispose: false);
        });

        services.AddSingleton<HttpClient>();
        services.AddSingleton<IApiClient, ApiClient>();

        services.AddSingleton<ITokenStorage, TokenStorage>();
        services.AddSingleton<IAuthService, AuthService>();

        services.AddSingleton<INavigationService, NavigationService>();

        services.AddSingleton<LocalizationService>();

        services.AddSingleton<LoginViewModel>();
        services.AddSingleton<TaskViewModel>();

        services.AddSingleton<MainWindow>();

        return services.BuildServiceProvider();
    }
}

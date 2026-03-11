using System.Net.Http;
using System.Windows;
using Microsoft.Extensions.Logging;
using Prism.DryIoc;
using Prism.Ioc;
using Serilog;
using TaskManager.UI.Services;
using TaskManager.UI.ViewModels;
using TaskManager.UI.Views;

namespace TaskManager.UI;

public partial class App : PrismApplication
{
    protected override Window CreateShell()
    {
        return Container.Resolve<ShellWindow>();
    }

    protected override void OnInitialized()
    {
        base.OnInitialized();
        
        // Serilog
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.Console()
            .WriteTo.File("logs/ui-.log", rollingInterval: RollingInterval.Day)
            .CreateLogger();
        
        Log.Information("Application started");
    }
    
    protected override void RegisterTypes(IContainerRegistry containerRegistry)
    {
        // Logging
        containerRegistry.RegisterInstance<ILoggerFactory>(new LoggerFactory().AddSerilog());
        containerRegistry.Register(typeof(ILogger<>), typeof(Logger<>));

        // HTTP Client
        containerRegistry.RegisterSingleton<HttpClient>();
        containerRegistry.RegisterSingleton<IApiClient, ApiClient>();

        // Language
        containerRegistry.RegisterSingleton<LanguageService>();
        containerRegistry.RegisterSingleton<LanguageViewModel>();

        // ViewModels
        containerRegistry.RegisterForNavigation<LoginView, LoginViewModel>();
        containerRegistry.RegisterForNavigation<TaskView, TaskViewModel>();
        
        containerRegistry.RegisterSingleton<ShellWindow>();
    }
    
    protected override void OnExit(ExitEventArgs e)
    {
        Log.CloseAndFlush();
        base.OnExit(e);
    }
}
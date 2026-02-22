using System.IO;
using System.Windows;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using TaskManager.Application.Services;
using TaskManager.Domain.Interfaces;
using TaskManager.Infrastructure.Data;
using TaskManager.Infrastructure.Repositories;
using TaskManager.UI.ViewModels;
using TaskManager.UI.Views;

namespace TaskManager.UI
{
    public partial class App : System.Windows.Application
    {
        private IHost? _host;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddUserSecrets<App>()
                .Build();

            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(configuration)
                .CreateLogger();

            try
            {
                Log.Information("Starting application");
                
                _host = Host.CreateDefaultBuilder()
                    .UseSerilog()
                    .ConfigureServices((context, services) =>
                    {
                        var conn = configuration.GetConnectionString("DefaultConnection");
                        
                        Log.Information("Connection to DB...");
                        services.AddDbContext<AppDbContext>(options =>
                            options.UseNpgsql(conn));

                        // Infrastructure -> Application
                        services.AddScoped<IUserRepository, UserRepository>();
                        services.AddScoped<ITaskRepository, TaskRepository>();

                        // Application services
                        services.AddScoped<IAuthService, AuthService>();
                        services.AddScoped<ITaskService, TaskService>();

                        // ViewModels
                        services.AddTransient<MainViewModel>();
                        services.AddTransient<TaskViewModel>();
                        services.AddTransient<LoginViewModel>();
                        services.AddTransient<SettingsViewModel>();

                        // Views
                        services.AddTransient<MainWindow>();

                        // Language service
                        services.AddSingleton<LanguageService>();

                        services.AddSingleton<ICurrentUserService, CurrentUserService>();
                    })
                    .Build();
                
                _host.Start();

                var mainWindow = _host.Services.GetRequiredService<MainWindow>();
                mainWindow.Show();
                
                Log.Information("Application started");
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Application startup failed");
            }
        }

        protected override async void OnExit(ExitEventArgs e)
        {
            try
            {
                Log.Information("Shutting down application");

                if (_host != null)
                {
                    await _host.StopAsync();
                    _host.Dispose();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error during shutting down");
            }
            finally
            {
                Log.CloseAndFlush();
                base.OnExit(e);
            }
        }
    }
}

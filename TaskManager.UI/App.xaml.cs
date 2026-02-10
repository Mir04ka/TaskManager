using System;
using System.Windows;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
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

            _host = Host.CreateDefaultBuilder()
                .ConfigureServices((context, services) =>
                {
                    var conn = "Host=localhost;Port=5432;Database=taskmanager;Username=postgres;Password=1234";
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
        }

        protected override async void OnExit(ExitEventArgs e)
        {
            if (_host != null)
            {
                await _host.StopAsync();
                _host.Dispose();
            }
            base.OnExit(e);
        }
    }
}

using Microsoft.Extensions.Logging;
using Microsoft.UI.Xaml;
using System;
using TaskManager.WinUI.Services;
using TaskManager.WinUI.Views;

namespace TaskManager.WinUI;

public sealed partial class MainWindow : Window
{
    private readonly IAuthService _authService;
    private readonly INavigationService _navigationService;
    private readonly ILogger<MainWindow> _logger;

    public MainWindow(IAuthService authService, INavigationService navigationService, ILogger<MainWindow> logger)
    {
        InitializeComponent();
        _authService = authService;
        _navigationService = navigationService;
        _logger = logger;

        _navigationService.SetFrame(ContentFrame);

        ContentFrame.Loaded += OnFrameLoaded;
    }

    private async void OnFrameLoaded(object sender, RoutedEventArgs e)
    {
        try
        {
            _logger.LogInformation("Checking for saved tokens");
            var autoLoginSuccess = await _authService.TryAutoLoginAsync();
            if (autoLoginSuccess)
            {
                _logger.LogInformation("Auto login successful");
                _navigationService.NavigateTo<MainPage>();
            }
            else
            {
                _logger.LogInformation("Auto login failed");
                _navigationService.NavigateTo<LoginPage>();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during auto login");
            _navigationService.NavigateTo<LoginPage>();
        }
    }
}
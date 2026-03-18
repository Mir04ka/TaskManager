using System.Windows;
using Microsoft.Extensions.Logging;
using Prism.Regions;
using TaskManager.UI.Services;

namespace TaskManager.UI.Views;

public partial class ShellWindow : MahApps.Metro.Controls.MetroWindow
{
    private readonly IRegionManager _regionManager;
    private readonly IAuthService _authService;
    private readonly ILogger<ShellWindow> _logger;
    
    public ShellWindow(IRegionManager regionManager, IAuthService authService, ILogger<ShellWindow> logger)
    {
        InitializeComponent();
        _regionManager = regionManager;
        _authService = authService;
        _logger = logger;
        
        Loaded += OnLoaded; // delay before nav
    }
    
    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        try
        {
            _logger.LogInformation("Checking for saved token");
            var autoLoginSuccess = await _authService.TryAutoLoginAsync();
            if (autoLoginSuccess)
            {
                _logger.LogInformation("Auto-login successful");
                _regionManager.RequestNavigate("ContentRegion", "TaskView");
            }
            else
            {
                _logger.LogInformation("Auto-login failed");
                _regionManager.RequestNavigate("ContentRegion", "LoginView");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during auto-login");
            _regionManager.RequestNavigate("ContentRegion", "LoginView");
        }
    }
}
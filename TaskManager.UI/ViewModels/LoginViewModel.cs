using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Extensions.Logging;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using TaskManager.UI.Services;

namespace TaskManager.UI.ViewModels;

public class LoginViewModel : BindableBase, INavigationAware
{
    private readonly IApiClient _apiClient;
    private readonly IRegionManager _regionManager;
    private readonly ILogger<LoginViewModel> _logger;
    private readonly LanguageService _languageService;

    private string _username = string.Empty;
    public string Username
    {
        get => _username;
        set => SetProperty(ref _username, value);
    }

    private string _statusMessage = string.Empty;
    public string StatusMessage
    {
        get => _statusMessage;
        set => SetProperty(ref _statusMessage, value);
    }

    public LanguageViewModel LanguageVM { get; }

    public ICommand LoginCommand { get; }
    public ICommand RegisterCommand { get; }

    public LoginViewModel(IApiClient apiClient, IRegionManager regionManager, ILogger<LoginViewModel> logger, LanguageViewModel languageViewModel)
    {
        _apiClient = apiClient;
        _regionManager = regionManager;
        _logger = logger;
        LanguageVM = languageViewModel;

        LoginCommand = new DelegateCommand<string>(async password => await Login(password));
        RegisterCommand = new DelegateCommand<string>(async password => await Register(password));
    }

    private async Task Login(string? password)
    {
        if (string.IsNullOrWhiteSpace(password))
        {
            StatusMessage = "Enter password";
            return;
        }

        _logger.LogDebug("Login attempt for username: {Username}", Username);

        var response = await _apiClient.LoginAsync(Username, password);

        if (response != null)
        {
            StatusMessage = "Login success";
            _logger.LogInformation("Login successful, navigating to tasks");
            _regionManager.RequestNavigate("ContentRegion", "TaskView");
        }
        else
        {
            StatusMessage = "Wrong username or password";
        }
    }

    private async Task Register(string? password)
    {
        if (string.IsNullOrWhiteSpace(password))
        {
            StatusMessage = "Enter password";
            return;
        }

        _logger.LogDebug("Register attempt for username: {Username}", Username);

        var response = await _apiClient.RegisterAsync(Username, password);

        if (response != null)
        {
            StatusMessage = "User has been registered successfully";
        }
        else
        {
            StatusMessage = "User already registered";
        }
    }

    public void OnNavigatedTo(NavigationContext navigationContext) { }
    public bool IsNavigationTarget(NavigationContext navigationContext) => true;
    public void OnNavigatedFrom(NavigationContext navigationContext) { }
}
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using TaskManager.WinUI.Localization;
using TaskManager.WinUI.Services;
using TaskManager.WinUI.Views;

namespace TaskManager.WinUI.ViewModels;

public sealed partial class LoginViewModel : BaseViewModel
{
    private readonly IApiClient _apiClient;
    private readonly INavigationService _navigationService;
    private readonly ILogger<LoginViewModel> _logger;
    private readonly IAuthService _authService;

    [ObservableProperty]
    private string _username = string.Empty;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    public LocalizationService Loc { get; }

    public LoginViewModel(
        IApiClient apiClient, 
        INavigationService navigationService,
        IAuthService authService,
        ILogger<LoginViewModel> logger,
        LocalizationService localization)
    {
        _apiClient = apiClient;
        _navigationService = navigationService;
        _logger = logger;
        _authService = authService;
        Loc = localization;

        OnPropertyChanged(nameof(Loc));

        Loc.LanguageChanged += () =>
        {
            OnPropertyChanged(nameof(Loc));
        };
    }

    [RelayCommand]
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
            await _authService.LoginAsync(response.Token, response.Username, response.UserId);
            _logger.LogInformation("Login successful, navigating to tasks");
            _navigationService.NavigateAndClearBackStack<TaskPage>();
        }
        else
        {
            StatusMessage = "Wrong username or password";
        }
    }

    [RelayCommand]
    private async Task Register(string? password)
    {
        if (string.IsNullOrWhiteSpace(password))
        {
            StatusMessage = "Enter password";
            return;
        }

        _logger.LogDebug("Register attempt for username: {Username}", Username);
        var response = await _apiClient.RegisterAsync(Username, password);

        StatusMessage = response != null
            ? "User has been registered successfully"
            : "User already registered";
    }
}
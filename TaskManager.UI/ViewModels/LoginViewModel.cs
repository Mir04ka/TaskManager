using System.Windows.Input;
using TaskManager.Application.Services;
using TaskManager.UI.Commands;

namespace TaskManager.UI.ViewModels;

public class LoginViewModel : BaseViewModel
{
    private readonly IAuthService _authService;
    private readonly LanguageService _languageService;
    
    public event EventHandler? LoginSucceeded;
    
    private string _username = string.Empty;
    public string Username 
    { 
        get => _username;
        set { _username = value; OnPropertyChanged(); }
    }
    
    private string _statusMessage = string.Empty;
    public string StatusMessage 
    { 
        get => _statusMessage;
        set { _statusMessage = value; OnPropertyChanged(); }
    }
    
    public List<string> AvailableLanguages => _languageService.AvailableLanguages;
    
    public string SelectedLanguage
    {
        get => _languageService.CurrentLanguage;
        set
        {
            _languageService.SetLanguage(value);
            OnPropertyChanged();
        }
    }
    
    public ICommand LoginCommand { get; }
    public ICommand RegisterCommand { get; }
    
    public LoginViewModel(IAuthService authService, LanguageService languageService)
    {
        _authService = authService;
        _languageService = languageService;
        LoginCommand = new RelayCommand(async p => await Login((p as string) ?? ""));
        RegisterCommand = new RelayCommand(async p => await Register((p as string) ?? ""));
    }

    private async Task Login(string password)
    {
        var ok = await _authService.LoginAsync(Username, password);
        StatusMessage = ok ? "Вход выполнен" : "Неверные учетные данные";
        OnPropertyChanged(nameof(StatusMessage));
        
        if (ok) LoginSucceeded?.Invoke(this, EventArgs.Empty);
    }
    
    private async Task Register(string password)
    {
        var ok = await _authService.RegisterAsync(Username, password);
        StatusMessage = ok ? "Пользователь зарегистрирован" : "Пользователь уже существует";
        OnPropertyChanged(nameof(StatusMessage));
    }
}
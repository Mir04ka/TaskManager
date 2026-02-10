namespace TaskManager.UI.ViewModels;

public class SettingsViewModel : BaseViewModel
{
    private readonly LanguageService _langService;

    public List<string> AvailableLanguages { get; }
    private string _selectedLanguage;

    public string SelectedLanguage
    {
        get => _selectedLanguage;
        set
        {
            _selectedLanguage = value;
            _langService.SetLanguage(value);
            OnPropertyChanged();
        }
    }
    
    public SettingsViewModel(LanguageService langService)
    {
        _langService = langService;
        AvailableLanguages = langService.AvailableLanguages;

        _selectedLanguage = AvailableLanguages.Count > 0 ? AvailableLanguages[0] : "en";
        _langService.SetLanguage(_selectedLanguage);
    }
}
using Prism.Mvvm;

namespace TaskManager.UI.ViewModels;

public class LanguageViewModel : BindableBase
{
    private readonly LanguageService _languageService;

    public List<string> AvailableLanguages => _languageService.AvailableLanguages;

    private string _selectedLanguage;
    public string SelectedLanguage
    {
        get => _selectedLanguage;
        set
        {
            if (SetProperty(ref _selectedLanguage, value))
            {
                _languageService.SetLanguage(value);
            }
        }
    }

    public LanguageViewModel(LanguageService languageService)
    {
        _languageService = languageService;
        _selectedLanguage = languageService.AvailableLanguages.FirstOrDefault() ?? "en";
    }
}
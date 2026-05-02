using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml;
using Microsoft.Windows.ApplicationModel.Resources;
using Microsoft.Windows.AppLifecycle;
using System;
using System.Collections.Generic;
using System.Linq;
using Windows.Globalization;
using Windows.Management.Deployment.Preview;

namespace TaskManager.WinUI.Localization;

public class LanguageOption
{
    public string Code { get; }
    public string NameKey { get; }

    public LanguageOption(string code, string nameKey)
    {
        Code = code;
        NameKey = nameKey;
    }

    public override string ToString() => NameKey;
}

public sealed partial class LocalizationService : ObservableObject
{
    public event Action? LanguageChanged;

    private ResourceLoader _loader = new();

    public IReadOnlyList<LanguageOption> AvailableLanguages { get; } = new[]
    {
        new LanguageOption("en-US", "English"),
        new LanguageOption("ru-RU", "Русский"),
    };

    [ObservableProperty]
    private LanguageOption _selectedLanguage;

    public LocalizationService()
    {
        var langCode = ApplicationLanguages.PrimaryLanguageOverride ?? "en-US";

        _selectedLanguage = AvailableLanguages.FirstOrDefault(x => x.Code == langCode)
                                                ?? AvailableLanguages[0];
    }

    partial void OnSelectedLanguageChanged(LanguageOption value)
    {
        if (value == null) return;

        ApplicationLanguages.PrimaryLanguageOverride = value.Code;

        _loader = new ResourceLoader();

        LanguageChanged?.Invoke();
        OnPropertyChanged(nameof(SelectedLanguage));
        OnPropertyChanged("Item[]");
    }

    public string GetString(string key)
    {
        return _loader.GetString(key);
    }

    public string this[string key] => _loader.GetString(key);
}
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;

namespace TaskManager.UI
{
    public class LanguageService : INotifyPropertyChanged
    {
        private readonly Dictionary<string, string> _map = new()
        {
            ["en"] = "Localization/Strings.en.xaml",
            ["ru"] = "Localization/Strings.ru.xaml"
        };

        public List<string> AvailableLanguages => new List<string>(_map.Keys);

        private string _currentLanguage = "en";
        public string CurrentLanguage
        {
            get => _currentLanguage;
            private set
            {
                _currentLanguage = value;
                OnPropertyChanged(nameof(CurrentLanguage));
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void SetLanguage(string code)
        {
            if (!_map.TryGetValue(code, out var source)) return;

            CurrentLanguage = code;

            var app = System.Windows.Application.Current;
            if (app == null) return;

            var newDict = new ResourceDictionary { Source = new Uri(source, UriKind.Relative) };
            var merged = app.Resources.MergedDictionaries;

            // Find and remove old language dictionaries
            var oldDicts = merged
                .Where(d => d.Source != null && d.Source.OriginalString.Contains("Localization/Strings."))
                .ToList();

            foreach (var old in oldDicts)
            {
                merged.Remove(old);
            }

            // Add new dictionary
            merged.Add(newDict);

            // Force refresh all windows
            app.Dispatcher.InvokeAsync(() =>
            {
                foreach (Window window in app.Windows)
                {
                    window.Language = System.Windows.Markup.XmlLanguage.GetLanguage(code);
                    RefreshWindow(window);
                }
            });
        }

        private void RefreshWindow(DependencyObject obj)
        {
            if (obj == null) return;

            // Recursively refresh all child elements
            int childCount = System.Windows.Media.VisualTreeHelper.GetChildrenCount(obj);
            for (int i = 0; i < childCount; i++)
            {
                var child = System.Windows.Media.VisualTreeHelper.GetChild(obj, i);
                
                if (child is System.Windows.FrameworkElement element)
                {
                    element.InvalidateProperty(System.Windows.FrameworkElement.LanguageProperty);
                }
                
                RefreshWindow(child);
            }
        }
    }
}
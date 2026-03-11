using System.Windows;
using Prism.Regions;

namespace TaskManager.UI.Views;

public partial class ShellWindow : MahApps.Metro.Controls.MetroWindow
{
    private readonly IRegionManager _regionManager;
    
    public ShellWindow(IRegionManager regionManager)
    {
        InitializeComponent();
        _regionManager = regionManager;
        
        Loaded += OnLoaded; // delay before nav
    }
    
    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        try
        {
            _regionManager.RequestNavigate("ContentRegion", "LoginView");
            Console.WriteLine("Navigation to LoginView requested");
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Navigation error: {ex.Message}\n{ex.StackTrace}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
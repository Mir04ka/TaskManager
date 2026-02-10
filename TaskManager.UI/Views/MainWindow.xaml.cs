using TaskManager.UI.ViewModels;

namespace TaskManager.UI.Views;

public partial class MainWindow : MahApps.Metro.Controls.MetroWindow
{
    public MainWindow(MainViewModel vm)
    {
        InitializeComponent();
        DataContext = vm;
    }
}
/*
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using TaskManager.WinUI.ViewModels;

namespace TaskManager.WinUI.Views;

public sealed partial class TaskPage : Page
{
    private TaskViewModel ViewModel => (TaskViewModel)DataContext;

    public TaskPage()
    {
        InitializeComponent();
        DataContext = App.Services.GetRequiredService<TaskViewModel>();
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        await ViewModel.LoadAsync();
    }
}
*/
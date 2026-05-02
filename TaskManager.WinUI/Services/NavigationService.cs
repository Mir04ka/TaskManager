using Microsoft.UI.Xaml.Controls;

namespace TaskManager.WinUI.Services;

class NavigationService : INavigationService
{
    private Frame? _frame;

    public void SetFrame(Frame frame) => _frame = frame;

    public void NavigateTo<TPage>() where TPage : Page
    {
        _frame?.Navigate(typeof(TPage));
    }

    public void NavigateAndClearBackStack<TPage>() where TPage : Page
    {
        _frame?.Navigate(typeof(TPage));
        _frame?.BackStack.Clear();
    }
}

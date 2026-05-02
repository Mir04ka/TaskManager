using Microsoft.UI.Xaml.Controls;

namespace TaskManager.WinUI.Services;

public interface INavigationService
{
    void SetFrame(Frame frame);

    void NavigateTo<TPage>() where TPage : Page;
    void NavigateAndClearBackStack<TPage>() where TPage : Page;
}

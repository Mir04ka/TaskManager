using System.Windows.Controls;
using TaskManager.UI.ViewModels;

namespace TaskManager.UI.Views;

public partial class LoginView : UserControl
{
    public LoginView()
    {
        InitializeComponent();
    }
    
    private void LoginButtonClick(object sender, System.Windows.RoutedEventArgs e)
    {
        if (DataContext is LoginViewModel viewModel)
        {
            var password = PwdBox.Password;
            if (viewModel.LoginCommand.CanExecute(password))
            {
                viewModel.LoginCommand.Execute(password);
            }
        }
    }

    private void RegisterButtonClick(object sender, System.Windows.RoutedEventArgs e)
    {
        if (DataContext is LoginViewModel viewModel)
        {
            var password = PwdBox.Password;
            if (viewModel.RegisterCommand.CanExecute(password))
            {
                viewModel.RegisterCommand.Execute(password);
            }
        }
    }
}
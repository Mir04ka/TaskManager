using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using TaskManager.WinUI.ViewModels;

namespace TaskManager.WinUI.Views;

public sealed partial class LoginPage : Page
{
    private LoginViewModel ViewModel => (LoginViewModel)DataContext;

    public LoginPage()
    {
        InitializeComponent();
        DataContext = App.Services.GetRequiredService<LoginViewModel>();
    }

    private void OnLoginClick(object sender, RoutedEventArgs e)
    {
        var password = PwdBox.Password;
        if (ViewModel.LoginCommand.CanExecute(password))
            ViewModel.LoginCommand.Execute(password);
    }

    private void OnRegisterClick(object sender, RoutedEventArgs e)
    {
        var password = PwdBox.Password;
        if (ViewModel.RegisterCommand.CanExecute(password))
            ViewModel.RegisterCommand.Execute(password);
    }
}

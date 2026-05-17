using CommunityToolkit.Mvvm.Input;
using System;
using System.Threading.Tasks;
using TaskManager.WinUI.Localization;
using TaskManager.WinUI.Services;
using TaskManager.WinUI.Services.State;
using TaskManager.WinUI.Views;

namespace TaskManager.WinUI.ViewModels;

public partial class MainViewModel : BaseViewModel
{
    private readonly IAuthService _authService;
    private readonly INavigationService _navigationService;

    public WorkspaceState State { get; }

    public ProcessListViewModel Processes { get; }
    public TaskListViewModel Tasks { get; }

    public LocalizationService Loc { get; }

    public MainViewModel(
                        ProcessListViewModel processes,
                        TaskListViewModel tasks,
                        IAuthService authService,
                        INavigationService navigationService,
                        LocalizationService localization,
                        WorkspaceState state)
    {
        Processes = processes;
        Tasks = tasks;

        _authService = authService;
        _navigationService = navigationService;

        Loc = localization;
        State = state;
    }

    public async Task LoadAsync()
    {
        try
        {
            await Processes.LoadAsync();
            await Tasks.LoadAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine(ex);
        }
    }

    [RelayCommand]
    private void Logout()
    {
        _authService.Logout();
        _navigationService.NavigateAndClearBackStack<LoginPage>();
    }
}

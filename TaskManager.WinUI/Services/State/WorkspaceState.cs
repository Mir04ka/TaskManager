using CommunityToolkit.Mvvm.ComponentModel;

namespace TaskManager.WinUI.Services.State;

public partial class WorkspaceState : ObservableObject
{
    [ObservableProperty]
    private ProcessItemVm? selectedProcess;
}
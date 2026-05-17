using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.Generic;
using TaskManager.WinUI.Models;

namespace TaskManager.WinUI.Services.State;

public partial class WorkspaceState : ObservableObject
{
    [ObservableProperty]
    private ProcessDto? selectedProcess;

    [ObservableProperty]
    private string currentUserRole = string.Empty;

    [ObservableProperty]
    private List<ProcessMemberDto> processMembers = new();

    public bool IsAdmin => currentUserRole.Equals("Admin", System.StringComparison.OrdinalIgnoreCase);
}
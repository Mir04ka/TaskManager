using System;
using System.Threading.Tasks;
using TaskManager.WinUI.Localization;
using TaskManager.WinUI.Services;
using TaskManager.WinUI.Services.State;

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

        //Processes.SelectedProcessChanged += OnProcessChanged;
    }

    public async Task LoadAsync()
    {
        try
        {
            await Processes.LoadAsync();
            await Tasks.LoadAsync(Guid.Empty);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine(ex);
        }
        //try
        //{
        //    var result = await _apiClient.GetTasksAsync(PageNumber, PageSize);
        //    Tasks.Clear();

        //    foreach (var item in result.Items)
        //    {
        //        Tasks.Add(new TaskItemVm(this, _apiClient)
        //        {
        //            Id = item.Id,
        //            Title = item.Title,
        //            Description = item.Description,
        //        });
        //    }

        //    TotalCount = result.TotalCount;

        //    _logger.LogInformation("Loaded page {Page} with {Count} tasks", PageNumber, Tasks.Count);
        //}
        //catch (Exception ex)
        //{
        //    _logger.LogError(ex, "Error loading tasks");
        //}
    }
}

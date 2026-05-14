using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskManager.Domain.Entities;
using TaskManager.WinUI.Localization;
using TaskManager.WinUI.Models;
using TaskManager.WinUI.Services;
using TaskManager.WinUI.Services.State;

namespace TaskManager.WinUI.ViewModels;

public sealed partial class TaskListViewModel : BaseViewModel
{
    private readonly IApiClient _apiClient;
    private readonly ILogger<TaskListViewModel> _logger;
    private readonly WorkspaceState _state;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(TotalPages))]
    [NotifyCanExecuteChangedFor(nameof(NextPageCommand))]
    [NotifyCanExecuteChangedFor(nameof(PrevPageCommand))]
    private int _pageNumber = 1;

    [ObservableProperty]
    private int _pageSize = 10;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(TotalPages))]
    [NotifyCanExecuteChangedFor(nameof(NextPageCommand))]
    private int _totalCount;

    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);

    public ObservableCollection<TaskItemVm> Tasks { get; } = [];

    public TaskListViewModel(
        IApiClient apiClient,
        ILogger<TaskListViewModel> logger,
        WorkspaceState state)
    {
        _apiClient = apiClient;
        _logger = logger;
        _state = state;

        _state.PropertyChanged += OnStateChanged;
    }

    private async void OnStateChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(WorkspaceState.SelectedProcess))
        {
            await LoadAsync(_state.SelectedProcess?.Id);
        }
    }

    public async Task LoadAsync(Guid? processId)
    {
        if (processId == null)
        {
            _logger.LogInformation("No process selected");
            return;
        }
            
        try
        {
            var result = await _apiClient.GetTasksAsync(PageNumber, PageSize);
            Tasks.Clear();

            foreach (var item in result.Items)
            {
                Tasks.Add(new TaskItemVm(this, _apiClient)
                {
                    Id = item.Id,
                    Title = item.Title,
                    Description = item.Description,
                });
            }

            TotalCount = result.TotalCount;

            _logger.LogInformation("Loaded page {Page} with {Count} tasks", PageNumber, Tasks.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading tasks");
        }
    }

    [RelayCommand(CanExecute = nameof(CanGoNext))]
    private async Task NextPage()
    {
        if (PageNumber < TotalPages)
        {
            PageNumber++;
            await LoadAsync(_state.SelectedProcess.Id);
        }
    }
    private bool CanGoNext() => PageNumber < TotalPages;

    [RelayCommand(CanExecute = nameof(CanGoPrev))]
    private async Task PrevPage()
    {
        if (PageNumber > 1)
        {
            PageNumber--;
            await LoadAsync(_state.SelectedProcess.Id);
        }
    }
    private bool CanGoPrev() => PageNumber > 1;
}

public sealed partial class TaskItemVm : ObservableObject
{
    private readonly TaskListViewModel _parent;
    private readonly IApiClient _apiClient;

    private string _originalTitle = string.Empty;
    private string _originalDescription = string.Empty;

    public Guid Id { get; set; }

    private string _title = string.Empty;
    public string Title
    {
        get => _title;
        set => SetProperty(ref _title, value?.Length > 200 ? value.Substring(0, 200) : value ?? string.Empty);
    }

    private string _description = string.Empty;
    public string Description
    {
        get => _description;
        set => SetProperty(ref _description, value?.Length > 1000 ? value.Substring(0, 1000) : value ?? string.Empty);
    }

    [ObservableProperty]
    private bool _isEditing;

    public TaskItemVm(TaskListViewModel parent, IApiClient apiClient)
    {
        _parent = parent;
        _apiClient = apiClient;
    }

    [RelayCommand]
    private void Edit()
    {
        _originalTitle = Title;
        _originalDescription = Description;
        IsEditing = true;
    }

    [RelayCommand(CanExecute = nameof(CanSave))]
    private async Task Save()
    {
        if (string.IsNullOrWhiteSpace(Title)) return;
        try
        {
            await _apiClient.UpdateTaskAsync(Id, new UpdateTaskRequest()
            {
                Id = Id,
                Title = Title,
                Description = Description,
                //Deadline = 
            });
            IsEditing = false;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error saving task: {ex.Message}");
        }
    }
    private bool CanSave() => !string.IsNullOrWhiteSpace(Title);

    [RelayCommand]
    private void Cancel()
    {
        Title = _originalTitle;
        Description = _originalDescription;
        IsEditing = false;
    }

    [RelayCommand]
    private async Task Delete()
    {
        try
        {
            await _apiClient.DeleteTaskAsync(Id);
            //_parent.RemoveTask(this);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error deleting task: {ex.Message}");
        }
    }
}
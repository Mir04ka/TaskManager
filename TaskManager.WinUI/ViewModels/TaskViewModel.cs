using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using TaskManager.WinUI.Localization;
using TaskManager.WinUI.Services;
using TaskManager.WinUI.ViewModels;
using TaskManager.WinUI.Views;

namespace TaskManager.WinUI.ViewModels;

public sealed partial class TaskViewModel : BaseViewModel
{
    private readonly IApiClient _apiClient;
    private readonly ILogger<TaskViewModel> _logger;
    private readonly INavigationService _navigationService;
    private readonly IAuthService _authService;

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

    public LocalizationService Loc { get; }

    public TaskViewModel(
        IApiClient apiClient, 
        IAuthService authService,
        INavigationService navigationService, 
        ILogger<TaskViewModel> logger,
        LocalizationService localization)
    {
        _apiClient = apiClient;
        _authService = authService;
        _navigationService = navigationService;
        _logger = logger;
        Loc = localization;

        OnPropertyChanged(nameof(Loc));

        Loc.LanguageChanged += () =>
        {
            OnPropertyChanged(nameof(Loc));
        };
    }

    public async Task LoadAsync()
    {
        try
        {
            var result  = await _apiClient.GetTasksAsync(PageNumber, PageSize);
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

    [RelayCommand]
    private async Task AddTask()
    {
        try
        {
            var newTask = await _apiClient.CreateTaskAsync("New task", string.Empty);

            if (newTask != null)
            {
                Tasks.Add(new TaskItemVm(this, _apiClient)
                {
                    Id = newTask.Id,
                    Title = newTask.Title,
                    Description = newTask.Description
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding task");
        }
    }

    [RelayCommand]
    private async Task Refresh() => await LoadAsync();

    [RelayCommand]
    private void Logout()
    {
        _authService.Logout();
        _navigationService.NavigateAndClearBackStack<LoginPage>();
    }

    [RelayCommand(CanExecute = nameof(CanGoNext))]
    private async Task NextPage()
    {
        if (PageNumber < TotalPages)
        {
            PageNumber++;
            await LoadAsync();
        }
    }
    private bool CanGoNext() => PageNumber < TotalPages;

    [RelayCommand(CanExecute = nameof(CanGoPrev))]
    private async Task PrevPage()
    {
        if (PageNumber > 1)
        {
            PageNumber--;
            await LoadAsync();
        }
    }
    private bool CanGoPrev() => PageNumber > 1;

    public void RemoveTask(TaskItemVm task) => Tasks.Remove(task);
}

public sealed partial class TaskItemVm : ObservableObject
{
    private readonly TaskViewModel _parent;
    private readonly IApiClient _apiClient;
    public LocalizationService Loc => _parent.Loc;

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

    public TaskItemVm(TaskViewModel parent, IApiClient apiClient)
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
            await _apiClient.UpdateTaskAsync(Id, Title, Description);
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
            _parent.RemoveTask(this);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error deleting task: {ex.Message}");
        }
    }
}
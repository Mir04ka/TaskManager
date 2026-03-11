using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Extensions.Logging;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using TaskManager.UI.Commands;
using TaskManager.UI.Services;

namespace TaskManager.UI.ViewModels;

public class TaskViewModel : BindableBase, INavigationAware
{
    private readonly IApiClient _apiClient;
    private readonly ILogger<TaskViewModel> _logger;
    private readonly LanguageService _languageService;

    public ObservableCollection<TaskItemVm> Tasks { get; } = new ObservableCollection<TaskItemVm>();

    public ICommand AddTaskCommand { get; }
    public ICommand RefreshCommand { get; }
    
    public LanguageViewModel LanguageVM { get; }

    public TaskViewModel(IApiClient apiClient, ILogger<TaskViewModel> logger, LanguageViewModel languageViewModel)
    {
        _apiClient = apiClient;
        _logger = logger;
        LanguageVM = languageViewModel;

        AddTaskCommand = new DelegateCommand(async () => await AddTaskAsync());
        RefreshCommand = new DelegateCommand(async () => await LoadAsync());
    }

    public async Task LoadAsync()
    {
        try
        {
            var items = await _apiClient.GetTasksAsync();
            Tasks.Clear();

            foreach (var item in items)
            {
                var vm = new TaskItemVm(this, _apiClient)
                {
                    Id = item.Id,
                    Title = item.Title,
                    Description = item.Description
                };
                Tasks.Add(vm);
            }

            _logger.LogInformation("Loaded {Count} tasks", items.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading tasks");
        }
    }

    private async Task AddTaskAsync()
    {
        try
        {
            var newTask = await _apiClient.CreateTaskAsync("New task", string.Empty);

            if (newTask != null)
            {
                var vm = new TaskItemVm(this, _apiClient)
                {
                    Id = newTask.Id,
                    Title = newTask.Title,
                    Description = newTask.Description
                };
                Tasks.Add(vm);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding task");
        }
    }

    public void RemoveTask(TaskItemVm task)
    {
        Tasks.Remove(task);
    }

    public void OnNavigatedTo(NavigationContext navigationContext)
    {
        _ = LoadAsync();
    }

    public bool IsNavigationTarget(NavigationContext navigationContext) => true;
    public void OnNavigatedFrom(NavigationContext navigationContext) { }
}

public class TaskItemVm : BindableBase
{
    private readonly TaskViewModel _parent;
    private readonly IApiClient _apiClient;

    public Guid Id { get; set; }

    private string _title = string.Empty;
    public string Title
    {
        get => _title;
        set => SetProperty(ref _title, value?.Length > 200 ? value.Substring(0, 200) : value ?? string.Empty);
    }

    private string _description = "";
    public string Description
    {
        get => _description;
        set => SetProperty(ref _description, value?.Length > 1000 ? value.Substring(0, 1000) : value ?? string.Empty);
    }

    private bool _isEditing;
    public bool IsEditing
    {
        get => _isEditing;
        set => SetProperty(ref _isEditing, value);
    }

    private string _originalTitle = string.Empty;
    private string _originalDescription = string.Empty;

    public ICommand EditCommand { get; }
    public ICommand SaveCommand { get; }
    public ICommand CancelCommand { get; }
    public ICommand DeleteCommand { get; }

    public TaskItemVm(TaskViewModel parent, IApiClient apiClient)
    {
        _parent = parent;
        _apiClient = apiClient;

        EditCommand = new RelayCommand(_ => StartEdit());
        SaveCommand = new RelayCommand(async _ => await SaveAsync(), _ => CanSave());
        CancelCommand = new RelayCommand(_ => CancelEdit());
        DeleteCommand = new RelayCommand(async _ => await DeleteAsync());
    }

    private void StartEdit()
    {
        _originalTitle = Title;
        _originalDescription = Description;
        IsEditing = true;
    }

    private void CancelEdit()
    {
        Title = _originalTitle;
        Description = _originalDescription;
        IsEditing = false;
    }

    private bool CanSave()
    {
        return !string.IsNullOrWhiteSpace(Title);
    }

    private async Task SaveAsync()
    {
        if (string.IsNullOrWhiteSpace(Title))
            return;

        try
        {
            await _apiClient.UpdateTaskAsync(Id, Title, Description);
            IsEditing = false;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving task: {ex.Message}");
        }
    }

    private async Task DeleteAsync()
    {
        try
        {
            await _apiClient.DeleteTaskAsync(Id);
            _parent.RemoveTask(this);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error deleting task: {ex.Message}");
        }
    }
}
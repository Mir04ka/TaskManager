using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.ObjectModel;
using System.Data;
using System.Threading.Tasks;
using TaskManager.WinUI.Localization;
using TaskManager.WinUI.Models;
using TaskManager.WinUI.Services;
using TaskManager.WinUI.Services.State;
using TaskManager.WinUI.ViewModels;

public sealed partial class ProcessListViewModel : BaseViewModel
{
    private readonly IApiClient _apiClient;
    private readonly ILogger<ProcessListViewModel> _logger;
    private readonly WorkspaceState _state;

    public LocalizationService Loc { get; }

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

    [ObservableProperty]
    private ProcessItemVm? _selectedProcess;

    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);

    public ObservableCollection<ProcessItemVm> Processes { get; } = [];

    public ProcessListViewModel(
        IApiClient apiClient,
        ILogger<ProcessListViewModel> logger,
        WorkspaceState state,
        LocalizationService loc)
    {
        _apiClient = apiClient;
        _logger = logger;
        _state = state;
        Loc = loc;
    }

    public async Task LoadAsync()
    {
        try
        {
            var result = await _apiClient.GetMyProcessesAsync(PageNumber, PageSize);
            Processes.Clear();

            foreach (var item in result.Items)
            {
                Processes.Add(new ProcessItemVm(this, _apiClient, Loc)
                {
                    Id = item.Id,
                    Name = item.Name,
                    Role = item.Role,
                });
            }

            TotalCount = result.TotalCount;

            _logger.LogInformation("Loaded processes page {Page} with {Count} processes", PageNumber, Processes.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading processes");
        }
    }

    partial void OnSelectedProcessChanged(ProcessItemVm? value)
    {
        if (value != null)
        {
            _state.SelectedProcess = new ProcessDto
            {
                Id = value.Id,
                Name = value.Name,
                Role = value.Role
            };
            _state.CurrentUserRole = value.Role;
        }
        else
        {
            _state.SelectedProcess = null;
            _state.CurrentUserRole = string.Empty;
        }
    }

    [RelayCommand]
    private async Task AddProcess()
    {
        try
        {
            var newProcess = await _apiClient.CreateProcessAsync(new CreateProcessRequest
            {
                Name = "New process"
            });

            if (newProcess != null)
            {
                Processes.Add(new ProcessItemVm(this, _apiClient, Loc)
                {
                    Id = newProcess.Id,
                    Name = newProcess.Name,
                    Role = newProcess.Role
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding process");
        }
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
}

public sealed partial class ProcessItemVm : ObservableObject
{
    private readonly ProcessListViewModel _parent;
    private readonly IApiClient _apiClient;

    public LocalizationService Loc { get; }

    public Guid Id { get; set; }

    private string _name = string.Empty;
    public string Name
    {
        get => _name;
        set => SetProperty(ref _name, value?.Length > 200 ? value.Substring(0, 200) : value ?? string.Empty);
    }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsAdmin))]
    private string _role = string.Empty;

    public bool IsAdmin => Role.Equals("Admin", StringComparison.OrdinalIgnoreCase);

    [ObservableProperty]
    private bool _isRenaming;

    [ObservableProperty]
    private string _editName = string.Empty;

    public ProcessItemVm(ProcessListViewModel parent, IApiClient apiClient, LocalizationService loc)
    {
        _parent = parent;
        _apiClient = apiClient;
        Loc = loc;
    }

    [RelayCommand]
    private void BeginRename()
    {
        EditName = Name;
        IsRenaming = true;
    }

    [RelayCommand]
    private async Task SaveRename()
    {
        if (string.IsNullOrWhiteSpace(EditName)) return;

        try
        {
            await _apiClient.RenameProcessAsync(Id, EditName);
            Name = EditName;
            IsRenaming = false;
        }
        catch
        { }
    }

    [RelayCommand]
    private void CancelRename()
    {
        IsRenaming = false;
    }
}
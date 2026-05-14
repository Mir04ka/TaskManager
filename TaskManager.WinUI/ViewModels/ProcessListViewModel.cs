using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using TaskManager.WinUI.Services;
using TaskManager.WinUI.Services.State;
using TaskManager.WinUI.ViewModels;

public sealed partial class ProcessListViewModel : BaseViewModel
{
    private readonly IApiClient _apiClient;
    private readonly ILogger<ProcessListViewModel> _logger;
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

    [ObservableProperty]
    private ProcessItemVm? _selectedProcess;
    public event Action<ProcessItemVm?>? SelectedProcessChanged;

    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);

    public ObservableCollection<ProcessItemVm> Processes { get; } = [];

    public ProcessListViewModel(
        IApiClient apiClient,
        ILogger<ProcessListViewModel> logger,
        WorkspaceState state)
    {
        _apiClient = apiClient;
        _logger = logger;
        _state = state;
    }

    public async Task LoadAsync()
    {
        throw new NotImplementedException();
    }

    partial void OnSelectedProcessChanged(ProcessItemVm? value)
    {
        _state.SelectedProcess = value;
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

    private string _originalTitle = string.Empty;
    private string _originalDescription = string.Empty;

    public Guid Id { get; set; }

    private string _name = string.Empty;
    public string Name
    {
        get => _name;
        set => SetProperty(ref _name, value?.Length > 200 ? value.Substring(0, 200) : value ?? string.Empty);
    }

    [ObservableProperty]
    private bool _isAdmin;

    public ProcessItemVm(ProcessListViewModel parent, IApiClient apiClient)
    {
        _parent = parent;
        _apiClient = apiClient;
    }
}
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

    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private string _selectedStatusFilter = "All";

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string _newMemberUsername = string.Empty;

    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    public bool IsAdmin => _state.IsAdmin;
    public WorkspaceState State => _state;

    public ObservableCollection<TaskItemVm> Tasks { get; } = [];
    public ObservableCollection<ProcessMemberDto> ProcessMembers { get; } = [];
    public ObservableCollection<TagDto> AvailableTags { get; } = [];
    public List<string> StatusFilters { get; } = new() { "All", "Todo", "InProgress", "Done" };

    [ObservableProperty]
    private TaskItemVm? _selectedTask; // Selected for details

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
            PageNumber = 1;
            SelectedTask = null;
            AddTaskCommand.NotifyCanExecuteChanged();
            await LoadProcessDataAsync();
            await LoadAsync();
        }
        if (e.PropertyName == nameof(WorkspaceState.IsAdmin))
        {
            OnPropertyChanged(nameof(IsAdmin));
            AddTaskCommand.NotifyCanExecuteChanged();
            await LoadProcessDataAsync();
            await LoadAsync();
        }
    }

    private async Task LoadProcessDataAsync()
    {
        var proc = _state.SelectedProcess;
        if (proc == null) return;

        var members = await _apiClient.GetProcessMembersAsync(proc.Id);
        ProcessMembers.Clear();

        foreach (var m in members) ProcessMembers.Add(m);
        
        _state.ProcessMembers = members;

        var tags = await _apiClient.GetTagsAsync(proc.Id);
        AvailableTags.Clear();

        foreach (var t in tags) AvailableTags.Add(t);
    }

    public async Task LoadAsync()
    {
        var proc = _state.SelectedProcess;
        if (proc == null) 
        { 
            Tasks.Clear(); 
            return; 
        }

        try
        {
            IsLoading = true;
            var result = await _apiClient.GetProcessTasksAsync(proc.Id, PageNumber, PageSize);
            Tasks.Clear();

            var filtered = result.Items.AsEnumerable();
            if (!string.IsNullOrWhiteSpace(SearchText))
                filtered = filtered.Where(t =>
                    t.Title.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                    t.Description.Contains(SearchText, StringComparison.OrdinalIgnoreCase));
            if (SelectedStatusFilter != "All")
                filtered = filtered.Where(t => t.Status == SelectedStatusFilter);

            foreach (var item in filtered)
            {
                Tasks.Add(MapToVm(item));
            }
            TotalCount = result.TotalCount;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading tasks");
        }
        finally 
        { 
            IsLoading = false; 
        }
    }

    private TaskItemVm MapToVm(TaskItemDto item) => new TaskItemVm(this, _apiClient)
    {
        Id = item.Id,
        Title = item.Title,
        Description = item.Description,
        Status = item.Status,
        Deadline = item.Deadline,
        AssignedToUserId = item.AssignedToUserId,
        AssignedToUsername = item.AssignedToUsername,
        Tags = new ObservableCollection<TagDto>(item.Tags),
        Remarks = new ObservableCollection<RemarkDto>(item.Remarks)
    };

    private bool CanAddTask() => IsAdmin && _state.SelectedProcess != null;

    [RelayCommand(CanExecute = nameof(CanAddTask))]
    private async Task AddTask()
    {
        var proc = _state.SelectedProcess;
        if (proc == null || !IsAdmin) return;
        try
        {
            var created = await _apiClient.CreateTaskAsync(new CreateTaskRequest
            {
                Title = "New task",
                Description = string.Empty,
                ProcessId = proc.Id,
                Deadline = DateTime.UtcNow.AddDays(30)
            });
            if (created != null)
            {
                var vm = MapToVm(created);
                Tasks.Insert(0, vm);
                SelectedTask = vm;
            }
        }
        catch (Exception ex) { _logger.LogError(ex, "Error adding task"); }
    }

    [RelayCommand]
    private async Task AddMember()
    {
        if (string.IsNullOrWhiteSpace(NewMemberUsername)) return;
        var proc = _state.SelectedProcess;
        if (proc == null) return;

        try
        {
            await _apiClient.AddUserToProcessAsync(proc.Id, NewMemberUsername);
            NewMemberUsername = string.Empty;
            await LoadProcessDataAsync();
        }
        catch (Exception ex) 
        { 
            _logger.LogError(ex, "Error adding member"); 
        }
    }

    [RelayCommand]
    private async Task Refresh() => await LoadAsync();

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

    public void RemoveTask(TaskItemVm task)
    {
        Tasks.Remove(task);
        if (SelectedTask == task) SelectedTask = null;
    }

    partial void OnSelectedTaskChanged(TaskItemVm? value)
    {
        if (value != null)
        {
            value.UpdateOriginals();
            value.UpdateSelectedAssignee();
            _ = LoadTaskRemarksAsync(value);
        }
    }

    private async Task LoadTaskRemarksAsync(TaskItemVm vm)
    {
        try
        {
            var remarks = await _apiClient.GetRemarksAsync(vm.Id);
            vm.Remarks.Clear();
            foreach (var r in remarks) vm.Remarks.Add(r);
        }
        catch (Exception ex) { _logger.LogError(ex, "Error loading remarks"); }
    }
    partial void OnSearchTextChanged(string value) { _ = LoadAsync(); }
    partial void OnSelectedStatusFilterChanged(string value) { _ = LoadAsync(); }
}

public sealed partial class TaskItemVm : ObservableObject
{
    private readonly TaskListViewModel _parent;
    private readonly IApiClient _apiClient;

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
    private string _status = "Todo";
    [ObservableProperty] 
    private DateTime? _deadline;
    [ObservableProperty] 
    private Guid? _assignedToUserId;
    [ObservableProperty] 
    private string _assignedToUsername = string.Empty;
    [ObservableProperty]
    private bool _isEditing;
    [ObservableProperty] 
    private string _newRemarkText = string.Empty;
    [ObservableProperty] 
    private bool _isAssigning;
    [ObservableProperty] 
    private ProcessMemberDto? _selectedAssignee;
    [ObservableProperty]
    private string _newTagName = string.Empty;

    public ObservableCollection<TagDto> Tags { get; set; } = [];
    public ObservableCollection<RemarkDto> Remarks { get; set; } = [];

    private string _originalTitle = string.Empty;
    private string _originalDescription = string.Empty;
    private string _originalStatus = "Todo";
    private DateTime? _originalDeadline;

    public bool IsAdmin => _parent.IsAdmin;
    public ObservableCollection<ProcessMemberDto> ProcessMembers => _parent.ProcessMembers;
    public ObservableCollection<TagDto> AvailableTags => _parent.AvailableTags;

    public string DeadlineDisplay => Deadline.HasValue
        ? Deadline.Value.ToString("dd.MM.yyyy")
        : "—";

    public string StatusDisplay => Status switch
    {
        "Todo" => "К выполнению",
        "InProgress" => "В работе",
        "Done" => "Завершено",
        _ => Status
    };

    public TaskItemVm(TaskListViewModel parent, IApiClient apiClient)
    {
        _parent = parent;
        _apiClient = apiClient;
    }

    public void UpdateOriginals()
    {
        _originalTitle = Title;
        _originalDescription = Description;
        _originalStatus = Status;
        _originalDeadline = Deadline;
    }

    public void UpdateSelectedAssignee()
    {
        SelectedAssignee = ProcessMembers.FirstOrDefault(m => m.UserId == AssignedToUserId);
    }

    [RelayCommand]
    private void BeginEdit()
    {
        UpdateOriginals();
        IsEditing = true;
    }

    [RelayCommand(CanExecute = nameof(CanSave))]
    private async Task Save()
    {
        if (string.IsNullOrWhiteSpace(Title)) return;
        try
        {
            await _apiClient.UpdateTaskAsync(Id, new UpdateTaskRequest
            {
                Id = Id,
                Title = Title,
                Description = Description,
                Status = Status,
                Deadline = Deadline
            });
            IsEditing = false;
            OnPropertyChanged(nameof(DeadlineDisplay));
            OnPropertyChanged(nameof(StatusDisplay));
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error saving task: {ex.Message}");
        }
    }
    private bool CanSave() => !string.IsNullOrWhiteSpace(Title);

    [RelayCommand]
    private void CancelEdit()
    {
        Title = _originalTitle;
        Description = _originalDescription;
        Status = _originalStatus;
        Deadline = _originalDeadline;
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

    [RelayCommand]
    private async Task ChangeStatus(string status)
    {
        try
        {
            await _apiClient.ChangeStatusAsync(Id, status);
            Status = status;
            OnPropertyChanged(nameof(StatusDisplay));
        }
        catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"ChangeStatus error: {ex.Message}"); }
    }

    [RelayCommand]
    private async Task AssignTo(ProcessMemberDto? member)
    {
        if (member == null) return;
        try
        {
            await _apiClient.AssignTaskAsync(Id, new AssignTaskRequest { UserId = member.UserId });
            AssignedToUserId = member.UserId;
            AssignedToUsername = member.Username;
            IsAssigning = false;
            OnPropertyChanged(nameof(AssignedToUsername));
        }
        catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"Assign error: {ex.Message}"); }
    }

    [RelayCommand]
    private async Task AddRemark()
    {
        if (string.IsNullOrWhiteSpace(NewRemarkText)) return;
        try
        {
            await _apiClient.AddRemarkAsync(Id, new AddRemarkRequest { Text = NewRemarkText });
            NewRemarkText = string.Empty;
            var remarks = await _apiClient.GetRemarksAsync(Id);
            Remarks.Clear();
            foreach (var r in remarks) Remarks.Add(r);
        }
        catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"AddRemark error: {ex.Message}"); }
    }

    [RelayCommand]
    private async Task DeleteRemark(RemarkDto? remark)
    {
        if (remark == null) return;
        try
        {
            await _apiClient.DeleteRemarkAsync(Id, remark.Id);
            Remarks.Remove(remark);
        }
        catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"DeleteRemark error: {ex.Message}"); }
    }

    [RelayCommand]
    private async Task AddTag(TagDto? tag)
    {
        if (tag == null || Tags.Any(t => t.Id == tag.Id)) return;
        try
        {
            await _apiClient.AddTagToTaskAsync(Id, tag.Id);
            Tags.Add(tag);
        }
        catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"AddTag error: {ex.Message}"); }
    }

    [RelayCommand]
    private async Task RemoveTag(TagDto? tag)
    {
        if (tag == null) return;
        try
        {
            await _apiClient.RemoveTagFromTaskAsync(Id, tag.Id);
            Tags.Remove(tag);
        }
        catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"RemoveTag error: {ex.Message}"); }
    }

    [RelayCommand]
    private async Task CreateTag()
    {
        if (string.IsNullOrWhiteSpace(NewTagName)) return;
        try
        {
            var procId = _parent.State.SelectedProcess!.Id;
            var tag = await _apiClient.CreateTagAsync(procId, NewTagName);
            if (tag != null)
            {
                AvailableTags.Add(tag);
                NewTagName = string.Empty;
            }
        }
        catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"CreateTag error: {ex.Message}"); }
    }
}
using System.Collections.ObjectModel;
using System.Windows.Input;
using TaskManager.Application.Services;
using TaskManager.UI.Commands;
using TaskManager.Domain.Entities;

namespace TaskManager.UI.ViewModels
{
    public class TaskViewModel : BaseViewModel
    {
        private readonly ITaskService _taskService;
        private readonly LanguageService _languageService;

        public ObservableCollection<TaskItemVm> Tasks { get; } = new ObservableCollection<TaskItemVm>();
        public ICommand AddTaskCommand { get; }
        public ICommand RefreshCommand { get; }

        public TaskViewModel(ITaskService taskService, LanguageService languageService)
        {
            _taskService = taskService;
            _languageService = languageService;
            AddTaskCommand = new RelayCommand(async _ => await AddTaskAsync());
            RefreshCommand = new RelayCommand(async _ => await LoadAsync());
        }
        
        public List<string> AvailableLanguages => _languageService.AvailableLanguages;
    
        public string SelectedLanguage
        {
            get => _languageService.CurrentLanguage;
            set
            {
                _languageService.SetLanguage(value);
                OnPropertyChanged();
            }
        }

        public async Task LoadAsync()
        {
            var items = await _taskService.GetCurrentUserTasksAsync();
            Tasks.Clear();
            foreach (var it in items)
            {
                var vm = new TaskItemVm(this)
                {
                    Id = it.Id,
                    Title = it.Title,
                    Description = it.Description
                };
                Tasks.Add(vm);
            }
        }

        private async Task AddTaskAsync()
        {
            var item = new TaskItem
            {
                Title = "New task",
                Description = ""
            };
            await _taskService.AddAsync(item);
            
            var vm = new TaskItemVm(this)
            {
                Id = item.Id,
                Title = item.Title,
                Description = item.Description
            };
            Tasks.Add(vm);
        }

        public async Task DeleteTaskAsync(TaskItemVm taskVm)
        {
            await _taskService.DeleteAsync(taskVm.Id);
            Tasks.Remove(taskVm);
        }

        public async Task UpdateTaskAsync(TaskItemVm taskVm)
        {
            var item = new TaskItem
            {
                Id = taskVm.Id,
                Title = taskVm.Title,
                Description = taskVm.Description,
                UserId = Guid.Empty 
            };
            await _taskService.UpdateAsync(item);
        }
    }

    public class TaskItemVm : BaseViewModel
    {
        private readonly TaskViewModel _parent;
        
        public Guid Id { get; set; }
        
        private string _title = "";
        public string Title
        {
            get => _title;
            set { 
                _title = value?.Length > 200 ? value.Substring(0, 200) : value ?? "";
                OnPropertyChanged(); 
            }
        }

        private string _description = "";
        public string Description
        {
            get => _description;
            set
            {
                _description = value?.Length > 1000 ? value.Substring(0, 1000) : value ?? ""; 
                OnPropertyChanged();
            }
        }

        private bool _isEditing;
        public bool IsEditing
        {
            get => _isEditing;
            set { _isEditing = value; OnPropertyChanged(); }
        }
        
        private string _originalTitle = "";
        private string _originalDescription = "";

        public ICommand EditCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }
        public ICommand DeleteCommand { get; }

        public TaskItemVm(TaskViewModel parent)
        {
            _parent = parent;
            EditCommand = new RelayCommand(_ => StartEdit());
            SaveCommand = new RelayCommand(async _ => await SaveAsync());
            CancelCommand = new RelayCommand(_ => CancelEdit());
            DeleteCommand = new RelayCommand(async _ => await _parent.DeleteTaskAsync(this));
        }

        private void StartEdit()
        {
            if (IsEditing)
            {
                CancelEdit();
            }
            else
            {
                _originalTitle = Title;
                _originalDescription = Description;
                IsEditing = true;
            }
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
            {
                return;
            }
            
            await _parent.UpdateTaskAsync(this);
            IsEditing = false;
        }
    }
}
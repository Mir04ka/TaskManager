using System.Windows.Input;
using TaskManager.UI.Commands;

namespace TaskManager.UI.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        public LoginViewModel LoginVM { get; }
        public TaskViewModel TaskVM { get; }
        public SettingsViewModel SettingsVM { get; }

        private BaseViewModel _currentViewModel;
        public BaseViewModel CurrentViewModel
        {
            get => _currentViewModel;
            set { _currentViewModel = value; OnPropertyChanged(); }
        }

        public ICommand ShowSettingsCommand { get; }
        public ICommand ShowTasksCommand { get; }

        public MainViewModel(LoginViewModel loginVm, TaskViewModel taskVm, SettingsViewModel settingsVm)
        {
            LoginVM = loginVm;
            TaskVM = taskVm;
            SettingsVM = settingsVm;

            ShowSettingsCommand = new RelayCommand(_ => CurrentViewModel = SettingsVM);
            ShowTasksCommand = new RelayCommand(_ => CurrentViewModel = TaskVM);

            LoginVM.LoginSucceeded += OnLoginSucceeded;
            CurrentViewModel = LoginVM;
        }

        private async void OnLoginSucceeded(object? sender, EventArgs e)
        {
            // Load tasks after successful login
            await TaskVM.LoadAsync();
            CurrentViewModel = TaskVM;
        }
    }
}
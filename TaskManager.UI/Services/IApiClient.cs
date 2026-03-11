using TaskManager.UI.Models;

namespace TaskManager.UI.Services;

public interface IApiClient
{
    Task<AuthResponse?> LoginAsync(string username, string password);
    Task<AuthResponse?> RegisterAsync(string username, string password);
    Task<List<TaskItemDto>> GetTasksAsync();
    Task<TaskItemDto?> CreateTaskAsync(string title, string description);
    Task UpdateTaskAsync(Guid id, string title, string description);
    Task DeleteTaskAsync(Guid id);
    void SetToken(string token);
}
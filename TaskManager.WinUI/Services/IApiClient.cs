using System;
using System.Threading.Tasks;
using TaskManager.AppCore.Common;
using TaskManager.WinUI.Models;

namespace TaskManager.WinUI.Services;

public interface IApiClient
{
    Task<AuthResponse?> LoginAsync(string username, string password);
    Task<AuthResponse?> RegisterAsync(string username, string password);
    Task<PagedResult<TaskItemDto>> GetTasksAsync(int pageNumber, int pageSize);
    Task<TaskItemDto?> CreateTaskAsync(string title, string description);
    Task UpdateTaskAsync(Guid id, string title, string description);
    Task DeleteTaskAsync(Guid id);
    void SetToken(string token);
    Task<bool> ValidateTokenAsync();
}
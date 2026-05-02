using RestEase;
using System;
using System.Threading.Tasks;
using TaskManager.AppCore.Common;
using TaskManager.WinUI.Models;

namespace TaskManager.WinUI.Services;

public interface ITasksApi
{
    [Post("auth/login")]
    Task<AuthResponse> LoginAsync([Body] LoginRequest request);

    [Post("auth/register")]
    Task<AuthResponse> RegisterAsync([Body] RegisterRequest request);

    [Get("tasks")]
    Task<PagedResult<TaskItemDto>> GetTasksAsync([Query] int pageNumber, [Query] int pageSize);

    [Post("tasks")]
    Task<TaskItemDto> CreateTaskAsync([Body] CreateTaskRequest request);

    [Put("tasks/{id}")]
    Task UpdateTaskAsync([Path("id")] Guid id, [Body] UpdateTaskRequest request);

    [Delete("tasks/{id}")]
    Task DeleteTaskAsync([Path("id")] Guid id);
}

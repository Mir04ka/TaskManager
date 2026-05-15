using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TaskManager.AppCore.Common;
using TaskManager.WinUI.Models;

namespace TaskManager.WinUI.Services;

public interface IApiClient
{
    Task<AuthResponse?> LoginAsync(string username, string password);
    Task<AuthResponse?> RegisterAsync(string username, string password);
    void SetToken(string token);
    Task<bool> ValidateTokenAsync();

    Task<PagedResult<TaskItemDto>> GetTasksAsync(int pageNumber, int pageSize);
    Task<TaskItemDto?> GetTaskByIdAsync(Guid id);
    Task<TaskItemDto?> CreateTaskAsync(CreateTaskRequest request);
    Task UpdateTaskAsync(Guid taskId, UpdateTaskRequest request);
    Task DeleteTaskAsync(Guid id);

    Task AssignTaskAsync(Guid taskId, AssignTaskRequest request);
    Task AddRemarkAsync(Guid taskId, AddRemarkRequest request);
    Task<List<RemarkDto>> GetRemarksAsync(Guid taskId);
    Task AddTagToTaskAsync(Guid taskId, Guid tagId);
    Task RemoveTagFromTaskAsync(Guid taskId, Guid tagId);

    Task<PagedResult<ProcessDto>> GetMyProcessesAsync(int pageNumber, int pageSize);
    Task<ProcessDto?> CreateProcessAsync(CreateProcessRequest request);
}
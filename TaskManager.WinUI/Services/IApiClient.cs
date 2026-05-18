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

    // Tasks
    Task<PagedResult<TaskItemDto>> GetMyTasksAsync(int pageNumber, int pageSize);
    Task<PagedResult<TaskItemDto>> GetProcessTasksAsync(Guid processId, int pageNumber, int pageSize);
    Task<TaskItemDto?> GetTaskByIdAsync(Guid id);
    Task<TaskItemDto?> CreateTaskAsync(CreateTaskRequest request);
    Task UpdateTaskAsync(Guid taskId, UpdateTaskRequest request);
    Task DeleteTaskAsync(Guid id);
    Task AssignTaskAsync(Guid taskId, AssignTaskRequest request);
    Task ChangeStatusAsync(Guid taskId, string status);

    // Remarks
    Task AddRemarkAsync(Guid taskId, AddRemarkRequest request);
    Task<List<RemarkDto>> GetRemarksAsync(Guid taskId);
    Task DeleteRemarkAsync(Guid taskId, Guid remarkId);

    // Tags
    Task AddTagToTaskAsync(Guid taskId, Guid tagId);
    Task RemoveTagFromTaskAsync(Guid taskId, Guid tagId);
    Task<List<TagDto>> GetTagsAsync(Guid processId);
    Task<TagDto?> CreateTagAsync(Guid processId, string name);
    Task DeleteTagAsync(Guid processId, Guid tagId);

    // Processes
    Task<PagedResult<ProcessDto>> GetMyProcessesAsync(int pageNumber, int pageSize);
    Task<ProcessDto?> CreateProcessAsync(CreateProcessRequest request);
    Task RenameProcessAsync(Guid processId, string name);
    Task DeleteProcessAsync(Guid processId);
    Task<string?> GetMyRoleAsync(Guid processId);
    Task<List<ProcessMemberDto>> GetProcessMembersAsync(Guid processId);
    Task AddUserToProcessAsync(Guid processId, string username);
}
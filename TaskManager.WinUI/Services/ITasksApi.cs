using RestEase;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TaskManager.AppCore.Common;
using TaskManager.WinUI.Models;

namespace TaskManager.WinUI.Services;

public interface ITasksApi
{
    // auth
    [Post("auth/login")]
    Task<AuthResponse> LoginAsync([Body] LoginRequest request);

    [Post("auth/register")]
    Task<AuthResponse> RegisterAsync([Body] RegisterRequest request);

    // tasks
    [Get("tasks/my")]
    Task<PagedResult<TaskItemDto>> GetMyTasksAsync([Query] int pageNumber, [Query] int pageSize);

    [Get("tasks/my")]
    Task<PagedResult<TaskItemDto>> GetTasksAsync([Query] int pageNumber, [Query] int pageSize); // ValidateTokenAsync

    [Get("tasks/process/{processId}")]
    Task<PagedResult<TaskItemDto>> GetProcessTasksAsync(
        [Path] Guid processId,
        [Query] int pageNumber, 
        [Query] int pageSize);

    [Get("tasks/{id}")]
    Task<TaskItemDto> GetTaskByIdAsync([Path] Guid id);

    [Post("tasks")]
    Task<TaskItemDto> CreateTaskAsync([Body] CreateTaskRequest request);

    [Put("tasks/{id}")]
    Task UpdateTaskAsync([Path] Guid id, [Body] UpdateTaskRequest request);

    [Delete("tasks/{id}")]
    Task DeleteTaskAsync([Path] Guid id);

    [Post("tasks/{id}/assign")]
    Task AssignTaskAsync([Path] Guid id, [Body] AssignTaskRequest request);

    [Patch("tasks/{id}/status")]
    Task ChangeStatusAsync([Path] Guid id, [Body] ChangeStatusRequest request);

    // task tags
    [Post("tasks/{id}/tags")]
    Task AddTagToTaskAsync([Path] Guid id, [Body] TaskTagRequest request);

    [Delete("tasks/{id}/tags/{tagId}")]
    Task RemoveTagFromTaskAsync([Path] Guid id, [Path] Guid tagId);

    // remarks
    [Get("tasks/{taskId}/remarks")]
    Task<List<RemarkDto>> GetRemarksAsync([Path] Guid taskId);

    [Post("tasks/{taskId}/remarks")]
    Task AddRemarkAsync([Path] Guid taskId, [Body] AddRemarkRequest request);

    [Delete("tasks/{taskId}/remarks/{remarkId}")]
    Task DeleteRemarkAsync([Path] Guid taskId, [Path] Guid remarkId);

    // tags
    [Get("processes/{processId}/tags")]
    Task<List<TagDto>> GetTagsAsync([Path] Guid processId);

    [Post("processes/{processId}/tags")]
    Task<TagDto> CreateTagAsync([Path] Guid processId, [Body] CreateTagRequest request);

    [Delete("processes/{processId}/tags/{tagId}")]
    Task DeleteTagAsync([Path] Guid processId, [Path] Guid tagId);

    // process
    [Get("Process/my")]
    Task<PagedResult<ProcessDto>> GetMyProcessesAsync([Query] int pageNumber, [Query] int pageSize);

    [Post("Process")]
    Task<ProcessDto> CreateProcessAsync([Body] CreateProcessRequest request);

    [Get("Process/{processId}/role")]
    Task<string> GetMyRoleAsync([Path] Guid processId);

    [Get("Process/{processId}/members")]
    Task<List<ProcessMemberDto>> GetProcessMembersAsync([Path] Guid processId);
}

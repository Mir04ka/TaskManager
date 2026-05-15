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
    Task<PagedResult<TaskItemDto>> GetTasksAsync([Query] int pageNumber, [Query] int pageSize);

    [Get("tasks/{id}")]
    Task<TaskItemDto> GetTaskByIdAsync([Path] Guid id);

    [Post("tasks")]
    Task<TaskItemDto> CreateTaskAsync([Body] CreateTaskRequest request);

    [Put("tasks/{id}")]
    Task UpdateTaskAsync([Path] Guid id, [Body] UpdateTaskRequest request);

    [Delete("tasks/{id}")]
    Task DeleteTaskAsync([Path] Guid id);

    // task ops
    [Post("tasks/{id}/assign")]
    Task AssignTaskAsync([Path] Guid id, [Body] AssignTaskRequest request);

    [Post("tasks/{id}/remarks")]
    Task AddRemarkAsync([Path] Guid id, [Body] AddRemarkRequest request);

    [Get("tasks/{taskId}/remarks")]
    Task<List<RemarkDto>> GetRemarkAsync([Path] Guid taskId);

    [Post("tasks/{taskId}/tags/{tagId}")]
    Task AddTagToTaskAsync([Path] Guid taskId, [Path] Guid tagId);

    [Delete("tasks/{taskId}/tags/{tagId}")]
    Task RemoveTagFromTaskAsync([Path] Guid taskId, [Path] Guid tagId);

    // tags
    [Get("process/{processId}/tags")]
    Task<List<TagDto>> GetTagsAsync([Path] Guid processId);

    [Post("process/{processId}/tags")]
    Task<TagDto> CreateTagAsync([Path] Guid processId, [Body] CreateTagRequest request);

    [Delete("tags/{id}")]
    Task DeleteTagAsync([Path] Guid id);

    // process
    [Get("process/my")]
    Task<PagedResult<ProcessDto>> GetMyProcessesAsync([Query] int pageNumber, [Query] int pageSize);
    [Post("process")]
    Task<ProcessDto> CreateProcessAsync([Body] CreateProcessRequest request);
}

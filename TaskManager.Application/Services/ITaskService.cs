using TaskManager.AppCore.Common;
using TaskManager.Domain.Entities;

namespace TaskManager.AppCore.Services;

public interface ITaskService
{
    Task<PagedResult<TaskItem>> GetCurrentUserTasksAsync(int pageNumber, int pageSize);
    Task<PagedResult<TaskItem>> GetByProcessIdAsync(Guid processId, int pageNumber, int pageSize);
    Task<TaskItem?> GetByIdAsync(Guid id);
    Task<TaskItem> CreateAsync(TaskItem item);
    Task UpdateAsync(TaskItem item);
    Task DeleteAsync(Guid id);
    Task AssignTaskAsync(Guid taskId, Guid userId);
    Task ChangeStatusAsync(Guid taskId, Domain.Entities.TaskStatus status);
    Task AddTagAsync(Guid taskId, Guid tagId);
    Task RemoveTagAsync(Guid taskId, Guid tagId);
    Task AddRemarkAsync(Guid taskId, string text);
    Task RemoveRemarkAsync(Guid taskId, Guid remarkId);
}
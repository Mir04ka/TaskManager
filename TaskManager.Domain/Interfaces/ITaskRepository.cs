using TaskManager.AppCore.Common;
using TaskManager.Domain.Entities;

namespace TaskManager.Domain.Interfaces;

public interface ITaskRepository
{
    Task<TaskItem?> GetByIdAsync(Guid id);
    Task<PagedResult<TaskItem>> GetByUserIdAsync(Guid userId, int pageNumber, int pageSize);
    Task<PagedResult<TaskItem>> GetByProcessIdAsync(Guid processId, int pageNumber, int pageSize);
    Task AddAsync(TaskItem item);
    Task UpdateAsync(TaskItem item);
    Task DeleteAsync(Guid id);
    Task AssignAsync(Guid taskId, Guid userId, Guid assignedByUserId);
    Task UpdateStatusAsync(Guid taskId, Entities.TaskStatus status);
    Task AddTagAsync(Guid taskId, Guid tagId);
    Task RemoveTagAsync(Guid taskId, Guid tagId);
    Task AddRemarkAsync(TaskRemark remark);
    Task RemoveRemarkAsync(Guid taskId, Guid remarkId);
}
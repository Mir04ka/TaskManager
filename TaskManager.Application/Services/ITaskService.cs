using TaskManager.AppCore.Common;
using TaskManager.Domain.Entities;

namespace TaskManager.AppCore.Services;

public interface ITaskService
{
    Task<PagedResult<TaskItem>> GetCurrentUserTasksAsync(int pageNumber, int pageSize);
    Task AddAsync(TaskItem item);
    Task UpdateAsync(TaskItem item);
    Task DeleteAsync(Guid id);
}
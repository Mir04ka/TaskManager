using TaskManager.Application.Common;
using TaskManager.Domain.Entities;

namespace TaskManager.Domain.Interfaces;

public interface ITaskRepository
{
    Task<PagedResult<TaskItem>> GetByUserIdAsync(Guid userId, int pageNumber, int pageSize);
    Task AddAsync(TaskItem item);
    Task UpdateAsync(TaskItem item);
    Task DeleteAsync(Guid id);
}
using TaskManager.Domain.Entities;

namespace TaskManager.Domain.Interfaces;

public interface ITaskRepository
{
    Task<List<TaskItem>> GetAllAsync();
    Task<List<TaskItem>> GetByUserIdAsync(Guid userId);
    Task AddAsync(TaskItem item);
    Task UpdateAsync(TaskItem item);
    Task DeleteAsync(Guid id);
}
using TaskManager.Domain.Entities;

namespace TaskManager.Application.Services;

public interface ITaskService
{
    Task<List<TaskItem>> GetAllAsync();
    Task<List<TaskItem>> GetCurrentUserTasksAsync();
    Task AddAsync(TaskItem item);
    Task UpdateAsync(TaskItem item);
    Task DeleteAsync(Guid id);
}
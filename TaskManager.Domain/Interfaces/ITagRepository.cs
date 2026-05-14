using TaskManager.Domain.Entities;

namespace TaskManager.Domain.Interfaces;

public interface ITagRepository
{
    Task<TaskTag?> GetByIdAsync(Guid id);
    Task<List<TaskTag>> GetAllAsync();
    Task<List<TaskTag>> GetByProcessIdAsync(Guid processId);
    Task AddAsync(TaskTag tag);
    Task DeleteAsync(Guid id);
}

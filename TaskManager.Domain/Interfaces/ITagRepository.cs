using TaskManager.Domain.Entities;

namespace TaskManager.Domain.Interfaces;

public interface ITagRepository
{
    Task<TaskTag?> GetByIdAsync(Guid id);
    Task<List<TaskTag>> GetAllAsync();
    Task AddAsync(TaskTag tag);
    Task DeleteAsync(Guid id);
}

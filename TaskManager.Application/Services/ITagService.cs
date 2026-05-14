using TaskManager.Domain.Entities;

namespace TaskManager.AppCore.Services;

public interface ITagService
{
    Task<List<TaskTag>> GetByProcessIdAsync(Guid processId);
    Task<TaskTag> CreateAsync(Guid processId, string name);
    Task DeleteAsync(Guid processId, Guid tagId);
}

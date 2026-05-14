using TaskManager.Domain.Entities;

namespace TaskManager.AppCore.Services;

public interface IRemarkService
{
    Task<List<TaskRemark>> GetByTaskIdAsync(Guid taskId);
    Task AddAsync(Guid taskId, string text);
    Task RemoveAsync(Guid taskId, Guid remarkId);
}

using TaskManager.Domain.Entities;

namespace TaskManager.Application.Services;

public interface IRemarkService
{
    Task AddAsync(Guid taskId, string text);
    Task<List<TaskRemark>> GetByTaskIdAsync(Guid taskId);
}

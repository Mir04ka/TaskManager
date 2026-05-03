using TaskManager.Domain.Entities;

namespace TaskManager.Domain.Interfaces;

public interface IRemarkRepository
{
    Task<List<TaskRemark>> GetByTaskIdAsync(Guid taskId);
    Task AddAsync(TaskRemark remark);
    Task RemoveAsync(Guid taskId, Guid id);
}

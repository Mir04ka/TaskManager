using TaskManager.AppCore.Common;
using TaskManager.Domain.Entities;

namespace TaskManager.Domain.Interfaces;

public interface IProcessRepository
{
    Task<Process?> GetByIdAsync(Guid id);
    Task<PagedResult<Process>> GetByUserIdAsync(Guid userId, int pageNumber, int pageSize);
    Task AddAsync(Process process);
    Task AddUserAsync(Guid processId, Guid userId, ProcessRole role);
    Task<ProcessUser?> GetUserRoleAsync(Guid processId, Guid userId);
}

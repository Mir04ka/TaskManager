using TaskManager.AppCore.Common;
using TaskManager.Domain.Entities;

namespace TaskManager.AppCore.Services;

public interface IProcessService
{
    Task<Process> CreateAsync(string name);
    Task AddUserAsync(Guid processId, Guid userId, ProcessRole role);
    Task<PagedResult<Process>> GetCurrentUserProcessesAsync(int pageNumber, int pageSize);
    Task<ProcessRole?> GetCurrentUserRoleAsync(Guid processId);
}

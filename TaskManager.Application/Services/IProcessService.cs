using TaskManager.AppCore.Common;
using TaskManager.Domain.Entities;

namespace TaskManager.AppCore.Services;

public interface IProcessService
{
    Task<Process> CreateAsync(string name);
    Task UpdateAsync(Guid processId, string name);
    Task DeleteAsync(Guid processId);
    Task AddUserAsync(Guid processId, Guid userId, ProcessRole role);
    Task<PagedResult<Process>> GetCurrentUserProcessesAsync(int pageNumber, int pageSize);
    Task<ProcessRole?> GetCurrentUserRoleAsync(Guid processId);
    Task<List<ProcessUser>> GetMembersAsync(Guid processId);
}

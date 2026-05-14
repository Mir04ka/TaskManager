using TaskManager.Domain.Entities;

namespace TaskManager.AppCore.Services;

public interface IProcessService
{
    Task<Process> CreateAsync(string name);
    Task AddUserAsync(Guid processId, Guid userId, ProcessRole role);
    Task<List<Process>> GetCurrentUserProcessesAsync();
    Task<ProcessRole?> GetCurrentUserRoleAsync(Guid processId);
}

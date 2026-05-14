using Microsoft.Extensions.Logging;
using TaskManager.AppCore.Services;
using TaskManager.Domain.Entities;
using TaskManager.Domain.Interfaces;

namespace TaskManager.AppCore.Services;

public class ProcessService : IProcessService
{
    private readonly IProcessRepository _processRepo;
    private readonly ICurrentUserContext _currentUser;
    private readonly ILogger<ProcessService> _logger;

    public ProcessService(
        IProcessRepository processRepo,
        ICurrentUserContext currentUser,
        ILogger<ProcessService> logger)
    {
        _processRepo = processRepo;
        _currentUser = currentUser;
        _logger = logger;
    }
    private Guid GetCurrentUserId()
    {
        return _currentUser.CurrentUserId ?? throw new UnauthorizedAccessException();
    }

    public async Task<Process> CreateAsync(string name)
    {
        var userId = GetCurrentUserId();

        var process = new Process
        {
            Name = name,
            Users = new List<ProcessUser>()
        };

        _logger.LogInformation("Creating process with name: {ProcessName}", name);

        await _processRepo.AddAsync(process);

        _logger.LogInformation("Adding user: {UserId} as process admin", userId);

        await _processRepo.AddUserAsync(process.Id, userId, ProcessRole.Admin);

        _logger.LogInformation("Process successfully created: {ProcessId}", process.Id);

        return process;
    }

    public async Task AddUserAsync(Guid processId, Guid userId, ProcessRole role)
    {
        var currUserId = GetCurrentUserId();
        var currRole = await _processRepo.GetUserRoleAsync(processId, currUserId);

        if (currRole == null)
        {
            throw new UnauthorizedAccessException("User is not in process");
        }

        if (currRole.Role != ProcessRole.Admin)
        {
            throw new UnauthorizedAccessException("Only admin is allowed to add users");
        }

        _logger.LogInformation("Adding user: {UserId} to process: {ProcessId}", userId, processId);

        await _processRepo.AddUserAsync(processId, userId, role);

        _logger.LogInformation("User successfully added to process: {ProcessId}", processId);
    }

    public async Task<List<Domain.Entities.Process>> GetCurrentUserProcessesAsync()
    {
        var userId = GetCurrentUserId();

        _logger.LogInformation("Getting current user processes: {UserId}", userId);

        return await _processRepo.GetByUserIdAsync(userId);
    }

    public async Task<ProcessRole?> GetCurrentUserRoleAsync(Guid processId)
    {
        var userId = GetCurrentUserId();

        _logger.LogInformation("Getting current user process role: {ProcessId}", processId);

        var role = await _processRepo.GetUserRoleAsync(processId, userId);

        return role?.Role;
    }
}

using Microsoft.Extensions.Logging;
using TaskManager.Domain.Entities;
using TaskManager.Domain.Interfaces;

namespace TaskManager.AppCore.Services;

public class RemarkService : IRemarkService
{
    private readonly IRemarkRepository _remarkRepo;
    private readonly ITaskRepository _taskRepo;
    private readonly IProcessRepository _processRepo;
    private readonly ICurrentUserContext _currentUser;
    private readonly ILogger<TagService> _logger;

    public RemarkService (
        IRemarkRepository remarkRepo,
        ITaskRepository taskRepo,
        IProcessRepository processRepo,
        ICurrentUserContext currentUser,
        ILogger<TagService> logger)
    {
        _remarkRepo = remarkRepo;
        _taskRepo = taskRepo;
        _processRepo = processRepo;
        _currentUser = currentUser;
        _logger = logger;
    }

    private Guid CurrentUserId => _currentUser.CurrentUserId ?? throw new UnauthorizedAccessException();

    private async Task<TaskItem> GetTask(Guid taskId)
    {
        return await _taskRepo.GetByIdAsync(taskId) ?? throw new Exception("Task not found");
    }

    private async Task<ProcessRole> GetUserRole(Guid processId)
    {
        var role = await _processRepo.GetUserRoleAsync(processId, CurrentUserId);

        if (role == null)
        {
            throw new UnauthorizedAccessException("User is not in process");
        }

        return role.Role;
    }

    public async Task<List<TaskRemark>> GetByTaskIdAsync(Guid taskId)
    {
        var task = await GetTask(taskId);

        await GetUserRole(task.ProcessId);

        _logger.LogInformation("Getting remarks for tasks: {TaskId}", taskId);

        return await _remarkRepo.GetByTaskIdAsync(taskId);
    }

    public async Task AddAsync(Guid taskId, string text)
    {
        var userId = CurrentUserId;
        var task = await GetTask(taskId);

        await GetUserRole(task.ProcessId);

        var remark = new TaskRemark
        {
            TaskId = taskId,
            UserId = userId,
            Text = text,
            CreatedAt = DateTime.UtcNow
        };

        _logger.LogInformation("Adding remark to task: {TaskId} by user: {UserId}", taskId, userId);

        await _remarkRepo.AddAsync(remark);

        _logger.LogInformation("Remark added successfully: {TaskId}", taskId);
    }

    public async Task RemoveAsync(Guid taskId, Guid remarkId)
    {
        var userId = CurrentUserId;
        var task = await GetTask(taskId);

        var role = await GetUserRole(task.ProcessId);

        if (role != ProcessRole.Admin)
        {
            throw new UnauthorizedAccessException("Only admin can remove remarks");
        }

        _logger.LogInformation("Removing remark: {RemarkId} from task: {TaskId} by user: {UserId}", remarkId, taskId, userId);

        await _remarkRepo.RemoveAsync(taskId, remarkId);

        _logger.LogInformation("Remark removed successfully: {TaskId}", taskId);
    }
}

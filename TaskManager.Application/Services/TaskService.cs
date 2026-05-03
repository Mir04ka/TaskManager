using Microsoft.Extensions.Logging;
using TaskManager.AppCore.Common;
using TaskManager.Domain.Entities;
using TaskManager.Domain.Interfaces;

namespace TaskManager.AppCore.Services;

public class TaskService : ITaskService
{
    private readonly ITaskRepository _taskRepo;
    private readonly IProcessRepository _processRepo;
    private readonly ICurrentUserContext _currentUser;
    private readonly ILogger<TaskService> _logger;

    public TaskService(
        ITaskRepository taskRepo,
        IProcessRepository processRepo,
        ICurrentUserService currentUser,
        ILogger<TaskService> logger)
    {
        _taskRepo = taskRepo;
        _processRepo = processRepo;
        _currentUser = currentUser;
        _logger = logger;
    }

    private async Task CheckAdmin(Guid processId)
    {
        var userId = _currentUser.CurrentUserId ?? throw new UnauthorizedAccessException();
        var role = await _processRepo.GetUserRoleAsync(processId, userId);

        if (role == null)
        {
            throw new UnauthorizedAccessException("User is not in process");
        }

        if (role?.Role != ProcessRole.Admin)
        {
            throw new UnauthorizedAccessException("User is not process admin");
        }
    }

    private Guid GetCurrentUserId()
    {
        return _currentUser.CurrentUserId ?? throw new UnauthorizedAccessException();
    }

    private async Task<TaskItem> GetTask(Guid taskId)
    {
        return await _taskRepo.GetByIdAsync(taskId) ?? throw new Exception("Task not found");
    }

    public async Task<PagedResult<TaskItem>> GetCurrentUserTasksAsync(int pageNumber, int pageSize)
    {
        pageNumber = pageNumber < 1 ? 1 : pageNumber;
        pageSize = pageSize < 1 ? 10 : pageSize;
        pageSize = pageSize > 100 ? 100 : pageSize;
        
        if (_currentUser.CurrentUserId == null)
        {
            _logger.LogWarning("Attempted to get tasks without logged in user");
            return new PagedResult<TaskItem>
            {
                Items = new List<TaskItem>(),
                TotalCount = 0,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }
        
        var userId = _currentUser.CurrentUserId.Value;

        _logger.LogInformation("Loading tasks for user: {UserId}, page {Page}, size {Size}",
            userId, pageNumber, pageSize);

        var result = await _taskRepo.GetByUserIdAsync(userId, pageNumber, pageSize);
        
        _logger.LogInformation("Loaded {Count} tasks for user: {UserId}",
            result.Items.Count, userId);

        return result;
    }
    
    public async Task<TaskItem> CreateAsync(TaskItem item)
    {
        var userId = GetCurrentUserId();

        await CheckAdmin(item.ProcessId);

        item.AssignedByUserId = userId;

        _logger.LogInformation("Creating new task: {Title} in process: {ProcessId}", item.Title, item.ProcessId);
        
        await _taskRepo.AddAsync(item);

        _logger.LogInformation("Task added successfully: {TaskId}", item.Id);

        return item;
    }

    public async Task UpdateAsync(TaskItem item)
    {
        var userId = GetCurrentUserId();
        var task = await GetTask(item.Id);

        await CheckAdmin(task.ProcessId);

        task.Title = item.Title;
        task.Description = item.Description;
        task.Deadline = item.Deadline;
        task.Status = item.Status;

        _logger.LogInformation("Updating task: {TaskId} by user: {UserId}", item.Id, userId);

        await _taskRepo.UpdateAsync(task);

        _logger.LogInformation("Task updated successfully: {TaskId}", item.Id);
    }

    public async Task DeleteAsync(Guid id)
    {
        var userId = GetCurrentUserId();
        var task = await GetTask(id);

        await CheckAdmin(task.ProcessId);

        _logger.LogInformation("Deleting task: {TaskId} by user: {UserId}", id, userId);

        await _taskRepo.DeleteAsync(id);

        _logger.LogInformation("Task deleted successfully: {TaskId}", id);
    }

    public async Task AssignTaskAsync(Guid taskId, Guid targetUserId)
    {
        var userId = GetCurrentUserId();
        var task = await GetTask(taskId);

        await CheckAdmin(task.ProcessId);

        var targetRole = await _processRepo.GetUserRoleAsync(task.ProcessId, targetUserId);

        if (targetRole == null)
        {
            throw new Exception("Target user is not in process");
        }

        _logger.LogInformation("Assigning task: {TaskId} to user: {UserId}", taskId, targetUserId);

        await _taskRepo.AssignAsync(taskId, targetUserId, userId);

        _logger.LogInformation("Task assigned successfully: {TaskId}", taskId);
    }

    public async Task ChangeStatusAsync(Guid taskId, Domain.Entities.TaskStatus status)
    {
        var userId = GetCurrentUserId();
        var task = await GetTask(taskId);

        if (task.AssignedToUserId != userId)
        {
            await CheckAdmin(task.ProcessId);
        }

        task.Status = status;

        _logger.LogInformation("Changing task status to {Status} by user: {UserId}", status.ToString(), userId);

        await _taskRepo.UpdateAsync(task);

        _logger.LogInformation("Task status changed successfully: {TaskId}", taskId);
    }

    public async Task AddTagAsync(Guid taskId, Guid tagId)
    {
        var userId = GetCurrentUserId();
        var task = await GetTask(taskId);

        await CheckAdmin(task.ProcessId);

        _logger.LogInformation("Adding tag: {TagId} to task: {TaskId} by user: {UserId}", tagId, taskId, userId);

        await _taskRepo.AddTagAsync(taskId, tagId);

        _logger.LogInformation("Tag added successfully: {TaskId}", taskId);
    }

    public async Task RemoveTagAsync(Guid taskId, Guid tagId)
    {
        var userId = GetCurrentUserId();
        var task = await GetTask(taskId);

        await CheckAdmin(task.ProcessId);

        _logger.LogInformation("Removing tag: {TagId} from task: {TaskId} by user: {UserId}", tagId, taskId, userId);

        await _taskRepo.RemoveTagAsync(task.Id, tagId);

        _logger.LogInformation("Tag removed successfully: {TaskId}", taskId);
    }

    public async Task AddRemarkAsync(Guid taskId, string text)
    {
        var userId = GetCurrentUserId();
        var task = await GetTask(taskId);

        var role = await _processRepo.GetUserRoleAsync(task.ProcessId, userId);

        if (role == null)
        {
            throw new UnauthorizedAccessException("User is not in process");
        }

        var remark = new TaskRemark
        {
            TaskId = taskId,
            UserId = userId,
            Text = text
        };

        _logger.LogInformation("Adding remark to task: {TaskId} by user: {UserId}", taskId, userId);

        await _taskRepo.AddRemarkAsync(remark);

        _logger.LogInformation("Remark added successfully: {TaskId}", taskId);
    }

    public async Task RemoveRemarkAsync(Guid taskId, Guid remarkId)
    {
        var userId = GetCurrentUserId();
        var task = await GetTask(taskId);

        await CheckAdmin(task.ProcessId);

        _logger.LogInformation("Removing remark: {RemarkId} from task: {TaskId} by user: {UserId}", remarkId, taskId, userId);

        await _taskRepo.RemoveRemarkAsync(task.Id, remarkId);

        _logger.LogInformation("Remark removed successfully: {TaskId}", taskId);
    }
}
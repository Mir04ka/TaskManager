using TaskManager.Domain.Entities;
using TaskManager.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace TaskManager.Application.Services;

public class TaskService : ITaskService
{
    private readonly ITaskRepository _repo;
    private readonly ICurrentUserService _currentUser;
    private readonly ILogger<TaskService> _logger;
    
    public TaskService(ITaskRepository repo, ICurrentUserService currentUser, ILogger<TaskService> logger)
    {
        _repo = repo;
        _currentUser = currentUser;
        _logger = logger;
    }

    public Task<List<TaskItem>> GetAllAsync() => _repo.GetAllAsync();
    
    public Task<List<TaskItem>> GetCurrentUserTasksAsync()
    {
        if (_currentUser.CurrentUserId == null)
        {
            _logger.LogWarning("Attempted  to get tasks without logged in user");
            return Task.FromResult(new List<TaskItem>());
        }
        
        _logger.LogInformation("Loading tasks for user: {UserId}", _currentUser.CurrentUserId);
        var tasks = _repo.GetByUserIdAsync(_currentUser.CurrentUserId.Value);
        _logger.LogInformation("Loaded tasks for user: {UserId}", _currentUser.CurrentUserId);

        return tasks;
    }
    
    public async Task AddAsync(TaskItem item)
    {
        if (_currentUser.CurrentUserId != null)
            item.UserId = _currentUser.CurrentUserId.Value;
        
        _logger.LogInformation("Adding new task: {Title} for user: {UserId}", item.Title, item.UserId);
        await _repo.AddAsync(item);
        _logger.LogInformation("Task added successfully: {TaskId}", item.Id);
    }

    public async Task UpdateAsync(TaskItem item)
    {
        // Preserve the current user's ID
        if (_currentUser.CurrentUserId != null)
            item.UserId = _currentUser.CurrentUserId.Value;
        
        _logger.LogInformation("Updating task: {TaskId} - {Title}", item.Id, item.Title);
        await _repo.UpdateAsync(item);
        _logger.LogInformation("Task updated successfully: {TaskId}", item.Id);
    }

    public async Task DeleteAsync(Guid id)
    {
        _logger.LogInformation("Deleting task: {TaskId}", id);
        await _repo.DeleteAsync(id);
        _logger.LogInformation("Task deleted successfully: {TaskId}", id);
    }
}
using TaskManager.Domain.Entities;
using TaskManager.Domain.Interfaces;

namespace TaskManager.Application.Services;

public class TaskService : ITaskService
{
    private readonly ITaskRepository _repo;
    private readonly ICurrentUserService _currentUser;
    
    public TaskService(ITaskRepository repo, ICurrentUserService currentUser)
    {
        _repo = repo;
        _currentUser = currentUser;
    }

    public Task<List<TaskItem>> GetAllAsync() => _repo.GetAllAsync();
    
    public Task<List<TaskItem>> GetCurrentUserTasksAsync()
    {
        if (_currentUser.CurrentUserId == null)
            return Task.FromResult(new List<TaskItem>());
        
        return _repo.GetByUserIdAsync(_currentUser.CurrentUserId.Value);
    }
    
    public async Task AddAsync(TaskItem item)
    {
        if (_currentUser.CurrentUserId != null)
            item.UserId = _currentUser.CurrentUserId.Value;
        
        await _repo.AddAsync(item);
    }

    public async Task UpdateAsync(TaskItem item)
    {
        // Preserve the current user's ID
        if (_currentUser.CurrentUserId != null)
            item.UserId = _currentUser.CurrentUserId.Value;
        
        await _repo.UpdateAsync(item);
    }
    
    public Task DeleteAsync(Guid id) => _repo.DeleteAsync(id);
}
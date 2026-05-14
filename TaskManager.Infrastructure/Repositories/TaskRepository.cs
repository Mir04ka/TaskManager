using Microsoft.EntityFrameworkCore;
using TaskManager.AppCore.Common;
using TaskManager.Domain.Entities;
using TaskManager.Domain.Interfaces;
using TaskManager.Infrastructure.Data;

namespace TaskManager.Infrastructure.Repositories;

public class TaskRepository : ITaskRepository
{
    private readonly AppDbContext _db;
    public TaskRepository(AppDbContext db) => _db = db;

    public async Task<TaskItem?> GetByIdAsync(Guid id)
        => await _db.Tasks
            .Include(t => t.Tags).ThenInclude(tt => tt.Tag)
            .Include(t => t.Remarks).ThenInclude(r => r.User)
            .FirstOrDefaultAsync(t => t.Id == id);

    public async Task<PagedResult<TaskItem>> GetByUserIdAsync(Guid userId, int pageNumber, int pageSize)
    {
        var query = _db.Tasks
            .Where(t => t.AssignedToUserId == userId)
            .Include(t => t.Tags).ThenInclude(tt => tt.Tag)
            .AsNoTracking();
        
        var totalCount  = await query.CountAsync();
        
        var items = await query
            .OrderBy(t => t.Deadline)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResult<TaskItem>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }

    public async Task<PagedResult<TaskItem>> GetByProcessIdAsync(Guid processId, int pageNumber, int pageSize)
    {
        var query = _db.Tasks
            .Where(t => t.ProcessId == processId)
            .Include(t => t.Tags).ThenInclude(tt => tt.Tag)
            .Include(t => t.AssignedToUser)
            .AsNoTracking();

        var totalCount = await query.CountAsync();
        var items = await query
            .OrderBy(t => t.Deadline)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResult<TaskItem>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }

    public async Task AddAsync(TaskItem item)
    {
        _db.Tasks.Add(item);
        await _db.SaveChangesAsync();
    }

    public async Task UpdateAsync(TaskItem item)
    {
        var tracked = _db.Tasks.Local.FirstOrDefault(t => t.Id == item.Id);
        if (tracked != null)
        {
            _db.Entry(tracked).State = EntityState.Detached;
        }
        
        _db.Tasks.Update(item);
        await _db.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var item = await _db.Tasks.FindAsync(id);
        if (item != null)
        {
            _db.Tasks.Remove(item);
            await _db.SaveChangesAsync();
        }
    }

    public async Task AssignAsync(Guid taskId, Guid userId, Guid assignedByUserId)
    {
        var task = await _db.Tasks.FindAsync(taskId)
            ?? throw new Exception("Task not found");

        task.AssignedToUserId = userId;
        task.AssignedByUserId = assignedByUserId;

        await _db.SaveChangesAsync();
    }

    public async Task UpdateStatusAsync(Guid taskId, Domain.Entities.TaskStatus status)
    {
        var task = await _db.Tasks.FindAsync(taskId)
            ?? throw new Exception("Task not found");

        task.Status = status;
        await _db.SaveChangesAsync();
    }

    public async Task AddTagAsync(Guid taskId, Guid tagId)
    {
        var alreadyExists = await _db.TaskItemTags
            .AnyAsync(tt => tt.TaskId == taskId && tt.TagId == tagId);

        if (!alreadyExists)
        {
            _db.TaskItemTags.Add(new TaskItemTag { TaskId = taskId, TagId = tagId });
            await _db.SaveChangesAsync();
        }
    }

    public async Task RemoveTagAsync(Guid taskId, Guid tagId)
    {
        var entry = await _db.TaskItemTags
            .FirstOrDefaultAsync(tt => tt.TaskId == taskId && tt.TagId == tagId);

        if (entry != null)
        {
            _db.TaskItemTags.Remove(entry);
            await _db.SaveChangesAsync();
        }
    }

    public async Task AddRemarkAsync(TaskRemark remark)
    {
        _db.TaskRemarks.Add(remark);
        await _db.SaveChangesAsync();
    }

    public async Task RemoveRemarkAsync(Guid taskId, Guid remarkId)
    {
        var remark = await _db.TaskRemarks
            .FirstOrDefaultAsync(r => r.TaskId == taskId && r.Id == remarkId);

        if (remark != null)
        {
            _db.TaskRemarks.Remove(remark);
            await _db.SaveChangesAsync();
        }
    }
}
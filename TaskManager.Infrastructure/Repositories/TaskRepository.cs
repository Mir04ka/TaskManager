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

    public async Task<PagedResult<TaskItem>> GetByUserIdAsync(Guid userId, int pageNumber, int pageSize)
    {
        var query = _db.Tasks
            .Where(t => t.UserId == userId)
            .AsNoTracking();
        
        var totalCount  = await query.CountAsync();
        
        var items = await query
            .OrderBy(t => t.Id)
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
}
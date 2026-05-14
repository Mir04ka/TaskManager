using Microsoft.EntityFrameworkCore;
using TaskManager.Domain.Entities;
using TaskManager.Domain.Interfaces;
using TaskManager.Infrastructure.Data;

namespace TaskManager.Infrastructure.Repositories;

public class TagRepository : ITagRepository
{
    private readonly AppDbContext _db;
    
    public TagRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<TaskTag?> GetByIdAsync(Guid id)
    {
        return await _db.TaskTags.FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task<List<TaskTag>> GetAllAsync()
    {
        return await _db.TaskTags.AsNoTracking().ToListAsync();
    }

    public async Task<List<TaskTag>> GetByProcessIdAsync(Guid processId)
    {
        return await _db.TaskTags
            .Where(t => t.ProcessId == processId)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task AddAsync(TaskTag tag)
    {
        _db.TaskTags.Add(tag);
        await _db.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var tag = await _db.TaskTags.FindAsync(id);
        if (tag != null)
        {
            _db.TaskTags.Remove(tag);
            await _db.SaveChangesAsync();
        }
    }
}

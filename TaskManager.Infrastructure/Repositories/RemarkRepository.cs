using Microsoft.EntityFrameworkCore;
using TaskManager.Domain.Entities;
using TaskManager.Domain.Interfaces;
using TaskManager.Infrastructure.Data;

namespace TaskManager.Infrastructure.Repositories;

public class RemarkRepository : IRemarkRepository
{
    private readonly AppDbContext _db;

    public RemarkRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<TaskRemark>> GetByTaskIdAsync(Guid taskId)
    {
        return await _db.TaskRemarks
            .Where(r => r.TaskId == taskId)
            .Include(r => r.User)
            .OrderBy(r => r.CreatedAt)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task AddAsync(TaskRemark remark)
    {
        _db.TaskRemarks.Add(remark);
        await _db.SaveChangesAsync();
    }

    public async Task RemoveAsync(Guid taskId, Guid id)
    {
        var remark = await _db.TaskRemarks.FirstOrDefaultAsync(r => r.TaskId == taskId && r.Id == id);

        if (remark != null)
        {
            _db.TaskRemarks.Remove(remark);
            await _db.SaveChangesAsync();
        }
    }
}

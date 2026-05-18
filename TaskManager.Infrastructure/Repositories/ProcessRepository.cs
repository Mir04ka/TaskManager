using Microsoft.EntityFrameworkCore;
using TaskManager.AppCore.Common;
using TaskManager.Domain.Entities;
using TaskManager.Domain.Interfaces;
using TaskManager.Infrastructure.Data;

namespace TaskManager.Infrastructure.Repositories;

public class ProcessRepository : IProcessRepository
{
    private readonly AppDbContext _db;
    
    public ProcessRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<Process?> GetByIdAsync(Guid id)
    {
        return await _db.Processes.Include(p => p.Users).FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<PagedResult<Process>> GetByUserIdAsync(Guid userId, int pageNumber, int pageSize)
    {
        var query = _db.Processes
            .Where(p => p.Users.Any(u => u.UserId == userId))
            .Include(p => p.Users)
            .AsNoTracking();

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderBy(t => t.Name)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResult<Process>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }

    public async Task AddAsync(Process process)
    {
        _db.Processes.Add(process);
        await _db.SaveChangesAsync();
    }

    public async Task UpdateAsync(Process process)
    {
        _db.Processes.Update(process);
        await _db.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var process = await _db.Processes.FindAsync(id);
        if (process != null)
        {
            _db.Processes.Remove(process);
            await _db.SaveChangesAsync();
        }
    }

    public async Task AddUserAsync(Guid processId, Guid userId, ProcessRole role)
    {
        var existing = await _db.ProcessUsers.FirstOrDefaultAsync(pu => pu.ProcessId == processId && pu.UserId == userId);

        if (existing != null)
        {
            existing.Role = role;
        }
        else
        {
            _db.ProcessUsers.Add(new ProcessUser
            {
                ProcessId = processId,
                UserId = userId,
                Role = role
            });
        }

        await _db.SaveChangesAsync();
    }

    public async Task<ProcessUser?> GetUserRoleAsync(Guid processId, Guid userId)
    {
        return await _db.ProcessUsers.FirstOrDefaultAsync(pu => pu.ProcessId == processId && pu.UserId == userId);
    }

    public async Task<List<ProcessUser>> GetMembersAsync(Guid processId)
    {
        return await _db.ProcessUsers
            .Include(pu => pu.User)
            .Where(pu => pu.ProcessId == processId)
            .ToListAsync();
    }
}

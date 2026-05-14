using Microsoft.EntityFrameworkCore;
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

    public async Task<List<Process>> GetByUserIdAsync(Guid userId)
    {
        return await _db.Processes
            .Where(p => p.Users.Any(u => u.UserId == userId))
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task AddAsync(Process process)
    {
        _db.Processes.Add(process);
        await _db.SaveChangesAsync();
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
}

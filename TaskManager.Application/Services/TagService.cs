using Microsoft.Extensions.Logging;
using System.Data;
using System.Diagnostics;
using System.Xml.Linq;
using TaskManager.AppCore.Services;
using TaskManager.Domain.Entities;
using TaskManager.Domain.Interfaces;

namespace TaskManager.Application.Services;

public class TagService
{
    private readonly ITagRepository _tagRepo;
    private readonly IProcessRepository _processRepo;
    private readonly ICurrentUserContext _currentUser;
    private readonly ILogger<TagService> _logger;

    public TagService(
        ITagRepository tagRepo,
        IProcessRepository processRepo,
        ICurrentUserContext currentUser,
        ILogger<TagService> logger)
    {
        _tagRepo = tagRepo;
        _processRepo = processRepo;
        _currentUser = currentUser;
        _logger = logger;
    }

    private Guid CurrentUserId => _currentUser.CurrentUserId ?? throw new UnauthorizedAccessException();

    private async Task CheckAdmin(Guid processId)
    {
        var role = await _processRepo.GetUserRoleAsync(processId, CurrentUserId);

        if (role == null)
            throw new UnauthorizedAccessException("User not in process");

        if (role.Role != ProcessRole.Admin)
            throw new UnauthorizedAccessException("User is not admin");
    }

    public async Task<List<TaskTag>> GetByProcessIdAsync(Guid processId)
    {
        var role = await _processRepo.GetUserRoleAsync(processId, CurrentUserId);

        if (role == null)
        {
            throw new UnauthorizedAccessException("User not in process");
        }

        _logger.LogInformation("Getting all tags");

        return await _tagRepo.GetAllAsync();
    }

    public async Task<TaskTag> CreateAsync(Guid processId, string name)
    {
        await CheckAdmin(processId);

        var tag = new TaskTag
        {
            Id = Guid.NewGuid(),
            Name = name,
            ProcessId = processId
        };

        _logger.LogInformation("Creating new tag: {TagName}", name);

        await _tagRepo.AddAsync(tag);

        return tag;
    }

    public async Task DeleteAsync(Guid id)
    {
        var tag = await _tagRepo.GetByIdAsync(id) ?? throw new Exception("Tag not found");

        await CheckAdmin(tag.ProcessId);

        _logger.LogInformation("Deleting tag: {TagId}", id);

        await _tagRepo.DeleteAsync(id);
    }
}

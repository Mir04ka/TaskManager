using Microsoft.Extensions.Logging;
using TaskManager.AppCore.Services;
using TaskManager.Domain.Entities;
using TaskManager.Domain.Interfaces;

namespace TaskManager.AppCore.Services;

public class TagService : ITagService
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

    private async Task CheckAdminAsync(Guid processId)
    {
        var role = await _processRepo.GetUserRoleAsync(processId, CurrentUserId);

        if (role == null)
        {
            throw new UnauthorizedAccessException("User not in process");
        }

        if (role.Role != ProcessRole.Admin)
        {
            throw new UnauthorizedAccessException("Only process admin can manage tags");
        }
    }

    public async Task<List<TaskTag>> GetByProcessIdAsync(Guid processId)
    {
        var role = await _processRepo.GetUserRoleAsync(processId, CurrentUserId);

        if (role == null)
        {
            throw new UnauthorizedAccessException("User not in process");
        }

        _logger.LogInformation("Getting all tags");

        return await _tagRepo.GetByProcessIdAsync(processId);
    }

    public async Task<TaskTag> CreateAsync(Guid processId, string name)
    {
        await CheckAdminAsync(processId);

        var tag = new TaskTag
        {
            Id = Guid.NewGuid(),
            Name = name,
            ProcessId = processId
        };

        _logger.LogInformation("Creating tag: {TagName} in process: {ProcessId}", name, processId);

        await _tagRepo.AddAsync(tag);

        return tag;
    }

    public async Task DeleteAsync(Guid processId, Guid tagId)
    {
        await CheckAdminAsync(processId);

        var tag = await _tagRepo.GetByIdAsync(tagId) ?? throw new Exception("Tag not found");

        if (tag.ProcessId != processId)
            throw new UnauthorizedAccessException("Tag does not belong to this process");

        _logger.LogInformation("Deleting tag: {TagId}", tagId);

        await _tagRepo.DeleteAsync(tagId);
    }
}

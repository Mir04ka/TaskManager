using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskManager.API.DTOs;
using TaskManager.AppCore.Services;

namespace TaskManager.API.Controllers;

[Authorize]
[ApiController]
[Route("api/processes/{processId}/tags")]
public class TagsController : ControllerBase
{
    private readonly ITagService _tagService;
    private readonly ILogger<TagsController> _logger;

    public TagsController(ITagService tagService, ILogger<TagsController> logger)
    {
        _tagService = tagService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<List<TagDto>>> GetTags(Guid processId)
    {
        var tags = await _tagService.GetByProcessIdAsync(processId);

        return Ok(tags.Select(t => new TagDto { 
            Id = t.Id,
            Name = t.Name,
            ProcessId = t.ProcessId
        }));
    }

    [HttpPost]
    public async Task<ActionResult<TagDto>> CreateTag(Guid processId, [FromBody] CreateTagRequest request)
    {
        _logger.LogInformation("CreateTag: {Name} in process: {ProcessId}", request.Name, processId);

        var tag = await _tagService.CreateAsync(processId, request.Name);
        var dto = new TagDto { 
            Id = tag.Id,
            Name = tag.Name,
            ProcessId = tag.ProcessId
        };

        return CreatedAtAction(nameof(GetTags), new { processId }, dto);
    }

    [HttpDelete("{tagId}")]
    public async Task<ActionResult> DeleteTag(Guid processId, Guid tagId)
    {
        _logger.LogInformation("DeleteTag: {TagId} from process: {ProcessId}", tagId, processId);

        await _tagService.DeleteAsync(processId, tagId);

        return NoContent();
    }
}

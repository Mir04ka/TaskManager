using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskManager.API.DTOs;
using TaskManager.AppCore.Services;

namespace TaskManager.API.Controllers;

[Authorize]
[ApiController]
[Route("api/tasks/{taskId}/remarks")]
public class RemarksController : ControllerBase
{
    private readonly ITaskService _taskService;
    private readonly ILogger<RemarksController> _logger;

    public RemarksController(ITaskService taskService, ILogger<RemarksController> logger)
    {
        _taskService = taskService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<List<RemarkDto>>> GetRemarks(Guid taskId)
    {
        var task = await _taskService.GetByIdAsync(taskId);

        if (task == null)
            return NotFound("Task not found");

        var remarks = task.Remarks.Select(r => new RemarkDto
        {
            Id = r.Id,
            TaskId = r.TaskId,
            UserId = r.UserId,
            Username = r.User?.Username ?? string.Empty,
            Text = r.Text,
            CreatedAt = r.CreatedAt
        }).ToList();

        return Ok(remarks);
    }

    [HttpPost]
    public async Task<ActionResult> AddRemark(Guid taskId, [FromBody] AddRemarkRequest request)
    {
        _logger.LogInformation("AddRemark to task: {TaskId}", taskId);

        await _taskService.AddRemarkAsync(taskId, request.Text);

        return NoContent();
    }

    [HttpDelete("{remarkId}")]
    public async Task<ActionResult> DeleteRemark(Guid taskId, Guid remarkId)
    {
        _logger.LogInformation("DeleteRemark: {RemarkId} from task: {TaskId}", remarkId, taskId);

        await _taskService.RemoveRemarkAsync(taskId, remarkId);

        return NoContent();
    }
}

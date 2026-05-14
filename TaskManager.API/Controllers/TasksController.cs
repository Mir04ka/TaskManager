using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskManager.API.DTOs;
using TaskManager.AppCore.Common;
using TaskManager.AppCore.Services;
using TaskManager.Domain.Entities;

namespace TaskManager.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class TasksController : ControllerBase
{
    private readonly ITaskService _taskService;
    private readonly ILogger<TasksController> _logger;

    public TasksController(ITaskService taskService, ILogger<TasksController> logger)
    {
        _taskService = taskService;
        _logger = logger;
    }

    [HttpGet("my")]
    public async Task<ActionResult<PagedResult<TaskItemDto>>> GetMyTasks(int pageNumber = 1, int pageSize = 10)
    {
        _logger.LogInformation("GetMyTasks – page {Page}, size {Size}", pageNumber, pageSize);

        var result = await _taskService.GetCurrentUserTasksAsync(pageNumber, pageSize);

        return Ok(MapPagedResult(result));
    }

    [HttpGet("process/{processId}")]
    public async Task<ActionResult<PagedResult<TaskItemDto>>> GetProcessTasks(
        Guid processId,
        int pageNumber = 1,
        int pageSize = 10)
    {
        _logger.LogInformation("GetProcessTasks – process {ProcessId}, page {Page}", processId, pageNumber);

        var result = await _taskService.GetByProcessIdAsync(processId, pageNumber, pageSize);

        return Ok(MapPagedResult(result));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<TaskDetailDto>> GetTask(Guid id)
    {
        var task = await _taskService.GetByIdAsync(id);

        if (task == null)
            return NotFound();

        return Ok(new TaskDetailDto
        {
            Id = task.Id,
            Title = task.Title,
            Description = task.Description,
            ProcessId = task.ProcessId,
            AssignedToUserId = task.AssignedToUserId,
            AssignedToUsername = task.AssignedToUser?.Username ?? string.Empty,
            Status = task.Status.ToString(),
            Deadline = task.Deadline,
            Tags = task.Tags.Select(tt => new TagDto
            {
                Id = tt.Tag.Id,
                Name = tt.Tag.Name,
                ProcessId = tt.Tag.ProcessId
            }).ToList(),
            Remarks = task.Remarks.Select(r => new RemarkDto
            {
                Id = r.Id,
                TaskId = r.TaskId,
                UserId = r.UserId,
                Username = r.User?.Username ?? string.Empty,
                Text = r.Text,
                CreatedAt = r.CreatedAt
            }).ToList()
        });
    }

    [HttpPost]
    public async Task<ActionResult<TaskItemDto>> CreateTask([FromBody] CreateTaskRequest request)
    {
        _logger.LogInformation("CreateTask: {Title} in process: {ProcessId}", request.Title, request.ProcessId);

        var item = new TaskItem
        {
            Title = request.Title,
            Description = request.Description,
            ProcessId = request.ProcessId,
            AssignedToUserId = request.AssignedToUserId,
            Deadline = request.Deadline
        };

        var created = await _taskService.CreateAsync(item);

        var dto = MapToDto(created);
        return CreatedAtAction(nameof(GetTask), new { id = created.Id }, dto);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> UpdateTask(Guid id, [FromBody] UpdateTaskRequest request)
    {
        _logger.LogInformation("UpdateTask: {TaskId}", id);

        if (!Enum.TryParse<Domain.Entities.TaskStatus>(request.Status, out var status))
            return BadRequest("Invalid status value");

        var item = new TaskItem
        {
            Id = id,
            Title = request.Title,
            Description = request.Description,
            Status = status,
            Deadline = request.Deadline
        };

        await _taskService.UpdateAsync(item);

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteTask(Guid id)
    {
        _logger.LogInformation("DeleteTask: {TaskId}", id);

        await _taskService.DeleteAsync(id);

        return NoContent();
    }

    [HttpPost("{id}/assign")]
    public async Task<ActionResult> AssignTask(Guid id, [FromBody] AssignTaskRequest request)
    {
        _logger.LogInformation("AssignTask: {TaskId} to user: {UserId}", id, request.UserId);

        await _taskService.AssignTaskAsync(id, request.UserId);

        return NoContent();
    }

    [HttpPatch("{id}/status")]
    public async Task<ActionResult> ChangeStatus(Guid id, [FromBody] ChangeStatusRequest request)
    {
        if (!Enum.TryParse<Domain.Entities.TaskStatus>(request.Status, out var status))
            return BadRequest("Invalid status value");

        await _taskService.ChangeStatusAsync(id, status);

        return NoContent();
    }

    [HttpPost("{id}/tags")]
    public async Task<ActionResult> AddTag(Guid id, [FromBody] TaskTagRequest request)
    {
        await _taskService.AddTagAsync(id, request.TagId);
        return NoContent();
    }

    [HttpDelete("{id}/tags/{tagId}")]
    public async Task<ActionResult> RemoveTag(Guid id, Guid tagId)
    {
        await _taskService.RemoveTagAsync(id, tagId);
        return NoContent();
    }

    private static PagedResult<TaskItemDto> MapPagedResult(PagedResult<TaskItem> src)
        => new()
        {
            Items = src.Items.Select(MapToDto).ToList(),
            TotalCount = src.TotalCount,
            PageNumber = src.PageNumber,
            PageSize = src.PageSize
        };

    private static TaskItemDto MapToDto(TaskItem t)
        => new()
        {
            Id = t.Id,
            Title = t.Title,
            Description = t.Description,
            ProcessId = t.ProcessId,
            AssignedToUserId = t.AssignedToUserId,
            AssignedToUsername = t.AssignedToUser?.Username ?? string.Empty,
            Status = t.Status.ToString(),
            Deadline = t.Deadline,
            Tags = t.Tags.Select(tt => new TagDto
            {
                Id = tt.Tag.Id,
                Name = tt.Tag.Name,
                ProcessId = tt.Tag.ProcessId
            }).ToList()
        };
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskManager.API.DTOs;
using TaskManager.Application.Common;
using TaskManager.Application.Services;
using TaskManager.Domain.Entities;

namespace TaskManager.API.Controllers;

/// <summary>
/// SRP: Only handles HTTP concerns – routing, status codes, DTO mapping.
///   Pagination validation/clamping moved to TaskService where it belongs.
/// DIP: Depends on ITaskService (Application abstraction), not ITaskRepository (Infrastructure).
///   The current user is resolved by HttpCurrentUserService via ICurrentUserContext.
/// </summary>
[Authorize]
[ApiController]
[Route("api/[controller]")]
public class TasksController : ControllerBase
{
    private readonly ITaskService            _taskService;
    private readonly ILogger<TasksController> _logger;

    public TasksController(ITaskService taskService, ILogger<TasksController> logger)
    {
        _taskService = taskService;
        _logger      = logger;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<TaskItemDto>>> GetTasks(int pageNumber = 1, int pageSize = 10)
    {
        _logger.LogInformation("GetTasks – page {Page}, size {Size}", pageNumber, pageSize);

        // Pagination clamping is now TaskService's responsibility (SRP).
        var result = await _taskService.GetCurrentUserTasksAsync(pageNumber, pageSize);

        var dto = new PagedResult<TaskItemDto>
        {
            Items = result.Items.Select(t => new TaskItemDto
            {
                Id          = t.Id,
                Title       = t.Title,
                Description = t.Description,
            }).ToList(),
            TotalCount = result.TotalCount,
            PageNumber  = result.PageNumber,
            PageSize    = result.PageSize,
        };

        return Ok(dto);
    }

    [HttpPost]
    public async Task<ActionResult<TaskItemDto>> CreateTask([FromBody] CreateTaskRequest request)
    {
        _logger.LogInformation("CreateTask: {Title}", request.Title);

        var task = new TaskItem
        {
            Title       = request.Title,
            Description = request.Description
        };

        await _taskService.AddAsync(task);

        var dto = new TaskItemDto { Id = task.Id, Title = task.Title, Description = task.Description };
        return CreatedAtAction(nameof(GetTasks), new { id = task.Id }, dto);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> UpdateTask(Guid id, [FromBody] CreateTaskRequest request)
    {
        _logger.LogInformation("UpdateTask: {TaskId}", id);

        var task = new TaskItem { Id = id, Title = request.Title, Description = request.Description };
        await _taskService.UpdateAsync(task);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteTask(Guid id)
    {
        _logger.LogInformation("DeleteTask: {TaskId}", id);
        await _taskService.DeleteAsync(id);
        return NoContent();
    }
}

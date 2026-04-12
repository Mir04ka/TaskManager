using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TaskManager.API.DTOs;
using TaskManager.Application.Common;
using TaskManager.Domain.Entities;
using TaskManager.Domain.Interfaces;

namespace TaskManager.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class TasksController : ControllerBase
{
    private readonly ITaskRepository _taskRepo;
    private readonly ILogger<TasksController> _logger;

    public TasksController(ITaskRepository taskRepo, ILogger<TasksController> logger)
    {
        _taskRepo = taskRepo;
        _logger = logger;
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.Parse(userIdClaim!);
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<TaskItemDto>>> GetTasks(int pageNumber = 1, int pageSize = 10)
    {
        var userId = GetCurrentUserId();
        
        _logger.LogInformation(
            "Getting tasks for user {UserId}, page {Page}, size {Size}",
            userId, pageNumber, pageSize);
        
        pageNumber = pageNumber < 1 ? 1 : pageNumber;
        pageSize = pageSize > 100 ? 100 : pageSize;

        var result = await _taskRepo.GetByUserIdAsync(userId, pageNumber, pageSize);

        var dto = new PagedResult<TaskItemDto>
        {
            Items = result.Items.Select(t => new TaskItemDto
            {
                Id = t.Id,
                Title = t.Title,
                Description = t.Description,
            }).ToList(),
            TotalCount = result.TotalCount,
            PageNumber = result.PageNumber,
            PageSize = result.PageSize,
        };

        return Ok(dto);
    }

    [HttpPost]
    public async Task<ActionResult<TaskItemDto>> CreateTask([FromBody] CreateTaskRequest request)
    {
        var userId = GetCurrentUserId();
        _logger.LogInformation("Creating task for user {UserId}", userId);

        var task = new TaskItem
        {
            Title = request.Title,
            Description = request.Description,
            UserId = userId
        };

        await _taskRepo.AddAsync(task);

        var dto = new TaskItemDto
        {
            Id = task.Id,
            Title = task.Title,
            Description = task.Description
        };

        return CreatedAtAction(nameof(GetTasks), new { id = task.Id }, dto);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> UpdateTask(Guid id, [FromBody] CreateTaskRequest request)
    {
        var userId = GetCurrentUserId();
        _logger.LogInformation("Updating task {TaskId} for user {UserId}", id, userId);

        var task = new TaskItem
        {
            Id = id,
            Title = request.Title,
            Description = request.Description,
            UserId = userId
        };

        await _taskRepo.UpdateAsync(task);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteTask(Guid id)
    {
        var userId = GetCurrentUserId();
        _logger.LogInformation("Deleting task {TaskId} for user {UserId}", id, userId);

        await _taskRepo.DeleteAsync(id);
        return NoContent();
    }
}
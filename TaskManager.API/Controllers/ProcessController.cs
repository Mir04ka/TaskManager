using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskManager.API.DTOs;
using TaskManager.AppCore.Services;
using TaskManager.Domain.Entities;

namespace TaskManager.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ProcessController : ControllerBase
{
    private readonly IProcessService _processService;
    private readonly ILogger<ProcessController> _logger;

    public ProcessController(IProcessService processService, ILogger<ProcessController> logger)
    {
        _processService = processService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<List<ProcessDto>>> GetMyProcesses()
    {
        var processes = await _processService.GetCurrentUserProcessesAsync();

        var result = new List<ProcessDto>();

        foreach (var p in processes)
        {
            var role = await _processService.GetCurrentUserRoleAsync(p.Id);
            result.Add(new ProcessDto 
            {
                Id = p.Id,
                Name = p.Name,
                Role = role?.ToString() ?? "Member"
            });
        }

        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<ProcessDto>> CreateProcess([FromBody] CreateProcessRequest request)
    {
        _logger.LogInformation("CreateProcess: {Name}", request.Name);

        var process = await _processService.CreateAsync(request.Name);
        var dto = new ProcessDto {
            Id = process.Id,
            Name = process.Name,
            Role = "Admin"
        };

        return CreatedAtAction(nameof(GetMyProcesses), new { id = process.Id}, dto);
    }

    [HttpPost("{processId}/users")]
    public async Task<ActionResult> AddUser(Guid processId, [FromBody] AddUserToProcessRequest request)
    {
        _logger.LogInformation("AddUser to process: {ProcessId}, user: {UserId}", processId, request.UserId);

        var role = request.Role.ToLower() == "admin" ? ProcessRole.Admin : ProcessRole.Member;
        await _processService.AddUserAsync(processId, request.UserId, role);

        return NoContent();
    }

    [HttpGet("{processId}/role")]
    public async Task<ActionResult<string>> GetMyRole(Guid processId)
    {
        var role = await _processService.GetCurrentUserRoleAsync(processId);

        if (role == null)
        {
            return NotFound("User is not in this process");
        }

        return Ok(role.ToString());
    }
}

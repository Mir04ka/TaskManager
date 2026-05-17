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
public class ProcessController : ControllerBase
{
    private readonly IProcessService _processService;
    private readonly ILogger<ProcessController> _logger;
    private readonly ICurrentUserContext _currentUser;

    public ProcessController(IProcessService processService, ILogger<ProcessController> logger, ICurrentUserContext currentUser)
    {
        _processService = processService;
        _logger = logger;
        _currentUser = currentUser;
    }

    [HttpGet("my")]
    public async Task<ActionResult<List<ProcessDto>>> GetMyProcesses([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        _logger.LogInformation("GetMyProcesses – page {Page}, size {Size}", pageNumber, pageSize);

        var result = await _processService.GetCurrentUserProcessesAsync(pageNumber, pageSize);
        var userId = _currentUser.CurrentUserId ?? Guid.Empty;

        var dtos = new List<ProcessDto>();
        foreach (var p in result.Items)
        {
            var userEntry = p.Users.FirstOrDefault(u => u.UserId == userId);

            string role;
            if (userEntry != null)
            {
                role = userEntry.Role.ToString();
            }
            else
            {
                _logger.LogWarning("Users empty for process {ProcessId}, querying role directly", p.Id);
                var directRole = await _processService.GetCurrentUserRoleAsync(p.Id);
                role = directRole?.ToString() ?? "Member";
            }

            _logger.LogInformation("Process {ProcessId} role={Role} usersCount={Count}", p.Id, role, p.Users.Count);
            dtos.Add(new ProcessDto { Id = p.Id, Name = p.Name, Role = role });
        }

        return Ok(new PagedResult<ProcessDto>
        {
            Items = dtos,
            TotalCount = result.TotalCount,
            PageNumber = result.PageNumber,
            PageSize = result.PageSize
        });
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

    [HttpGet("{processId}/members")]
    public async Task<ActionResult<List<ProcessMemberDto>>> GetMembers(Guid processId)
    {
        _logger.LogInformation("GetMembers for process: {ProcessId}", processId);
        var members = await _processService.GetMembersAsync(processId);
        return Ok(members.Select(m => new ProcessMemberDto
        {
            UserId = m.UserId,
            Username = m.User?.Username ?? string.Empty,
            Role = m.Role.ToString()
        }));
    }

    private static PagedResult<ProcessDto> MapPagedResult(PagedResult<Process> src)
        => new()
        {
            Items = src.Items.Select(MapToDto).ToList(),
            TotalCount = src.TotalCount,
            PageNumber = src.PageNumber,
            PageSize = src.PageSize
        };

    private static ProcessDto MapToDto(Process p)
        => new()
        {
            Id = p.Id,
            Name = p.Name,
            Role = "Admin"
        };
}

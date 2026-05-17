using System;
namespace TaskManager.WinUI.Models;

public class ProcessMemberDto
{
    public Guid UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public bool IsAdmin => Role.Equals("Admin", StringComparison.OrdinalIgnoreCase);
}

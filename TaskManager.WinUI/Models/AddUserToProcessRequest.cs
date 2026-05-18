using System;
namespace TaskManager.WinUI.Models;

public class AddUserToProcessRequest
{
    public Guid UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Role { get; set; } = "Member";
}

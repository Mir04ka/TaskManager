using System;
namespace TaskManager.WinUI.Models;

public class AddUserToProcessRequest
{
    public Guid UserId { get; set; }
    public string Role { get; set; } = "Member";
}

using System;

namespace TaskManager.WinUI.Models;

public class AuthResponse
{
    public string Token { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public Guid UserId { get; set; }
}
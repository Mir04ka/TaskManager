using System;

namespace TaskManager.WinUI.Models;

public class UpdateTaskRequest
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime? Deadline { get; set; }
    public string Status { get; set; } = string.Empty;
}
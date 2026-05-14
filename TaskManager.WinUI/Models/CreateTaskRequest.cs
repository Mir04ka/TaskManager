using System;

namespace TaskManager.WinUI.Models;

public class CreateTaskRequest
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Guid ProcessId { get; set; }
    public Guid? AssignedToUserId { get; set; }
    public DateTime? Deadline { get; set; }
}
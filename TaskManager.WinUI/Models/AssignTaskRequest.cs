using System;

namespace TaskManager.WinUI.Models;

public class AssignTaskRequest
{
    public Guid TaskId { get; set; }
    public Guid UserId { get; set; }
}

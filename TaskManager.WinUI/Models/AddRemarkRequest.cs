using System;

namespace TaskManager.WinUI.Models;

public class AddRemarkRequest
{
    public Guid TaskId { get; set; }
    public string Text { get; set; } = string.Empty;
}

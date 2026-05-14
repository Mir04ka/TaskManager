using System;

namespace TaskManager.WinUI.Models;

public class CreateTagRequest
{
    public Guid ProcessId { get; set; }
    public string Name { get; set; } = string.Empty;
}

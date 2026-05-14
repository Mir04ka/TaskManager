using System;
using System.Collections.Generic;
using TaskManager.Domain.Entities;

namespace TaskManager.WinUI.Models;

public class TaskItemDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime? Deadline { get; set; }
    public string AssignedToUsername { get; set; } = string.Empty;
    public List<string> Tags { get; set; } = new();
    
    public bool CanEdit { get; set; }
    public bool CanDelete { get; set; }
    public bool CanAssign { get; set; }
}
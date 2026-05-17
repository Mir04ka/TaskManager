using System;
using System.Collections.Generic;
using TaskManager.Domain.Entities;

namespace TaskManager.WinUI.Models;

public class TaskItemDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Guid ProcessId { get; set; }
    public Guid? AssignedToUserId { get; set; }
    public string AssignedToUsername { get; set; } = string.Empty;
    public string Status { get; set; } = "Todo";
    public DateTime? Deadline { get; set; }
    public List<TagDto> Tags { get; set; } = new();
    public List<RemarkDto> Remarks { get; set; } = new();
}
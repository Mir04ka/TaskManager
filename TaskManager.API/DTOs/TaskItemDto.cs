namespace TaskManager.API.DTOs;

public class TaskItemDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Guid ProcessId { get; set; }
    public Guid AssignedToUserId { get; set; }
    public string AssignedToUsername { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime? Deadline { get; set; }
    public List<TagDto> Tags { get; set; } = new();
}

public class TaskDetailDto : TaskItemDto
{
    public List<RemarkDto> Remarks { get; set; } = new();
}

public class UpdateTaskRequest
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Status { get; set; } = "Todo";
    public DateTime? Deadline { get; set; }
}

public class ChangeStatusRequest
{
    public string Status { get; set; } = string.Empty;
}

public class AssignTaskRequest
{
    public Guid UserId { get; set; }
}

public class TaskTagRequest
{
    public Guid TagId { get; set; }
}
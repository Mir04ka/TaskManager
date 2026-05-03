namespace TaskManager.Domain.Entities;

public class TaskItemTag
{
    public Guid TaskId { get; set; }
    public TaskItem Task { get; set; } = null!;
    
    public Guid TagId { get; set; }
    public TaskTag Tag { get; set; } = null!;
}

namespace TaskManager.Domain.Entities;

public enum TaskStatus
{
    Todo,
    InProgress,
    Done
}

public class TaskItem
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    
    public Guid ProcessId { get; set; }
    public Process Process { get; set; } = null!;

    public Guid? AssignedToUserId { get; set; }
    public User? AssignedToUser { get; set; } = null!;
    
    public Guid AssignedByUserId { get; set; }

    public TaskStatus Status { get; set; } = TaskStatus.Todo;
    public DateTime? Deadline { get; set; }
    public List<TaskItemTag> Tags { get; set; } = new();
    public List<TaskRemark> Remarks { get; set; } = new();
}
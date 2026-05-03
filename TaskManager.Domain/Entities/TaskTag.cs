namespace TaskManager.Domain.Entities;

public class TaskTag
{
    public Guid Id { get; set; }
    public Guid ProcessId { get; set; }
    public string Name { get; set; } = string.Empty;
}

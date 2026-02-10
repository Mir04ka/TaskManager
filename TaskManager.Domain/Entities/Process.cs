namespace TaskManager.Domain.Entities;

public class Process
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
}
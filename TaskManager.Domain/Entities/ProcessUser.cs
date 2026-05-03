namespace TaskManager.Domain.Entities;

public enum ProcessRole
{
    Member,
    Admin
}

public class ProcessUser
{
    public Guid ProcessId { get; set; }
    public Guid UserId { get; set; }
    public ProcessRole Role { get; set; }
}

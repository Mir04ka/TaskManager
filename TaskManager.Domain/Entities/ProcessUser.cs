namespace TaskManager.Domain.Entities;

public enum ProcessRole
{
    Member,
    Admin
}

public class ProcessUser
{
    public Guid ProcessId { get; set; }
    public Process Process { get; set; } = null!;
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
    public ProcessRole Role { get; set; }
}

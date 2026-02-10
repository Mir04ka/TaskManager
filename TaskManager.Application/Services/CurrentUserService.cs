namespace TaskManager.Application.Services;

public class CurrentUserService : ICurrentUserService
{
    public Guid? CurrentUserId { get; set; }
    public string? CurrentUsername { get; set; }
}
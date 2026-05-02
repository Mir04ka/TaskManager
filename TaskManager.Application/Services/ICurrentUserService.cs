namespace TaskManager.AppCore.Services;

public interface ICurrentUserService : ICurrentUserContext
{
    Guid? CurrentUserId { get; set; }
    string? CurrentUsername { get; set; }
}
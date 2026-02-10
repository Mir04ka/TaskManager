namespace TaskManager.Application.Services;

public interface ICurrentUserService
{
    Guid? CurrentUserId { get; set; }
    string? CurrentUsername { get; set; }
}
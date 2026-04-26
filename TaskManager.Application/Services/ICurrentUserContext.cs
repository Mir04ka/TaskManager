namespace TaskManager.Application.Services;

public interface ICurrentUserContext
{
    Guid?   CurrentUserId   { get; set; }
    string? CurrentUsername { get; set; }
}
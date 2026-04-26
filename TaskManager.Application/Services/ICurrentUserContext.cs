namespace TaskManager.Application.Services;

public interface ICurrentUserContext
{
    Guid?   CurrentUserId   { get; }
    string? CurrentUsername { get; }
}
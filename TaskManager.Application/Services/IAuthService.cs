namespace TaskManager.Application.Services;

public interface IAuthService
{
    Task<bool> RegisterAsync(string username, string password);
    Task<bool> LoginAsync(string username, string password);
}
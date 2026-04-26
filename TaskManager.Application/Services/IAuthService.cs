using TaskManager.Application.Common;

namespace TaskManager.Application.Services;

public interface IAuthService
{
    Task<AuthResult> RegisterAsync(string username, string password);
    Task<AuthResult> LoginAsync(string username, string password);
}

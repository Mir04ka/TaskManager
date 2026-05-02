using TaskManager.AppCore.Common;

namespace TaskManager.AppCore.Services;

public interface IAuthService
{
    Task<AuthResult> RegisterAsync(string username, string password);
    Task<AuthResult> LoginAsync(string username, string password);
}

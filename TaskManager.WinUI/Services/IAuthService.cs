using System;
using System.Threading.Tasks;

namespace TaskManager.WinUI.Services;

public interface IAuthService
{
    Task<bool> TryAutoLoginAsync();
    Task LoginAsync(string token, string username, Guid userId);
    void Logout();
    bool IsAuthenticated { get; }
    string? CurrentUsername { get; }
    Guid? CurrentUserId { get; }
}
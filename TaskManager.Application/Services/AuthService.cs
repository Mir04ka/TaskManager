using TaskManager.Domain.Entities;
using TaskManager.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace TaskManager.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _repo;
    private readonly ICurrentUserService _currentUser;
    private readonly ILogger<AuthService> _logger;
    
    public AuthService(IUserRepository repo, ICurrentUserService currentUser, ILogger<AuthService> logger)
    {
        _repo = repo;
        _currentUser = currentUser;
        _logger = logger;
    }

    public async Task<bool> RegisterAsync(string username, string password)
    {
        _logger.LogInformation("Registration attempt for username: {Username}", username);
        
        var exists = await _repo.GetByUsernameAsync(username);
        if (exists != null)
        {
            _logger.LogWarning("Registration failed - user already exists: {Username}", username);
            return false;
        }

        var hash = BCrypt.Net.BCrypt.HashPassword(password);
        Console.WriteLine($"[REGISTER] Generated hash: {hash}");
        
        var user = new User
        {
            Username = username,
            PasswordHash = hash
        };
        
        await _repo.AddAsync(user);
        _logger.LogInformation("User registered successfully: {Username}", username);
        return true;
    }

    public async Task<bool> LoginAsync(string username, string password)
    {
        _logger.LogInformation("Login attempt for username: {Username}", username);
        
        var user = await _repo.GetByUsernameAsync(username);
        
        if (user == null)
        {
            _logger.LogWarning("User not found: {Username}", username);
            return false;
        }
        
        var verified = BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);
        
        if (!verified)
        {
            _logger.LogWarning("Invalid password for user: {Username}", username);
            return false;
        }
        
        _currentUser.CurrentUserId = user.Id;
        _currentUser.CurrentUsername = user.Username;
        
        _logger.LogInformation("User logged in successfully: {Username} (ID: {UserId})", username, user.Id);
        return true;
    }
}
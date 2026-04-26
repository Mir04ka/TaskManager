using Microsoft.Extensions.Logging;
using TaskManager.Application.Common;
using TaskManager.Domain.Entities;
using TaskManager.Domain.Interfaces;

namespace TaskManager.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepo;
    private readonly ICurrentUserService _currentUser;
    private readonly ILogger<AuthService> _logger;

    public AuthService(IUserRepository userRepo, ICurrentUserService currentUser, ILogger<AuthService> logger)
    {
        _userRepo = userRepo;
        _currentUser = currentUser;
        _logger = logger;
    }

    public async Task<AuthResult> RegisterAsync(string username, string password)
    {
        _logger.LogInformation("Registration attempt for {Username}", username);
        
        var existing = await _userRepo.GetByUsernameAsync(username);
        if (existing != null)
        {
            _logger.LogWarning("Registration failed - user exists: {Username}", username);
            return AuthResult.Fail("User already exists");
        }
        
        var user = new User
        {
            Username = username,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password)
        };
        await _userRepo.AddAsync(user);
        
        _logger.LogInformation("User registered: {Username}", username);
        return AuthResult.Ok(user.Id, user.Username);
    }

    public async Task<AuthResult> LoginAsync(string username, string password)
    {
        _logger.LogInformation("Login attempt for {Username}", username);

        var user = await _userRepo.GetByUsernameAsync(username);
        if (user == null)
        {
            _logger.LogWarning("Login failed - user not found: {Username}", username);
            return AuthResult.Fail("Invalid credentials");
        }

        if (!BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
        {
            _logger.LogWarning("Login failed - invalid password: {Username}", username);
            return AuthResult.Fail("Invalid credentials");
        }
        
        _currentUser.CurrentUserId = user.Id;
        _currentUser.CurrentUsername = user.Username;
        
        _logger.LogInformation("User logged in successfully: {Username} (ID: {UserId})", username, user.Id);
        return AuthResult.Ok(user.Id, user.Username);
    }
}

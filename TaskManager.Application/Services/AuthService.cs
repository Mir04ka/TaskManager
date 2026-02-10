using System;
using TaskManager.Domain.Entities;
using TaskManager.Domain.Interfaces;

namespace TaskManager.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _repo;
    private readonly ICurrentUserService _currentUser;
    
    public AuthService(IUserRepository repo, ICurrentUserService currentUser)
    {
        _repo = repo;
        _currentUser = currentUser;
    }

    public async Task<bool> RegisterAsync(string username, string password)
    {
        Console.WriteLine($"[REGISTER] Username: {username}, Password length: {password.Length}");
        
        var exists = await _repo.GetByUsernameAsync(username);
        if (exists != null)
        {
            Console.WriteLine("[REGISTER] User already exists");
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
        Console.WriteLine("[REGISTER] User saved to database");
        return true;
    }

    public async Task<bool> LoginAsync(string username, string password)
    {
        Console.WriteLine($"[LOGIN] Attempting login for: {username}");
        Console.WriteLine($"[LOGIN] Password length: {password.Length}");
        
        var user = await _repo.GetByUsernameAsync(username);
        
        if (user == null)
        {
            Console.WriteLine("[LOGIN] User not found");
            return false;
        }
        
        Console.WriteLine($"[LOGIN] User found with hash: {user.PasswordHash}");
        
        var verified = BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);
        Console.WriteLine($"[LOGIN] Password verified: {verified}");
        
        if (!verified)
        {
            Console.WriteLine("[LOGIN] Password verification failed");
            return false;
        }
        
        _currentUser.CurrentUserId = user.Id;
        _currentUser.CurrentUsername = user.Username;
        
        Console.WriteLine("[LOGIN] Login successful");
        return true;
    }
}
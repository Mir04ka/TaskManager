using Microsoft.AspNetCore.Mvc;
using TaskManager.API.DTOs;
using TaskManager.API.Services;
using TaskManager.Domain.Entities;
using TaskManager.Domain.Interfaces;

namespace TaskManager.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IUserRepository _userRepo;
    private readonly IJwtService _jwtService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IUserRepository userRepo, IJwtService jwtService, ILogger<AuthController> logger)
    {
        _userRepo = userRepo;
        _jwtService = jwtService;
        _logger = logger;
    }

    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register([FromBody] RegisterRequest request)
    {
        _logger.LogInformation("Registration attempt for {Username}", request.Username);

        var exists = await _userRepo.GetByUsernameAsync(request.Username);
        if (exists != null)
        {
            _logger.LogWarning("Registration failed - user exists: {Username}", request.Username);
            return BadRequest(new { message = "User already exists" });
        }

        var user = new User
        {
            Username = request.Username,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password)
        };

        await _userRepo.AddAsync(user);
        _logger.LogInformation("User registered: {Username}", request.Username);

        var token = _jwtService.GenerateToken(user.Id, user.Username);

        return Ok(new AuthResponse
        {
            Token = token,
            Username = user.Username,
            UserId = user.Id
        });
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest request)
    {
        _logger.LogInformation("Login attempt for {Username}", request.Username);

        var user = await _userRepo.GetByUsernameAsync(request.Username);
        if (user == null)
        {
            _logger.LogWarning("Login failed - user not found: {Username}", request.Username);
            return Unauthorized(new { message = "Invalid credentials" });
        }

        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            _logger.LogWarning("Login failed - invalid password: {Username}", request.Username);
            return Unauthorized(new { message = "Invalid credentials" });
        }

        var token = _jwtService.GenerateToken(user.Id, user.Username);
        _logger.LogInformation("User logged in: {Username}", request.Username);

        return Ok(new AuthResponse
        {
            Token = token,
            Username = user.Username,
            UserId = user.Id
        });
    }
}
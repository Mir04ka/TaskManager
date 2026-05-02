using Microsoft.AspNetCore.Mvc;
using TaskManager.API.DTOs;
using TaskManager.API.Services;
using TaskManager.AppCore.Services;

namespace TaskManager.API.Controllers;

/// <summary>
/// SRP: This controller only handles HTTP concerns (routing, status codes, DTO mapping).
/// All auth business logic (password hashing, user existence checks) lives in IAuthService.
/// DIP: Depends on IAuthService (Application abstraction) not on IUserRepository (Infrastructure).
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService         _authService;
    private readonly IJwtService          _jwtService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IAuthService authService, IJwtService jwtService, ILogger<AuthController> logger)
    {
        _authService = authService;
        _jwtService  = jwtService;
        _logger      = logger;
    }

    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register([FromBody] RegisterRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Username))
        {
            return BadRequest(new { message = "Username is required" });
        }

        if (string.IsNullOrWhiteSpace(request.Password))
        {
            return BadRequest(new { message = "Password is required" });
        }
        
        _logger.LogInformation("Registration attempt for {Username}", request.Username);

        var result = await _authService.RegisterAsync(request.Username, request.Password);

        if (!result.Success)
        {
            _logger.LogWarning("Registration failed for {Username}: {Error}", request.Username, result.Error);
            return BadRequest(new { message = result.Error });
        }

        _logger.LogInformation("User registered: {Username}", request.Username);
        var token = _jwtService.GenerateToken(result.UserId, result.Username);

        return Ok(new AuthResponse
        {
            Token    = token,
            Username = result.Username,
            UserId   = result.UserId
        });
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest request)
    {
        _logger.LogInformation("Login attempt for {Username}", request.Username);

        var result = await _authService.LoginAsync(request.Username, request.Password);

        if (!result.Success)
        {
            _logger.LogWarning("Login failed for {Username}: {Error}", request.Username, result.Error);
            return Unauthorized(new { message = result.Error });
        }

        _logger.LogInformation("User logged in: {Username}", request.Username);
        var token = _jwtService.GenerateToken(result.UserId, result.Username);

        return Ok(new AuthResponse
        {
            Token    = token,
            Username = result.Username,
            UserId   = result.UserId
        });
    }
}

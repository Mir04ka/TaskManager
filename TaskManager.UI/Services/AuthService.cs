using Microsoft.Extensions.Logging;

namespace TaskManager.UI.Services;

public class AuthService : IAuthService
{
    private readonly ITokenStorage _tokenStorage;
    private readonly IApiClient _apiClient;
    private readonly ILogger<AuthService> _logger;
    
    public bool IsAuthenticated { get; private set; }
    public string? CurrentUsername { get; private set; }
    public Guid? CurrentUserId { get; private set; }

    public AuthService(ITokenStorage tokenStorage, IApiClient apiClient, ILogger<AuthService> logger)
    {
        _tokenStorage = tokenStorage;
        _apiClient = apiClient;
        _logger = logger;
    }

    public async Task<bool> TryAutoLoginAsync()
    {
        _logger.LogInformation("Auto-login attempt");

        var (token, username, userId) = _tokenStorage.GetToken();

        if (string.IsNullOrEmpty(token))
        {
            _logger.LogInformation("No token found");
            return false;
        }
        
        _logger.LogInformation("Found token for {Username}", username);
        
        _apiClient.SetToken(token);

        var isValid = await _apiClient.ValidateTokenAsync();

        if (isValid)
        {
            _logger.LogInformation("Token is valid, auto-login successful");
            IsAuthenticated = true;
            CurrentUsername = username;
            CurrentUserId = userId;
            return true;
        }
        else
        {
            _logger.LogWarning("Saved token is invalid, deleting...");
            _tokenStorage.DeleteToken();
            return false;
        }
    }
    
    public Task LoginAsync(string token, string username, Guid userId)
    {
        _tokenStorage.SaveToken(token, username, userId);
        _apiClient.SetToken(token);
        
        IsAuthenticated = true;
        CurrentUsername = username;
        CurrentUserId = userId;

        _logger.LogInformation("User {Username} logged in", username);
        
        return Task.CompletedTask;
    }

    public Task LogoutAsync(string token, string username, Guid userId)
    {
        _tokenStorage.SaveToken(token, username, userId);
        _apiClient.SetToken(token);

        IsAuthenticated = true;
        CurrentUsername = username;
        CurrentUserId = userId;
        
        _logger.LogInformation("User {Username} logged in", username);
        
        return Task.CompletedTask;
    }

    public void Logout()
    {
        _tokenStorage.DeleteToken();
        
        IsAuthenticated = false;
        CurrentUsername = null;
        CurrentUserId = null;
        
        _logger.LogInformation("User logged out");
    }
}
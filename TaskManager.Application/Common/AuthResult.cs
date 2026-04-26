namespace TaskManager.Application.Common;

public class AuthResult
{
    public bool Success { get; init; }
    public Guid UserId { get; init; }
    public string Username { get; init; } = string.Empty;
    public string? Error { get; init; }
    
    public static AuthResult Ok(Guid userId, string username) => 
        new() { Success = true, UserId = userId, Username = username };
    
    public static AuthResult Fail(string error) =>
        new() { Success = false, Error = error };
}
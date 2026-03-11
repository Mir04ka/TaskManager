namespace TaskManager.API.Services;

public interface IJwtService
{
    string GenerateToken(Guid userId, string username);
}
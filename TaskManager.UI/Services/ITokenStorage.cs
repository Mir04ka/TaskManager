namespace TaskManager.UI.Services;

public interface ITokenStorage
{
    void SaveToken(string token, string username, Guid userId);
    (string? token, string? username, Guid? userId) GetToken();
    void DeleteToken();
    bool HasToken();
}
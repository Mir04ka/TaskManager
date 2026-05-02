using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace TaskManager.WinUI.Services;

public class TokenStorage : ITokenStorage
{
    private readonly ILogger<TokenStorage> _logger;
    private readonly string _tokenFilePath;

    public TokenStorage(ILogger<TokenStorage> logger)
    {
        _logger = logger;
        var appDataPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "TaskManager");
        
        Directory.CreateDirectory(appDataPath);
        _tokenFilePath = Path.Combine(appDataPath, "token.jwt");
    }

    public void SaveToken(string token, string username, Guid userId)
    {
        try
        {
            var data = new TokenData
            {
                Token = token,
                Username = username,
                UserId = userId,
                SavedAt = DateTime.UtcNow
            };
            
            var json = JsonSerializer.Serialize(data);

            var encryptedData = ProtectedData.Protect(
                Encoding.UTF8.GetBytes(json),
                null,
                DataProtectionScope.CurrentUser);
            
            File.WriteAllBytes(_tokenFilePath, encryptedData);
            _logger.LogInformation($"Token saved to {_tokenFilePath} for user {username}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to save token to {_tokenFilePath}");
        }
    }

    public (string? token, string? username, Guid? userId) GetToken()
    {
        try
        {
            if (!File.Exists(_tokenFilePath))
                return (null, null, null);
            
            var encryptedData = File.ReadAllBytes(_tokenFilePath);

            var decryptedData = ProtectedData.Unprotect(
                encryptedData,
                null,
                DataProtectionScope.CurrentUser);
            
            var json = Encoding.UTF8.GetString(decryptedData);
            var data = JsonSerializer.Deserialize<TokenData>(json);
            
            if (data == null)
                return (null, null, null);

            if ((DateTime.UtcNow - data.SavedAt).TotalMinutes > 60)
            {
                _logger.LogInformation($"Token expired");
                DeleteToken();
                return (null, null, null);
            }
            
            return (data.Token, data.Username, data.UserId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to load token from {_tokenFilePath}");
            return (null, null, null);
        }
    }

    public void DeleteToken()
    {
        try
        {
            if (File.Exists(_tokenFilePath))
            {
                File.Delete(_tokenFilePath);
                _logger.LogInformation($"Token deleted");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete token");
        }
    }

    public bool HasToken()
    {
        return File.Exists(_tokenFilePath);
    }
    
    private class TokenData
    {
        public string Token { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public Guid UserId { get; set; }
        public DateTime SavedAt { get; set; }
    }
}
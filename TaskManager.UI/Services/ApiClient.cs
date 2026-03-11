using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using TaskManager.UI.Models;

namespace TaskManager.UI.Services;

public class ApiClient : IApiClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ApiClient> _logger;

    public ApiClient(HttpClient httpClient, ILogger<ApiClient> logger)
    {
        _httpClient = httpClient;
        _httpClient.BaseAddress = new Uri("http://localhost:5217/api/"); // http only
        _logger = logger;
    }

    public void SetToken(string token)
    {
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    public async Task<AuthResponse?> LoginAsync(string username, string password)
    {
        try
        {
            var request = new LoginRequest { Username = username, Password = password };
            var response = await _httpClient.PostAsJsonAsync("auth/login", request);

            if (response.IsSuccessStatusCode)
            {
                var authResponse = await response.Content.ReadFromJsonAsync<AuthResponse>();
                if (authResponse != null)
                {
                    SetToken(authResponse.Token);
                }
                return authResponse;
            }

            _logger.LogWarning("Login failed with status {StatusCode}", response.StatusCode);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login");
            return null;
        }
    }
    
    public async Task<AuthResponse?> RegisterAsync(string username, string password)
    {
        try
        {
            var request = new RegisterRequest { Username = username, Password = password };
            var response = await _httpClient.PostAsJsonAsync("auth/register", request);

            if (response.IsSuccessStatusCode)
            {
                var authResponse = await response.Content.ReadFromJsonAsync<AuthResponse>();
                if (authResponse != null)
                {
                    SetToken(authResponse.Token);
                }
                return authResponse;
            }

            _logger.LogWarning("Registration failed with status {StatusCode}", response.StatusCode);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during registration");
            return null;
        }
    }

    public async Task<List<TaskItemDto>> GetTasksAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync("tasks");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<List<TaskItemDto>>() ?? new List<TaskItemDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting tasks");
            return new List<TaskItemDto>();
        }
    }

    public async Task<TaskItemDto?> CreateTaskAsync(string title, string description)
    {
        try
        {
            var request = new CreateTaskRequest { Title = title, Description = description };
            var response = await _httpClient.PostAsJsonAsync("tasks", request);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<TaskItemDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating task");
            return null;
        }
    }

    public async Task UpdateTaskAsync(Guid id, string title, string description)
    {
        try
        {
            var request = new UpdateTaskRequest { Title = title, Description = description };
            var response = await _httpClient.PutAsJsonAsync($"tasks/{id}", request);
            response.EnsureSuccessStatusCode();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating task {TaskId}", id);
            throw;
        }
    }

    public async Task DeleteTaskAsync(Guid id)
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"tasks/{id}");
            response.EnsureSuccessStatusCode();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting task {TaskId}", id);
            throw;
        }
    }
}
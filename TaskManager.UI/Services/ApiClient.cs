using System.Net.Http;
using System.Net.Http.Headers;
using Microsoft.Extensions.Logging;
using RestEase;
using Polly;
using Polly.Retry;
using TaskManager.Application.Common;
using TaskManager.UI.Models;

namespace TaskManager.UI.Services;

public class ApiClient : IApiClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ApiClient> _logger;
    private readonly ITasksApi _api;
    private readonly AsyncRetryPolicy _retryPolicy = Policy
        .Handle<ApiException>()
        .WaitAndRetryAsync(3, attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)));

    public ApiClient(HttpClient httpClient, ILogger<ApiClient> logger)
    {
        _httpClient = httpClient;
        _httpClient.BaseAddress = new Uri("http://localhost:5217/api/"); // http only
        _logger = logger;
        _api = RestClient.For<ITasksApi>(_httpClient);
    }

    public void SetToken(string token)
    {
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    private async Task<T> ExecuteWithRetryAsync<T>(Func<Task<T>> action)
    {
        return await _retryPolicy.ExecuteAsync(action);
    }

    private async Task ExecuteWithRetryAsync(Func<Task> action)
    {
        await _retryPolicy.ExecuteAsync(action);
    }
    
    public async Task<bool> ValidateTokenAsync()
    {
        try
        {
            // Light call to validate token (existing endpoint semantics preserved)
            var _ = await ExecuteWithRetryAsync(() => _api.GetTasksAsync(1, 1));
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<AuthResponse?> LoginAsync(string username, string password)
    {
        try
        {
            var request = new LoginRequest { Username = username, Password = password };
            var resp = await ExecuteWithRetryAsync(() => _api.LoginAsync(request));
            if (resp != null)
            {
                SetToken(resp.Token);
            }
            return resp;
        }
        catch (ApiException ex)
        {
            _logger.LogWarning("Login failed with status {StatusCode}", ex.StatusCode);
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
            var resp = await ExecuteWithRetryAsync(() => _api.RegisterAsync(request));
            if (resp != null)
            {
                SetToken(resp.Token);
            }
            return resp;
        }
        catch (ApiException ex)
        {
            _logger.LogWarning("Registration failed with status {StatusCode}", ex.StatusCode);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during registration");
            return null;
        }
    }

    public async Task<PagedResult<TaskItemDto>> GetTasksAsync(int pageNumber, int pageSize)
    {
        try
        {
            var result = await ExecuteWithRetryAsync(() => _api.GetTasksAsync(pageNumber, pageSize));
            return result;
        }
        catch (ApiException ex)
        {
            _logger.LogWarning("GetTasks failed with status {StatusCode}", ex.StatusCode);
            return new PagedResult<TaskItemDto> { Items = new List<TaskItemDto>(), TotalCount = 0, PageNumber = pageNumber, PageSize = pageSize };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting tasks");
            return new PagedResult<TaskItemDto> { Items = new List<TaskItemDto>(), TotalCount = 0, PageNumber = pageNumber, PageSize = pageSize };
        }
    }

    public async Task<TaskItemDto?> CreateTaskAsync(string title, string description)
    {
        try
        {
            var request = new CreateTaskRequest { Title = title, Description = description };
            var result = await ExecuteWithRetryAsync(() => _api.CreateTaskAsync(request));
            return result;
        }
        catch (ApiException ex)
        {
            _logger.LogWarning("CreateTask failed with status {StatusCode}", ex.StatusCode);
            return null;
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
            await ExecuteWithRetryAsync(() => _api.UpdateTaskAsync(id, request));
        }
        catch (ApiException ex)
        {
            _logger.LogWarning("UpdateTask failed with status {StatusCode}", ex.StatusCode);
            throw;
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
            await ExecuteWithRetryAsync(() => _api.DeleteTaskAsync(id));
        }
        catch (ApiException ex)
        {
            _logger.LogWarning("DeleteTask failed with status {StatusCode}", ex.StatusCode);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting task {TaskId}", id);
            throw;
        }
    }
}

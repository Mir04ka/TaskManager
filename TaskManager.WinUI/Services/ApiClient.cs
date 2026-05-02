using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;
using RestEase;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using TaskManager.AppCore.Common;
using TaskManager.WinUI.Models;

namespace TaskManager.WinUI.Services;

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
        _httpClient.BaseAddress = new Uri("http://localhost:5000/api/"); // http only
        _logger = logger;
        _api = RestClient.For<ITasksApi>(_httpClient);
    }

    public void SetToken(string token)
        => _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

    private Task<T> WithRetryAsync<T>(Func<Task<T>> action)
        => _retryPolicy.ExecuteAsync(action);

    private Task WithRetryAsync(Func<Task> action)
        => _retryPolicy.ExecuteAsync(action);
    
    public async Task<bool> ValidateTokenAsync()
    {
        try
        {
            await WithRetryAsync(() => _api.GetTasksAsync(1, 1));
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
            var resp = await WithRetryAsync(() 
                => _api.LoginAsync(new LoginRequest { Username = username, Password = password }));
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
            var resp = await WithRetryAsync(() => _api.RegisterAsync(new RegisterRequest { Username = username, Password = password }));
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
            return await WithRetryAsync(() 
                => _api.GetTasksAsync(pageNumber, pageSize));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting tasks");
            return new PagedResult<TaskItemDto> 
            { 
                Items = new List<TaskItemDto>(), 
                TotalCount = 0, 
                PageNumber = pageNumber, 
                PageSize = pageSize 
            };
        }
    }

    public async Task<TaskItemDto?> CreateTaskAsync(string title, string description)
    {
        try
        {
            var result = await WithRetryAsync(() 
                => _api.CreateTaskAsync(new CreateTaskRequest { Title = title, Description = description }));
            return result;
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
            await WithRetryAsync(() => _api.UpdateTaskAsync(id, new UpdateTaskRequest { Title = title, Description = description }));
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
            await WithRetryAsync(() => _api.DeleteTaskAsync(id));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting task {TaskId}", id);
            throw;
        }
    }
}

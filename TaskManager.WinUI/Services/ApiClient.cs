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

    private Task<T> WithRetryAsync<T>(Func<Task<T>> action)
        => _retryPolicy.ExecuteAsync(action);

    private Task WithRetryAsync(Func<Task> action)
        => _retryPolicy.ExecuteAsync(action);

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

    public void SetToken(string token)
        => _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

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

    public async Task<PagedResult<TaskItemDto>> GetTasksAsync(int pageNumber, int pageSize)
    {
        try
        {
            return await WithRetryAsync(() 
                => _api.GetTasksAsync(pageNumber, pageSize));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetTasks error");
            return new PagedResult<TaskItemDto> 
            { 
                Items = new List<TaskItemDto>(), 
                TotalCount = 0, 
                PageNumber = pageNumber, 
                PageSize = pageSize 
            };
        }
    }

    public async Task<TaskItemDto?> GetTaskByIdAsync(Guid id)
    {
        try
        {
            return await WithRetryAsync(() => _api.GetTaskByIdAsync(id));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetTaskById error");
            return null;
        }
    }

    public async Task<TaskItemDto?> CreateTaskAsync(CreateTaskRequest request)
    {
        try
        {
            return await WithRetryAsync(() => _api.CreateTaskAsync(request));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "CreateTask error");
            return null;
        }
    }

    public async Task UpdateTaskAsync(Guid taskId, UpdateTaskRequest request)
    {
        try
        {
            await WithRetryAsync(() => _api.UpdateTaskAsync(taskId, request));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "UpdateTask error {TaskId}", request.Id);
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
            _logger.LogError(ex, "DeleteTask error {TaskId}", id);
            throw;
        }
    }

    public async Task AssignTaskAsync(Guid taskId, AssignTaskRequest request)
    {
        try
        {
            await WithRetryAsync(() => _api.AssignTaskAsync(taskId, request));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "AssignTask error");
            throw;
        }
    }

    public async Task AddRemarkAsync(Guid taskId, AddRemarkRequest request)
    {
        try
        {
            await WithRetryAsync(() => _api.AddRemarkAsync(taskId, request));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "AddRemark error");
            throw;
        }
    }

    public async Task<List<RemarkDto>> GetRemarksAsync(Guid taskId)
    {
        try
        {
            return await WithRetryAsync(() => _api.GetRemarkAsync(taskId));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetRemarks error");
            return new List<RemarkDto>();
        }
    }

    public async Task AddTagToTaskAsync(Guid taskId, Guid tagId)
    {
        try
        {
            await WithRetryAsync(() => _api.AddTagToTaskAsync(taskId, tagId));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "AddTag error");
            throw;
        }
    }

    public async Task RemoveTagFromTaskAsync(Guid taskId, Guid tagId)
    {
        try
        {
            await WithRetryAsync(() => _api.RemoveTagFromTaskAsync(taskId, tagId));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "RemoveTag error");
            throw;
        }
    }

    public async Task<PagedResult<ProcessDto>> GetMyProcessesAsync(int pageNumber, int pageSize)
    {
        try
        {
            return await WithRetryAsync(() => _api.GetMyProcessesAsync(pageNumber, pageSize));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetMyProcesses error");
            throw;
        }
    }

    public async Task<ProcessDto?> CreateProcessAsync(CreateProcessRequest request)
    {
        try
        {
            return await WithRetryAsync(() => _api.CreateProcessAsync(request));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "CreateProcess error");
            return null;
        }
    }
}

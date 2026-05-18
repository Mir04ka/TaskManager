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

    private Task<T> WithRetryAsync<T>(Func<Task<T>> action) => _retryPolicy.ExecuteAsync(action);

    private Task WithRetryAsync(Func<Task> action) => _retryPolicy.ExecuteAsync(action);

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

    public async Task<PagedResult<TaskItemDto>> GetMyTasksAsync(int pageNumber, int pageSize)
    {
        try
        {
            return await WithRetryAsync(() 
                => _api.GetMyTasksAsync(pageNumber, pageSize));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetMyTasks error");
            return Empty<TaskItemDto>(pageNumber, pageSize);
        }
    }
    public async Task<PagedResult<TaskItemDto>> GetProcessTasksAsync(Guid processId, int pageNumber, int pageSize)
    {
        try { 
            return await WithRetryAsync(() 
                => _api.GetProcessTasksAsync(processId, pageNumber, pageSize)); 
        }
        catch (Exception ex) 
        { 
            _logger.LogError(ex, "GetProcessTasks error"); 
            return Empty<TaskItemDto>(pageNumber, pageSize); 
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
            _logger.LogError(ex, "UpdateTask error");
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
            _logger.LogError(ex, "DeleteTask error");
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

    public async Task ChangeStatusAsync(Guid taskId, string status)
    {
        try 
        { 
            await WithRetryAsync(()
                => _api.ChangeStatusAsync(taskId, new ChangeStatusRequest 
                { 
                    Status = status 
                })); 
        }
        catch (Exception ex) 
        { 
            _logger.LogError(ex, "ChangeStatus error"); 
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
            return await WithRetryAsync(() => _api.GetRemarksAsync(taskId));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetRemarks error");
            return new();
        }
    }

    public async Task DeleteRemarkAsync(Guid taskId, Guid remarkId)
    {
        try 
        { 
            await WithRetryAsync(() => _api.DeleteRemarkAsync(taskId, remarkId)); 
        }
        catch (Exception ex) 
        { 
            _logger.LogError(ex, "DeleteRemark error"); 
            throw; 
        }
    }

    public async Task AddTagToTaskAsync(Guid taskId, Guid tagId)
    {
        try
        {
            await WithRetryAsync(() 
                => _api.AddTagToTaskAsync(taskId, new TaskTagRequest
                {
                    TagId = tagId
                }));
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

    public async Task<List<TagDto>> GetTagsAsync(Guid processId)
    {
        try 
        { 
            return await WithRetryAsync(() => _api.GetTagsAsync(processId)); 
        }
        catch (Exception ex) 
        { 
            _logger.LogError(ex, "GetTags error"); 
            return new(); 
        }
    }

    public async Task<TagDto?> CreateTagAsync(Guid processId, string name)
    {
        try 
        { 
            return await WithRetryAsync(() 
                => _api.CreateTagAsync(processId, new CreateTagRequest 
                { 
                    Name = name 
                })); 
        }
        catch (Exception ex) 
        { 
            _logger.LogError(ex, "CreateTag error"); 
            return null; 
        }
    }

    public async Task DeleteTagAsync(Guid processId, Guid tagId)
    {
        try 
        { 
            await WithRetryAsync(() => _api.DeleteTagAsync(processId, tagId)); 
        }
        catch (Exception ex) 
        { 
            _logger.LogError(ex, "DeleteTag error"); 
            throw; 
        }
    }

    public async Task RenameProcessAsync(Guid processId, string name)
    {
        try { await WithRetryAsync(() => _api.RenameProcessAsync(processId, new CreateProcessRequest { Name = name })); }
        catch (Exception ex) { _logger.LogError(ex, "RenameProcess error"); throw; }
    }

    public async Task DeleteProcessAsync(Guid processId)
    {
        try { await WithRetryAsync(() => _api.DeleteProcessAsync(processId)); }
        catch (Exception ex) { _logger.LogError(ex, "DeleteProcess error"); throw; }
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
            return Empty<ProcessDto>(pageNumber, pageSize);
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

    public async Task<string?> GetMyRoleAsync(Guid processId)
    {
        try 
        { 
            return await WithRetryAsync(() => _api.GetMyRoleAsync(processId)); 
        }
        catch (Exception ex) 
        { 
            _logger.LogError(ex, "GetMyRole error"); 
            return null; 
        }
    }

    public async Task<List<ProcessMemberDto>> GetProcessMembersAsync(Guid processId)
    {
        try 
        { 
            return await WithRetryAsync(() => _api.GetProcessMembersAsync(processId)); 
        }
        catch (Exception ex) 
        { 
            _logger.LogError(ex, "GetProcessMembers error"); 
            return new(); 
        }
    }

    private static PagedResult<T> Empty<T>(int pageNumber, int pageSize) => new()
    {
        Items = new List<T>(),
        TotalCount = 0,
        PageNumber = pageNumber,
        PageSize = pageSize
    };
}

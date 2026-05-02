using System.Security.Claims;
using TaskManager.AppCore.Services;

namespace TaskManager.API.Services;

public class HttpCurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public HttpCurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid? CurrentUserId
    {
        get
        {
            var value = _httpContextAccessor.HttpContext?
                .User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            
            return value is not null ? new Guid(value) : null;
        }
        
        set {}
    }
    
    public string? CurrentUsername
    {
        get => _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Name)?.Value;
        set {}
    }
}
using Shared.Models;
using System.Security.Claims;
using Task11_DotNETBackendWebApi.Services.Contracts;

namespace Task11_DotNETBackendWebApi.Services;

public class UserContext : IUserContext
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public UserContext(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid UserId
    {
        get
        {
            var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdClaim))
            {
                throw new UnauthorizedAccessException("User is not authenticated");
            }

            return Guid.Parse(userIdClaim);
        }
    }

    public string Username
    {
        get
        {
            return _httpContextAccessor.HttpContext?.User?.Identity?.Name ?? "Guest";
        }
    }

    public bool IsAdmin
    {
        get
        {
            return _httpContextAccessor.HttpContext?.User?.IsInRole(nameof(UserRoles.Admin)) ?? false;
        }
    }
}

using System.Security.Claims;
using HelpDev.Modules.Identity.Application.Auth;
using HelpDev.SharedApplication.Abstractions.Security;
using Microsoft.AspNetCore.Http;

namespace HelpDev.Modules.Identity.Infrastructure.Security;

public sealed class CurrentUser : ICurrentUser
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUser(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    private ClaimsPrincipal? Principal => _httpContextAccessor.HttpContext?.User;

    public Guid? UserId
    {
        get
        {
            var value = Principal?.FindFirstValue(JwtClaimTypes.UserId);
            return Guid.TryParse(value, out var userId) ? userId : null;
        }
    }

    public string? Mobile => Principal?.FindFirstValue(JwtClaimTypes.Mobile);

    public IReadOnlyCollection<string> Roles =>
        Principal?
            .FindAll(JwtClaimTypes.Role)
            .Select(claim => claim.Value)
            .ToArray()
        ?? Array.Empty<string>();

    public bool IsAuthenticated =>
        Principal?.Identity?.IsAuthenticated == true;
}

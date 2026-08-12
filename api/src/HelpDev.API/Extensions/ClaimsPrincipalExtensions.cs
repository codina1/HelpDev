using System.Security.Claims;
using HelpDev.Modules.Identity.Application.Auth;

namespace HelpDev.API.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static Guid? GetUserId(this ClaimsPrincipal user)
    {
        var value = user.FindFirstValue(JwtClaimTypes.UserId);
        return Guid.TryParse(value, out var userId) ? userId : null;
    }
}

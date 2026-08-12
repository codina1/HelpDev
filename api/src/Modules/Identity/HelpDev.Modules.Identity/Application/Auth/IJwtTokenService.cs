using HelpDev.Modules.Identity.Domain.Enums;

namespace HelpDev.Modules.Identity.Application.Auth;

public interface IJwtTokenService
{
    (string Token, int ExpiresInSeconds) GenerateToken(Guid userId, UserRole role, string mobile);
}

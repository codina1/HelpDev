using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using HelpDev.Modules.Identity.Application.Auth;
using HelpDev.Modules.Identity.Domain.Enums;
using HelpDev.Modules.Identity.Infrastructure.Auth;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace HelpDev.Identity.Tests;

public sealed class JwtTokenServiceTests
{
    private static readonly JwtSettings Settings = new()
    {
        Secret = "01234567890123456789012345678901",
        Issuer = "HelpDev",
        Audience = "HelpDev.Client",
        ExpirationMinutes = 60,
    };

    [Fact]
    public void GenerateToken_is_valid_with_configured_signing_key()
    {
        var service = new JwtTokenService(Options.Create(Settings));
        var userId = Guid.Parse("aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee");

        var (token, expiresIn) = service.GenerateToken(userId, UserRole.Writer, "09123456789");

        var principal = ValidateToken(token, Settings.Secret);
        Assert.True(expiresIn > 0);
        Assert.Equal(userId.ToString(), principal.FindFirstValue(JwtClaimTypes.UserId));
        Assert.Equal(AppRoles.Writer, principal.FindFirstValue(JwtClaimTypes.Role));
        Assert.Equal("09123456789", principal.FindFirstValue(JwtClaimTypes.Mobile));
        Assert.Equal(userId.ToString(), principal.FindFirstValue(JwtRegisteredClaimNames.Sub));
        Assert.False(string.IsNullOrWhiteSpace(principal.FindFirstValue(JwtRegisteredClaimNames.Jti)));
    }

    [Fact]
    public void GenerateToken_fails_validation_with_incorrect_signing_key()
    {
        var service = new JwtTokenService(Options.Create(Settings));
        var (token, _) = service.GenerateToken(Guid.NewGuid(), UserRole.User, "09123456789");

        var exception = Record.Exception(() =>
            ValidateToken(token, "xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx"));

        Assert.NotNull(exception);
        Assert.IsAssignableFrom<SecurityTokenException>(exception);
    }

    [Fact]
    public void GenerateToken_embeds_configured_issuer_and_audience()
    {
        var service = new JwtTokenService(Options.Create(Settings));
        var (token, _) = service.GenerateToken(Guid.NewGuid(), UserRole.Admin, "09120000001");

        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(token);

        Assert.Equal(Settings.Issuer, jwt.Issuer);
        Assert.Contains(Settings.Audience, jwt.Audiences);
    }

    [Fact]
    public void GenerateToken_sets_expiration_based_on_settings()
    {
        var service = new JwtTokenService(Options.Create(Settings));
        var before = DateTime.UtcNow;

        var (token, expiresIn) = service.GenerateToken(Guid.NewGuid(), UserRole.User, "09123456789");

        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);
        Assert.True(jwt.ValidTo > before.AddMinutes(Settings.ExpirationMinutes - 1));
        Assert.True(jwt.ValidTo <= before.AddMinutes(Settings.ExpirationMinutes + 1));
        Assert.InRange(expiresIn, (Settings.ExpirationMinutes * 60) - 5, Settings.ExpirationMinutes * 60);
    }

    [Fact]
    public void GenerateToken_uses_custom_claim_type_names()
    {
        Assert.Equal("userId", JwtClaimTypes.UserId);
        Assert.Equal("role", JwtClaimTypes.Role);
        Assert.Equal("mobile", JwtClaimTypes.Mobile);

        var service = new JwtTokenService(Options.Create(Settings));
        var (token, _) = service.GenerateToken(Guid.NewGuid(), UserRole.User, "09123456789");
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);

        Assert.Contains(jwt.Claims, claim => claim.Type == "userId");
        Assert.Contains(jwt.Claims, claim => claim.Type == "role");
        Assert.Contains(jwt.Claims, claim => claim.Type == "mobile");
    }

    private static ClaimsPrincipal ValidateToken(string token, string secret)
    {
        var handler = new JwtSecurityTokenHandler
        {
            MapInboundClaims = false,
        };

        return handler.ValidateToken(
            token,
            new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = Settings.Issuer,
                ValidAudience = Settings.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret)),
                ClockSkew = TimeSpan.FromSeconds(30),
            },
            out _);
    }
}

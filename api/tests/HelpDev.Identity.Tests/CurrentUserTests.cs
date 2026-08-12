using System.Security.Claims;
using HelpDev.Modules.Identity.Application.Auth;
using HelpDev.Modules.Identity.Infrastructure.Security;
using Microsoft.AspNetCore.Http;

namespace HelpDev.Identity.Tests;

public sealed class CurrentUserTests
{
    [Fact]
    public void No_HttpContext_means_unauthenticated_with_empty_claims()
    {
        var accessor = new HttpContextAccessor();
        var currentUser = new CurrentUser(accessor);

        Assert.False(currentUser.IsAuthenticated);
        Assert.Null(currentUser.UserId);
        Assert.Null(currentUser.Mobile);
        Assert.Empty(currentUser.Roles);
    }

    [Fact]
    public void Unauthenticated_principal_is_not_authenticated()
    {
        var currentUser = CreateCurrentUser(new ClaimsPrincipal(new ClaimsIdentity()));

        Assert.False(currentUser.IsAuthenticated);
        Assert.Null(currentUser.UserId);
    }

    [Fact]
    public void Missing_userId_claim_returns_null_UserId()
    {
        var identity = new ClaimsIdentity(
            [new Claim(JwtClaimTypes.Mobile, "09123456789")],
            authenticationType: "Bearer");
        var currentUser = CreateCurrentUser(new ClaimsPrincipal(identity));

        Assert.True(currentUser.IsAuthenticated);
        Assert.Null(currentUser.UserId);
        Assert.Equal("09123456789", currentUser.Mobile);
    }

    [Fact]
    public void Malformed_userId_claim_returns_null_UserId()
    {
        var identity = new ClaimsIdentity(
            [new Claim(JwtClaimTypes.UserId, "not-a-guid")],
            authenticationType: "Bearer");
        var currentUser = CreateCurrentUser(new ClaimsPrincipal(identity));

        Assert.True(currentUser.IsAuthenticated);
        Assert.Null(currentUser.UserId);
    }

    [Fact]
    public void Valid_claims_are_exposed()
    {
        var userId = Guid.Parse("aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee");
        var identity = new ClaimsIdentity(
            [
                new Claim(JwtClaimTypes.UserId, userId.ToString()),
                new Claim(JwtClaimTypes.Mobile, "09123456789"),
                new Claim(JwtClaimTypes.Role, AppRoles.Admin),
            ],
            authenticationType: "Bearer");
        var currentUser = CreateCurrentUser(new ClaimsPrincipal(identity));

        Assert.True(currentUser.IsAuthenticated);
        Assert.Equal(userId, currentUser.UserId);
        Assert.Equal("09123456789", currentUser.Mobile);
        Assert.Equal([AppRoles.Admin], currentUser.Roles);
    }

    [Fact]
    public void Multiple_role_claims_are_returned()
    {
        var identity = new ClaimsIdentity(
            [
                new Claim(JwtClaimTypes.Role, AppRoles.Writer),
                new Claim(JwtClaimTypes.Role, AppRoles.Admin),
            ],
            authenticationType: "Bearer");
        var currentUser = CreateCurrentUser(new ClaimsPrincipal(identity));

        Assert.Equal([AppRoles.Writer, AppRoles.Admin], currentUser.Roles);
    }

    private static CurrentUser CreateCurrentUser(ClaimsPrincipal principal)
    {
        var context = new DefaultHttpContext
        {
            User = principal,
        };
        return new CurrentUser(new HttpContextAccessor { HttpContext = context });
    }
}

using HelpDev.Modules.Identity.Application.Auth;

namespace HelpDev.Identity.Tests;

public sealed class IdentityContractTests
{
    [Fact]
    public void JwtClaimTypes_lock_current_string_contracts()
    {
        Assert.Equal("userId", JwtClaimTypes.UserId);
        Assert.Equal("role", JwtClaimTypes.Role);
        Assert.Equal("mobile", JwtClaimTypes.Mobile);
    }

    [Fact]
    public void AppRoles_lock_current_role_strings()
    {
        Assert.Equal("User", AppRoles.User);
        Assert.Equal("Writer", AppRoles.Writer);
        Assert.Equal("Admin", AppRoles.Admin);
    }

    [Fact]
    public void AuthorizationPolicies_lock_current_policy_names()
    {
        Assert.Equal("Authenticated", AuthorizationPolicies.Authenticated);
        Assert.Equal("WriterOrAdmin", AuthorizationPolicies.WriterOrAdmin);
        Assert.Equal("AdminOnly", AuthorizationPolicies.AdminOnly);
    }
}

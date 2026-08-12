using HelpDev.Modules.Identity.Application.Auth;
using HelpDev.SharedContracts.Auditing;

namespace HelpDev.API.Tests;

public sealed class SecurityApiTests
{
    [Fact]
    public void AuthService_constructor_requires_audit_abstractions()
    {
        var parameters = typeof(AuthService).GetConstructors().Single().GetParameters()
            .Select(parameter => parameter.ParameterType)
            .ToList();

        Assert.Contains(typeof(IAuditRecorder), parameters);
        Assert.Contains(typeof(IAuditRequestContext), parameters);
    }

    [Fact]
    public void Audit_actions_include_authentication_otp_events()
    {
        Assert.True(AuditActions.IsSupported(AuditActions.AuthenticationOtpRequested));
        Assert.True(AuditActions.IsSupported(AuditActions.AuthenticationOtpVerified));
        Assert.True(AuditActions.IsSupported(AuditActions.AuthenticationOtpVerificationFailed));
    }
}

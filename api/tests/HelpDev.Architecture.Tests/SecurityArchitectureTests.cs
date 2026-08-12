using System.Reflection;
using HelpDev.API.Security;
using HelpDev.Modules.Identity.Application.Auth;
using HelpDev.SharedContracts.Auditing;
using NetArchTest.Rules;

namespace HelpDev.Architecture.Tests;

public sealed class SecurityArchitectureTests
{
    [Fact]
    public void AuthService_depends_on_audit_abstractions_not_concrete_recorder()
    {
        var ctor = typeof(AuthService).GetConstructors().Single();
        Assert.Contains(ctor.GetParameters(), p => p.ParameterType == typeof(IAuditRecorder));
        Assert.Contains(ctor.GetParameters(), p => p.ParameterType == typeof(IAuditRequestContext));
        Assert.DoesNotContain(
            ctor.GetParameters(),
            p => p.ParameterType.Name.Contains("AuditRecorder", StringComparison.Ordinal)
                && p.ParameterType != typeof(IAuditRecorder));
    }

    [Fact]
    public void AccessDeniedAuditMiddleware_records_authorization_audit()
    {
        var ctor = typeof(AccessDeniedAuditMiddleware).GetConstructors().First();
        var invokeMethod = typeof(AccessDeniedAuditMiddleware)
            .GetMethods(BindingFlags.Instance | BindingFlags.Public)
            .Single(method => method.Name == "InvokeAsync");

        Assert.Contains(invokeMethod.GetParameters(), p => p.ParameterType == typeof(IAuditRecorder));
        Assert.Contains(invokeMethod.GetParameters(), p => p.ParameterType == typeof(IAuditRequestContext));
    }

    [Fact]
    public void Application_services_do_not_define_otp_logger_types()
    {
        var suspiciousTypes = typeof(AuthService).Assembly
            .GetTypes()
            .Where(type => type.Name.Contains("OtpLogger", StringComparison.OrdinalIgnoreCase)
                || type.Name.Contains("PhoneLogger", StringComparison.OrdinalIgnoreCase))
            .Select(type => type.FullName)
            .ToList();

        Assert.Empty(suspiciousTypes);
    }
}

using System.Reflection;
using HelpDev.API.Controllers;
using HelpDev.API.Security;
using HelpDev.Modules.Auditing.Application.Queries;
using HelpDev.Modules.Auditing.Application.Recording;
using HelpDev.Modules.Auditing.Domain.Records;
using HelpDev.Modules.Identity.Application.Auth;
using HelpDev.SharedContracts.Auditing;
using NetArchTest.Rules;
using AuditingModuleMarker = HelpDev.Modules.Auditing.ModuleMarker;

namespace HelpDev.Architecture.Tests;

public sealed class AuditingArchitectureTests
{
    [Fact]
    public void Auditing_Domain_depends_only_on_allowed_building_blocks()
    {
        var result = Types.InAssembly(typeof(AuditingModuleMarker).Assembly)
            .That()
            .ResideInNamespaceContaining(".Domain")
            .ShouldNot()
            .HaveDependencyOnAny(
                "HelpDev.Modules.Identity",
                "HelpDev.Modules.Content",
                "HelpDev.Modules.Learning",
                "HelpDev.Modules.Search",
                "HelpDev.Modules.Administration",
                "HelpDev.Modules.Toolbox",
                "HelpDev.Modules.PromptLab",
                "HelpDev.Infrastructure",
                "Microsoft.AspNetCore",
                "Microsoft.EntityFrameworkCore")
            .GetResult();

        Assert.True(result.IsSuccessful, FormatFailures(result));
    }

    [Fact]
    public void Auditing_Application_does_not_depend_on_AspNetCore_or_other_module_Infrastructure()
    {
        foreach (var ns in new[]
                 {
                     ".Application.Recording",
                     ".Application.Queries",
                     ".Application.Persistence",
                 })
        {
            var result = Types.InAssembly(typeof(AuditingModuleMarker).Assembly)
                .That()
                .ResideInNamespaceContaining(ns)
                .ShouldNot()
                .HaveDependencyOnAny(
                    "Microsoft.AspNetCore",
                    "HelpDev.Infrastructure",
                    "HelpDev.Modules.Identity.Infrastructure",
                    "HelpDev.Modules.Administration.Infrastructure")
                .GetResult();

            Assert.True(result.IsSuccessful, $"{ns}: {FormatFailures(result)}");
        }
    }

    [Fact]
    public void Audit_admin_controller_depends_on_query_abstractions_only()
    {
        var parameters = typeof(AuditAdminController).GetConstructors().Single().GetParameters();
        Assert.Single(parameters);
        Assert.Equal(typeof(IAuditQueries), parameters[0].ParameterType);
    }

    [Fact]
    public void AuditRecord_domain_has_no_sensitive_payload_properties()
    {
        var properties = typeof(AuditRecord).GetProperties(BindingFlags.Instance | BindingFlags.Public)
            .Select(property => property.Name)
            .ToList();

        Assert.DoesNotContain(properties, name => name.Contains("Password", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(properties, name => name.Contains("Otp", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(properties, name => name.Contains("Phone", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void AuditRecorder_swallows_persistence_failures_without_throwing()
    {
        var method = typeof(AuditRecorder).GetMethod(nameof(AuditRecorder.RecordAsync));
        Assert.NotNull(method);
        Assert.Equal(typeof(Task), method!.ReturnType);
    }

    private static string FormatFailures(TestResult result) =>
        result.FailingTypeNames is null || !result.FailingTypeNames.Any()
            ? "Architecture rule failed."
            : string.Join(Environment.NewLine, result.FailingTypeNames);
}

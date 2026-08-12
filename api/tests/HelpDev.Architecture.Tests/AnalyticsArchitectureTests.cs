using System.Reflection;
using HelpDev.API.Controllers;
using HelpDev.Infrastructure.Outbox;
using HelpDev.Modules.Analytics.Application.ContentAnalytics;
using HelpDev.Modules.Analytics.Application.Queries;
using HelpDev.SharedApplication.Abstractions.Events;
using NetArchTest.Rules;
using AnalyticsModuleMarker = HelpDev.Modules.Analytics.ModuleMarker;
using IdentityModuleMarker = HelpDev.Modules.Identity.ModuleMarker;
using SearchModuleMarker = HelpDev.Modules.Search.ModuleMarker;
using ToolboxModuleMarker = HelpDev.Modules.Toolbox.ModuleMarker;
using PromptLabModuleMarker = HelpDev.Modules.PromptLab.ModuleMarker;
using AdministrationModuleMarker = HelpDev.Modules.Administration.ModuleMarker;

namespace HelpDev.Architecture.Tests;

public sealed class AnalyticsArchitectureTests
{
    [Fact]
    public void Analytics_Domain_depends_only_on_allowed_building_blocks()
    {
        var result = Types.InAssembly(typeof(AnalyticsModuleMarker).Assembly)
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
    public void Analytics_Application_services_do_not_depend_on_AspNetCore_EF_or_other_module_Infrastructure()
    {
        foreach (var ns in new[]
                 {
                     ".Application.Processing",
                     ".Application.Queries",
                     ".Application.Persistence",
                 })
        {
            var result = Types.InAssembly(typeof(AnalyticsModuleMarker).Assembly)
                .That()
                .ResideInNamespaceContaining(ns)
                .ShouldNot()
                .HaveDependencyOnAny(
                    "Microsoft.AspNetCore",
                    "Npgsql",
                    "HelpDev.Modules.Identity.Infrastructure",
                    "HelpDev.Modules.Content.Infrastructure",
                    "HelpDev.Modules.Learning.Infrastructure",
                    "HelpDev.Modules.Search.Infrastructure",
                    "HelpDev.Modules.Administration.Infrastructure",
                    "HelpDev.Modules.Toolbox.Infrastructure",
                    "HelpDev.Modules.PromptLab.Infrastructure",
                    "HelpDev.Infrastructure")
                .GetResult();

            Assert.True(result.IsSuccessful, $"{ns}: {FormatFailures(result)}");
        }
    }

    [Fact]
    public void Other_modules_do_not_depend_on_Analytics_module()
    {
        AssertNoDependency(typeof(IdentityModuleMarker).Assembly, "HelpDev.Modules.Analytics");
        AssertNoDependency(typeof(SearchModuleMarker).Assembly, "HelpDev.Modules.Analytics");
        AssertNoDependency(typeof(ToolboxModuleMarker).Assembly, "HelpDev.Modules.Analytics");
        AssertNoDependency(typeof(PromptLabModuleMarker).Assembly, "HelpDev.Modules.Analytics");
        AssertNoDependency(typeof(AdministrationModuleMarker).Assembly, "HelpDev.Modules.Analytics");
    }

    [Fact]
    public void Analytics_admin_controller_requires_admin_policy_and_depends_on_query_abstractions()
    {
        var ctor = typeof(AnalyticsAdminController).GetConstructors().Single();
        Assert.Contains(ctor.GetParameters(), p => p.ParameterType == typeof(IAnalyticsOverviewQueries));
        Assert.Contains(ctor.GetParameters(), p => p.ParameterType == typeof(IContentAnalyticsQueries));
        Assert.DoesNotContain(
            ctor.GetParameters(),
            p => p.ParameterType.Name.Contains("Repository", StringComparison.Ordinal)
                || p.ParameterType.Name.Contains("DbContext", StringComparison.Ordinal));
    }

    [Fact]
    public void Analytics_does_not_depend_on_Content_Infrastructure()
    {
        var result = Types.InAssembly(typeof(AnalyticsModuleMarker).Assembly)
            .ShouldNot()
            .HaveDependencyOn("HelpDev.Modules.Content.Infrastructure")
            .GetResult();

        Assert.True(result.IsSuccessful, FormatFailures(result));
    }

    [Fact]
    public void Content_analytics_domain_has_no_score_properties()
    {
        var names = typeof(HelpDev.Modules.Analytics.Domain.ContentAnalytics.ContentAnalyticsSnapshot)
            .GetProperties()
            .Select(p => p.Name)
            .Concat(typeof(HelpDev.Modules.Analytics.Domain.ContentAnalytics.ContentHealthResult)
                .GetProperties()
                .Select(p => p.Name))
            .ToArray();

        Assert.DoesNotContain(names, n => n.Contains("Score", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(names, n => n.Contains("Rank", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Analytics_has_no_public_ingestion_controller()
    {
        var controllers = typeof(AnalyticsAdminController).Assembly
            .GetTypes()
            .Where(t => t.Name.Contains("Analytics", StringComparison.OrdinalIgnoreCase)
                        && t.Name.Contains("Ingest", StringComparison.OrdinalIgnoreCase))
            .ToList();

        Assert.Empty(controllers);
    }

    [Fact]
    public void Analytics_domain_has_no_IpAddress_UserAgent_or_SearchQuery_properties()
    {
        var domainTypes = typeof(AnalyticsModuleMarker).Assembly
            .GetTypes()
            .Where(t => t.Namespace?.Contains(".Domain") == true)
            .ToList();

        var sensitiveProperties = domainTypes
            .SelectMany(t => t.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            .Where(p =>
                p.Name.Contains("IpAddress", StringComparison.OrdinalIgnoreCase)
                || p.Name.Contains("UserAgent", StringComparison.OrdinalIgnoreCase)
                || p.Name.Contains("SearchQuery", StringComparison.OrdinalIgnoreCase))
            .Select(p => $"{p.DeclaringType!.Name}.{p.Name}")
            .ToList();

        Assert.Empty(sensitiveProperties);
    }

    [Fact]
    public void Analytics_application_does_not_write_OutboxMessage_or_dispatch_events()
    {
        var result = Types.InAssembly(typeof(AnalyticsModuleMarker).Assembly)
            .That()
            .ResideInNamespaceContaining(".Application")
            .ShouldNot()
            .HaveDependencyOnAny(
                typeof(OutboxMessage).FullName!,
                typeof(IDomainEventDispatcher).FullName!)
            .GetResult();

        Assert.True(result.IsSuccessful, FormatFailures(result));
    }

    [Fact]
    public void Analytics_has_no_OpenAI_external_ML_dependency()
    {
        var assembly = typeof(AnalyticsModuleMarker).Assembly;
        var referenced = assembly.GetReferencedAssemblies().Select(a => a.Name ?? string.Empty).ToList();

        Assert.DoesNotContain(referenced, name => name.Contains("OpenAI", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(referenced, name => name.Contains("Anthropic", StringComparison.OrdinalIgnoreCase));
    }

    private static void AssertNoDependency(Assembly assembly, string dependency)
    {
        var result = Types.InAssembly(assembly)
            .ShouldNot()
            .HaveDependencyOn(dependency)
            .GetResult();

        Assert.True(result.IsSuccessful, FormatFailures(result));
    }

    private static string FormatFailures(TestResult result) =>
        result.FailingTypeNames is null || !result.FailingTypeNames.Any()
            ? "Architecture rule failed."
            : string.Join(Environment.NewLine, result.FailingTypeNames);
}

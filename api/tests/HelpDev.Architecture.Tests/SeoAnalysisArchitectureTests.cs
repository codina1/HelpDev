using System.Reflection;
using HelpDev.Modules.Content.Application.SeoAnalysis;
using HelpDev.Modules.Content.Application.SeoAnalysis.Rules;
using NetArchTest.Rules;
using ContentModuleMarker = HelpDev.Modules.Content.ModuleMarker;

namespace HelpDev.Architecture.Tests;

public sealed class SeoAnalysisArchitectureTests
{
    [Fact]
    public void SeoAnalysis_types_do_not_depend_on_EF_or_AspNetCore_or_Infrastructure()
    {
        var result = Types.InAssembly(typeof(ContentModuleMarker).Assembly)
            .That()
            .ResideInNamespaceStartingWith("HelpDev.Modules.Content.Application.SeoAnalysis")
            .ShouldNot()
            .HaveDependencyOnAny(
                "Microsoft.EntityFrameworkCore",
                "Microsoft.AspNetCore",
                "HelpDev.Modules.Content.Infrastructure",
                "HelpDev.Infrastructure")
            .GetResult();

        Assert.True(result.IsSuccessful, FormatFailures(result));
    }

    [Fact]
    public void SeoAnalysis_has_no_AI_or_external_SEO_SDK_references()
    {
        var assembly = typeof(ContentModuleMarker).Assembly;
        var referenced = assembly.GetReferencedAssemblies().Select(a => a.Name ?? string.Empty).ToList();

        Assert.DoesNotContain(referenced, name => name.Contains("OpenAI", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(referenced, name => name.Contains("Anthropic", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(referenced, name => name.Contains("Azure.AI", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(referenced, name => name.Contains("SeoToolkit", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(referenced, name => name.Contains("Google.Apis", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void SeoAnalysis_does_not_depend_on_Search_module()
    {
        var result = Types.InAssembly(typeof(ContentModuleMarker).Assembly)
            .That()
            .ResideInNamespaceStartingWith("HelpDev.Modules.Content.Application.SeoAnalysis")
            .ShouldNot()
            .HaveDependencyOn("HelpDev.Modules.Search")
            .GetResult();

        Assert.True(result.IsSuccessful, FormatFailures(result));
    }

    [Fact]
    public void ContentManagementController_does_not_take_DbContext()
    {
        var ctor = typeof(HelpDev.API.Controllers.ContentManagementController).GetConstructors().Single();
        Assert.DoesNotContain(
            ctor.GetParameters(),
            p => p.ParameterType.Name.Contains("DbContext", StringComparison.Ordinal)
                 || p.ParameterType.Namespace?.StartsWith("Microsoft.EntityFrameworkCore", StringComparison.Ordinal) == true);
    }

    [Fact]
    public void SeoAuditReportDto_contains_no_Domain_types()
    {
        Assert.DoesNotContain(
            typeof(SeoAuditReportDto).GetProperties(BindingFlags.Public | BindingFlags.Instance),
            p => p.PropertyType.Namespace?.StartsWith("HelpDev.Modules.Content.Domain", StringComparison.Ordinal) == true);
        Assert.DoesNotContain(
            typeof(SeoAuditFindingDto).GetProperties(BindingFlags.Public | BindingFlags.Instance),
            p => p.PropertyType.Namespace?.StartsWith("HelpDev.Modules.Content.Domain", StringComparison.Ordinal) == true);
    }

    [Fact]
    public void Rules_have_no_HttpClient_or_network_client_dependency()
    {
        var result = Types.InAssembly(typeof(ContentModuleMarker).Assembly)
            .That()
            .ResideInNamespace("HelpDev.Modules.Content.Application.SeoAnalysis.Rules")
            .ShouldNot()
            .HaveDependencyOnAny("System.Net.Http", "System.Net.Http.HttpClient")
            .GetResult();

        Assert.True(result.IsSuccessful, FormatFailures(result));
    }

    [Fact]
    public void Default_rules_are_explicitly_registered_without_reflection_plugins()
    {
        var rules = ContentSeoAnalyzer.CreateDefaultRules();
        Assert.NotEmpty(rules);
        Assert.All(rules, r => Assert.False(string.IsNullOrWhiteSpace(r.RuleId)));
        Assert.Contains(rules, r => r is SeoTitleExistsRule);
        Assert.Contains(rules, r => r is InternalLinksPresenceRule);
        Assert.Contains(rules, r => r is SeoMetadataValidityRule);
    }

    [Fact]
    public void Audit_report_model_exposes_no_seo_score_or_percentage()
    {
        var names = typeof(SeoAuditReportDto).GetProperties().Select(p => p.Name)
            .Concat(typeof(SeoAuditSummaryDto).GetProperties().Select(p => p.Name))
            .ToArray();

        Assert.DoesNotContain(names, n => n.Contains("Score", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(names, n => n.Contains("Percent", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(names, n => n.Contains("Rank", StringComparison.OrdinalIgnoreCase));
    }

    private static string FormatFailures(TestResult result) =>
        result.FailingTypeNames is null || !result.FailingTypeNames.Any()
            ? "Architecture rule failed."
            : string.Join(Environment.NewLine, result.FailingTypeNames);
}

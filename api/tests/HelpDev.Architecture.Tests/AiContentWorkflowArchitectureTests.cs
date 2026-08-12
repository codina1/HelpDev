using HelpDev.API.Controllers;
using HelpDev.Modules.Content;
using HelpDev.Modules.Search;
using NetArchTest.Rules;
using ContentModuleMarker = HelpDev.Modules.Content.ModuleMarker;
using SearchModuleMarker = HelpDev.Modules.Search.ModuleMarker;

namespace HelpDev.Architecture.Tests;

public sealed class AiContentWorkflowArchitectureTests
{
    [Fact]
    public void Content_Application_does_not_reference_Search_Infrastructure_or_AI_SDKs()
    {
        var result = Types.InAssembly(typeof(ContentModuleMarker).Assembly)
            .That()
            .ResideInNamespaceStartingWith("HelpDev.Modules.Content.Application")
            .ShouldNot()
            .HaveDependencyOnAny(
                "HelpDev.Modules.Search.Infrastructure",
                "HelpDev.Infrastructure.Search",
                "HelpDev.Infrastructure.Ai",
                "OpenAI",
                "Anthropic",
                "Pgvector")
            .GetResult();

        Assert.True(result.IsSuccessful, FormatFailures(result));
    }

    [Fact]
    public void Workflow_controller_does_not_take_AI_provider_or_DbContext()
    {
        var ctor = typeof(ContentWorkflowEngineController).GetConstructors().Single();
        Assert.DoesNotContain(
            ctor.GetParameters(),
            p => p.ParameterType.Name.Contains("DbContext", StringComparison.Ordinal)
                 || p.ParameterType.Name.Contains("Generator", StringComparison.Ordinal)
                 || p.ParameterType.Namespace?.Contains("Pgvector", StringComparison.Ordinal) == true
                 || p.ParameterType.Namespace?.Contains("Infrastructure.Ai", StringComparison.Ordinal) == true);
    }

    [Fact]
    public void Search_Application_does_not_reference_Content_Infrastructure()
    {
        var result = Types.InAssembly(typeof(SearchModuleMarker).Assembly)
            .That()
            .ResideInNamespaceStartingWith("HelpDev.Modules.Search.Application")
            .ShouldNot()
            .HaveDependencyOnAny(
                "HelpDev.Modules.Content.Infrastructure",
                "HelpDev.Infrastructure.Content")
            .GetResult();

        Assert.True(result.IsSuccessful, FormatFailures(result));
    }

    private static string FormatFailures(TestResult result) =>
        result.FailingTypeNames is null || !result.FailingTypeNames.Any()
            ? "Architecture rule failed."
            : string.Join(Environment.NewLine, result.FailingTypeNames);
}

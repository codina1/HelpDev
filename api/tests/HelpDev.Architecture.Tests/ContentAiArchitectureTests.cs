using System.Reflection;
using HelpDev.Infrastructure.Ai;
using HelpDev.Modules.Content.Application.ContentAi;
using HelpDev.SharedContracts.Ai;
using NetArchTest.Rules;
using ContentModuleMarker = HelpDev.Modules.Content.ModuleMarker;

namespace HelpDev.Architecture.Tests;

public sealed class ContentAiArchitectureTests
{
    [Fact]
    public void Content_Domain_has_no_AI_dependency()
    {
        var result = Types.InAssembly(typeof(ContentModuleMarker).Assembly)
            .That()
            .ResideInNamespaceStartingWith("HelpDev.Modules.Content.Domain")
            .ShouldNot()
            .HaveDependencyOnAny(
                "HelpDev.SharedContracts.Ai",
                "HelpDev.Infrastructure.Ai",
                "OpenAI",
                "Anthropic",
                "Google.Cloud.AIPlatform")
            .GetResult();

        Assert.True(result.IsSuccessful, FormatFailures(result));
    }

    [Fact]
    public void Content_Application_ContentAi_does_not_reference_provider_SDK_or_Infrastructure_Ai()
    {
        var result = Types.InAssembly(typeof(ContentModuleMarker).Assembly)
            .That()
            .ResideInNamespaceStartingWith("HelpDev.Modules.Content.Application.ContentAi")
            .ShouldNot()
            .HaveDependencyOnAny(
                "HelpDev.Infrastructure.Ai",
                "HelpDev.Infrastructure",
                "Microsoft.EntityFrameworkCore",
                "System.Net.Http")
            .GetResult();

        Assert.True(result.IsSuccessful, FormatFailures(result));
    }

    [Fact]
    public void Content_module_assembly_has_no_OpenAI_Claude_Gemini_package_references()
    {
        var referenced = typeof(ContentModuleMarker).Assembly
            .GetReferencedAssemblies()
            .Select(a => a.Name ?? string.Empty)
            .ToList();

        Assert.DoesNotContain(referenced, name => name.Contains("OpenAI", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(referenced, name => name.Contains("Anthropic", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(referenced, name => name.Contains("Gemini", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(referenced, name => name.Contains("Azure.AI", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Ai_adapter_implements_shared_contract_and_lives_in_Infrastructure()
    {
        Assert.True(typeof(IAiTextGenerator).IsAssignableFrom(typeof(FakeAiTextGenerator)));
        Assert.True(typeof(IAiTextGenerator).IsAssignableFrom(typeof(HttpAiTextGenerator)));
        Assert.StartsWith("HelpDev.Infrastructure.Ai", typeof(FakeAiTextGenerator).Namespace);
        Assert.StartsWith("HelpDev.Infrastructure.Ai", typeof(HttpAiTextGenerator).Namespace);
    }

    [Fact]
    public void ContentAiResultDto_exposes_no_secrets_or_prompts()
    {
        var names = typeof(ContentAiResultDto).GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Select(p => p.Name)
            .ToArray();

        Assert.Contains("TaskType", names);
        Assert.Contains("GeneratedText", names);
        Assert.Contains("CreatedAtUtc", names);
        Assert.Contains("Model", names);
        Assert.DoesNotContain(names, n => n.Contains("ApiKey", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(names, n => n.Contains("Prompt", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(names, n => n.Contains("SystemInstruction", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(names, n => n.Contains("Secret", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(names, n => n.Contains("Score", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void AiProviderOptionsValidator_does_not_echo_api_key_in_failures()
    {
        var validator = new AiProviderOptionsValidator();
        var options = new AiProviderOptions
        {
            Enabled = true,
            ProviderName = "Http",
            Model = "m",
            Endpoint = "not-a-url",
            ApiKey = "super-secret-key-value",
        };

        var result = validator.Validate(null, options);
        Assert.True(result.Failed);
        Assert.DoesNotContain("super-secret-key-value", result.FailureMessage);
    }

    [Fact]
    public void Resilient_wrapper_and_health_probe_live_in_Infrastructure()
    {
        Assert.True(typeof(IAiTextGenerator).IsAssignableFrom(typeof(ResilientAiTextGenerator)));
        Assert.True(typeof(IAiHealthProbe).IsAssignableFrom(typeof(AiHealthProbe)));
        Assert.True(typeof(IAiOperationMetrics).IsAssignableFrom(typeof(AiOperationMetrics)));
        Assert.StartsWith("HelpDev.Infrastructure.Ai", typeof(ResilientAiTextGenerator).Namespace);
    }

    [Fact]
    public void AiPolicy_exposes_governance_rules_without_secrets()
    {
        Assert.NotEmpty(AiPolicy.Rules);
        Assert.All(AiPolicy.Rules, rule =>
        {
            Assert.DoesNotContain("sk-", rule, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("ApiKey", rule, StringComparison.OrdinalIgnoreCase);
        });
    }

    private static string FormatFailures(TestResult result) =>
        result.FailingTypeNames is null || !result.FailingTypeNames.Any()
            ? "Architecture rule failed."
            : string.Join(Environment.NewLine, result.FailingTypeNames);
}

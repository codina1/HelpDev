using System.Reflection;
using HelpDev.Modules.PromptLab.Domain;
using HelpDev.Modules.PromptLab.Domain.Prompts;
using HelpDev.SharedKernel.Exceptions;

namespace HelpDev.PromptLab.Tests;

public sealed class PromptVersionTests
{
    private static readonly DateTime Now = new(2026, 7, 19, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Create_is_immutable_with_no_public_mutation_setters()
    {
        var versionType = typeof(PromptVersion);
        var mutableSetters = versionType
            .GetProperties(BindingFlags.Instance | BindingFlags.Public)
            .Where(property => property.SetMethod is { IsPublic: true })
            .Select(property => property.Name)
            .ToList();

        Assert.Empty(mutableSetters);

        var mutationMethods = versionType
            .GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly)
            .Where(method => !method.Name.StartsWith("get_", StringComparison.Ordinal))
            .Where(method =>
                method.Name.Contains("Update", StringComparison.Ordinal)
                || (method.Name.StartsWith("Set", StringComparison.Ordinal)
                    && !method.Name.StartsWith("get_", StringComparison.Ordinal)))
            .Select(method => method.Name)
            .ToList();

        Assert.Empty(mutationMethods);
    }

    [Fact]
    public void Create_requires_exact_placeholder_variable_match()
    {
        var versionId = Guid.NewGuid();
        var variable = CreateVariable(versionId, "code");

        var unknown = Assert.Throws<DomainException>(() =>
            PromptVersion.Create(
                versionId,
                Guid.NewGuid(),
                1,
                "Hello {{name}}",
                null,
                null,
                [variable],
                ["name"],
                Now));
        Assert.Equal(PromptLabErrorCodes.TemplateUnknownPlaceholder, unknown.Code);
    }

    [Fact]
    public void Create_rejects_unused_variable()
    {
        var versionId = Guid.NewGuid();
        var used = CreateVariable(versionId, "code");
        var unused = CreateVariable(versionId, "extra");

        var ex = Assert.Throws<DomainException>(() =>
            PromptVersion.Create(
                versionId,
                Guid.NewGuid(),
                1,
                "Review {{code}}",
                null,
                null,
                [used, unused],
                ["code"],
                Now));

        Assert.Equal(PromptLabErrorCodes.TemplateUnusedVariable, ex.Code);
    }

    [Fact]
    public void Create_rejects_duplicate_placeholders_in_list()
    {
        var versionId = Guid.NewGuid();
        var variable = CreateVariable(versionId, "code");

        var ex = Assert.Throws<DomainException>(() =>
            PromptVersion.Create(
                versionId,
                Guid.NewGuid(),
                1,
                "Review {{code}} {{code}}",
                null,
                null,
                [variable],
                ["code", "code"],
                Now));

        Assert.Equal(PromptLabErrorCodes.TemplatePlaceholderDuplicate, ex.Code);
    }

    private static PromptVariable CreateVariable(Guid versionId, string name) =>
        PromptVariable.Create(
            Guid.NewGuid(),
            versionId,
            name,
            "Code",
            null,
            PromptVariableType.MultilineText,
            true,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            0);
}

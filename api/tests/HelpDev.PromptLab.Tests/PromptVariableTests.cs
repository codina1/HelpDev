using HelpDev.Modules.PromptLab.Domain;
using HelpDev.Modules.PromptLab.Domain.Prompts;
using HelpDev.SharedKernel.Exceptions;

namespace HelpDev.PromptLab.Tests;

public sealed class PromptVariableTests
{
    [Fact]
    public void Create_accepts_valid_types()
    {
        var versionId = Guid.NewGuid();

        var text = PromptVariable.Create(
            Guid.NewGuid(), versionId, "title", "Title", null,
            PromptVariableType.Text, true, null, null, null, null, null, null, null, 0);
        Assert.Equal(PromptVariableType.Text, text.Type);

        var integer = PromptVariable.Create(
            Guid.NewGuid(), versionId, "count", "Count", null,
            PromptVariableType.Integer, false, "1", null, null, 0, 10, null, null, 1);
        Assert.Equal("1", integer.DefaultValue);

        var boolean = PromptVariable.Create(
            Guid.NewGuid(), versionId, "enabled", "Enabled", null,
            PromptVariableType.Boolean, false, "true", null, null, null, null, null, null, 2);
        Assert.Equal("true", boolean.DefaultValue);

        var select = PromptVariable.Create(
            Guid.NewGuid(), versionId, "lang", "Language", null,
            PromptVariableType.Select, true, "cs", null, null, null, null, null, ["cs", "ts"], 3);
        Assert.Equal(["cs", "ts"], select.AllowedValues);
    }

    [Fact]
    public void Create_rejects_reserved_name()
    {
        var ex = Assert.Throws<DomainException>(() =>
            PromptVariable.Create(
                Guid.NewGuid(),
                Guid.NewGuid(),
                "system",
                "System",
                null,
                PromptVariableType.Text,
                true,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                0));

        Assert.Equal(PromptLabErrorCodes.VariableNameReserved, ex.Code);
    }

    [Fact]
    public void Duplicate_names_are_rejected_at_version_create()
    {
        var versionId = Guid.NewGuid();
        var first = PromptVariable.Create(
            Guid.NewGuid(), versionId, "code", "Code", null,
            PromptVariableType.MultilineText, true, null, null, null, null, null, null, null, 0);
        var second = PromptVariable.Create(
            Guid.NewGuid(), versionId, "Code", "Code 2", null,
            PromptVariableType.MultilineText, true, null, null, null, null, null, null, null, 1);

        var ex = Assert.Throws<DomainException>(() =>
            PromptVersion.Create(
                versionId,
                Guid.NewGuid(),
                1,
                "{{code}}",
                null,
                null,
                [first, second],
                ["code"],
                DateTime.UtcNow));

        Assert.Equal(PromptLabErrorCodes.VariableNameDuplicate, ex.Code);
    }

    [Fact]
    public void Create_rejects_invalid_select()
    {
        var emptyOptions = Assert.Throws<DomainException>(() =>
            PromptVariable.Create(
                Guid.NewGuid(), Guid.NewGuid(), "lang", "Language", null,
                PromptVariableType.Select, true, null, null, null, null, null, null, [], 0));
        Assert.Equal(PromptLabErrorCodes.VariableOptionsInvalid, emptyOptions.Code);

        var duplicateOptions = Assert.Throws<DomainException>(() =>
            PromptVariable.Create(
                Guid.NewGuid(), Guid.NewGuid(), "lang", "Language", null,
                PromptVariableType.Select, true, null, null, null, null, null, null, ["a", "a"], 0));
        Assert.Equal(PromptLabErrorCodes.VariableOptionsInvalid, duplicateOptions.Code);
    }
}

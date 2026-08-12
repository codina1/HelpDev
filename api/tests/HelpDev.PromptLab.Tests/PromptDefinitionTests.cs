using HelpDev.Modules.PromptLab.Domain;
using HelpDev.Modules.PromptLab.Domain.Prompts;
using HelpDev.SharedKernel.Exceptions;

namespace HelpDev.PromptLab.Tests;

public sealed class PromptDefinitionTests
{
    private static readonly DateTime Now = new(2026, 7, 19, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void CreateDraft_creates_enabled_unpublished_prompt()
    {
        var categoryId = Guid.NewGuid();
        var prompt = CreateDraft(categoryId: categoryId, name: "  Code Review  ");

        Assert.Equal("Code Review", prompt.Name);
        Assert.Equal(categoryId, prompt.CategoryId);
        Assert.False(prompt.IsPublished);
        Assert.True(prompt.IsEnabled);
        Assert.Null(prompt.PublishedAtUtc);
        Assert.Equal(0, prompt.LatestVersionNumber);
        Assert.Null(prompt.PublishedVersionNumber);
        Assert.Equal(PromptPurpose.CodeReview, prompt.Purpose);
    }

    [Fact]
    public void CreateDraft_rejects_invalid_fields()
    {
        var nameEx = Assert.Throws<DomainException>(() => CreateDraft(name: " "));
        Assert.Equal(PromptLabErrorCodes.PromptNameRequired, nameEx.Code);

        var categoryEx = Assert.Throws<DomainException>(() => CreateDraft(categoryId: Guid.Empty));
        Assert.Equal(PromptLabErrorCodes.PromptCategoryInvalid, categoryEx.Code);

        var slugEx = Assert.Throws<DomainException>(() => CreateDraft(slug: "Bad!"));
        Assert.Equal(PromptLabErrorCodes.PromptSlugInvalid, slugEx.Code);
    }

    [Fact]
    public void RegisterVersion_increments_latest_and_returns_immutable_version()
    {
        var prompt = CreateDraft();
        var versionId = Guid.NewGuid();
        var variable = CreateVariable(versionId, "code");

        var version = prompt.RegisterVersion(
            versionId,
            "Review {{code}}",
            "initial",
            Guid.NewGuid(),
            [variable],
            ["code"],
            Now.AddMinutes(1));

        Assert.Equal(1, prompt.LatestVersionNumber);
        Assert.Equal(1, version.VersionNumber);
        Assert.Equal("Review {{code}}", version.Template);
        Assert.Single(prompt.Versions);
    }

    [Fact]
    public void Publish_when_enabled_sets_published_at_once_and_republish_newer()
    {
        var prompt = CreateDraft();
        var v1Id = Guid.NewGuid();
        prompt.RegisterVersion(
            v1Id,
            "Review {{code}}",
            null,
            null,
            [CreateVariable(v1Id, "code")],
            ["code"],
            Now.AddMinutes(1));

        var publishedAt = Now.AddMinutes(2);
        Assert.True(prompt.PublishVersion(1, publishedAt));
        Assert.True(prompt.IsPublished);
        Assert.Equal(1, prompt.PublishedVersionNumber);
        Assert.Equal(publishedAt, prompt.PublishedAtUtc);

        Assert.False(prompt.PublishVersion(1, publishedAt.AddMinutes(5)));
        Assert.Equal(publishedAt, prompt.PublishedAtUtc);

        var v2Id = Guid.NewGuid();
        prompt.RegisterVersion(
            v2Id,
            "Review {{code}} carefully",
            "v2",
            null,
            [CreateVariable(v2Id, "code")],
            ["code"],
            Now.AddMinutes(10));

        Assert.True(prompt.PublishVersion(2, publishedAt.AddMinutes(15)));
        Assert.Equal(2, prompt.PublishedVersionNumber);
        Assert.Equal(publishedAt, prompt.PublishedAtUtc);
    }

    [Fact]
    public void Publish_when_disabled_is_rejected()
    {
        var prompt = CreateDraft();
        var versionId = Guid.NewGuid();
        prompt.RegisterVersion(
            versionId,
            "Review {{code}}",
            null,
            null,
            [CreateVariable(versionId, "code")],
            ["code"],
            Now.AddMinutes(1));
        Assert.True(prompt.Disable(Now.AddMinutes(2)));

        var ex = Assert.Throws<DomainException>(() => prompt.PublishVersion(1, Now.AddMinutes(3)));
        Assert.Equal(PromptLabErrorCodes.PromptCannotPublish, ex.Code);
        Assert.False(prompt.IsPublished);
    }

    private static PromptDefinition CreateDraft(
        Guid? categoryId = null,
        string name = "Code Review",
        string slug = "code-review") =>
        PromptDefinition.CreateDraft(
            Guid.NewGuid(),
            categoryId ?? Guid.NewGuid(),
            name,
            slug,
            "Reviews code",
            null,
            PromptPurpose.CodeReview,
            PromptVisibility.Public,
            requiresAuthentication: false,
            allowHistory: false,
            displayOrder: 0,
            Now);

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

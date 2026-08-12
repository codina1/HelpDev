using HelpDev.Modules.Toolbox.Domain;
using HelpDev.Modules.Toolbox.Domain.Tools;
using HelpDev.SharedKernel.Exceptions;

namespace HelpDev.Toolbox.Tests;

public sealed class ToolDefinitionTests
{
    private static readonly DateTime Now = new(2026, 7, 19, 12, 0, 0, DateTimeKind.Utc);
    private const string DefaultSchema = """{"type":"object","properties":{"text":{"type":"string"}}}""";

    [Fact]
    public void CreateDraft_creates_enabled_unpublished_tool()
    {
        var categoryId = Guid.NewGuid();
        var tool = CreateDraft(categoryId: categoryId, name: "  JSON Formatter  ");

        Assert.Equal("JSON Formatter", tool.Name);
        Assert.Equal(categoryId, tool.CategoryId);
        Assert.False(tool.IsPublished);
        Assert.True(tool.IsEnabled);
        Assert.Null(tool.PublishedAtUtc);
        Assert.Equal(ToolType.JsonFormatter, tool.Type);
    }

    [Fact]
    public void CreateDraft_rejects_invalid_fields()
    {
        var nameEx = Assert.Throws<DomainException>(() =>
            CreateDraft(name: " "));
        Assert.Equal(ToolboxErrorCodes.ToolNameRequired, nameEx.Code);

        var categoryEx = Assert.Throws<DomainException>(() =>
            CreateDraft(categoryId: Guid.Empty));
        Assert.Equal(ToolboxErrorCodes.ToolCategoryInvalid, categoryEx.Code);

        var slugEx = Assert.Throws<DomainException>(() =>
            CreateDraft(slug: "Bad!"));
        Assert.Equal(ToolboxErrorCodes.ToolSlugInvalid, slugEx.Code);
    }

    [Fact]
    public void CreateDraft_rejects_invalid_schema()
    {
        var ex = Assert.Throws<DomainException>(() =>
            CreateDraft(inputSchema: "{not-json"));

        Assert.Equal(ToolboxErrorCodes.ToolSchemaInvalid, ex.Code);
    }

    [Fact]
    public void Publish_when_enabled_sets_published_at_once()
    {
        var tool = CreateDraft();
        var publishedAt = Now.AddMinutes(1);

        Assert.True(tool.Publish(publishedAt));
        Assert.True(tool.IsPublished);
        Assert.Equal(publishedAt, tool.PublishedAtUtc);

        Assert.False(tool.Publish(publishedAt.AddMinutes(5)));
        Assert.Equal(publishedAt, tool.PublishedAtUtc);

        Assert.True(tool.Unpublish(publishedAt.AddMinutes(10)));
        Assert.True(tool.Publish(publishedAt.AddMinutes(15)));
        Assert.Equal(publishedAt, tool.PublishedAtUtc);
    }

    [Fact]
    public void Publish_when_disabled_is_rejected()
    {
        var tool = CreateDraft();
        Assert.True(tool.Disable(Now.AddMinutes(1)));

        var ex = Assert.Throws<DomainException>(() => tool.Publish(Now.AddMinutes(2)));
        Assert.Equal(ToolboxErrorCodes.ToolCannotPublish, ex.Code);
        Assert.False(tool.IsPublished);
    }

    [Fact]
    public void Enable_and_disable_work()
    {
        var tool = CreateDraft();

        Assert.False(tool.Enable(Now.AddMinutes(1)));
        Assert.True(tool.Disable(Now.AddMinutes(2)));
        Assert.False(tool.IsEnabled);
        Assert.True(tool.Enable(Now.AddMinutes(3)));
        Assert.True(tool.IsEnabled);
    }

    [Fact]
    public void CategoryId_is_scalar_and_can_change()
    {
        var original = Guid.NewGuid();
        var next = Guid.NewGuid();
        var tool = CreateDraft(categoryId: original);

        Assert.Equal(original, tool.CategoryId);
        Assert.True(tool.ChangeCategory(next, Now.AddMinutes(1)));
        Assert.Equal(next, tool.CategoryId);
        Assert.False(tool.ChangeCategory(next, Now.AddMinutes(2)));
    }

    private static ToolDefinition CreateDraft(
        Guid? categoryId = null,
        string name = "JSON Formatter",
        string slug = "json-formatter",
        string inputSchema = DefaultSchema) =>
        ToolDefinition.CreateDraft(
            Guid.NewGuid(),
            categoryId ?? Guid.NewGuid(),
            name,
            slug,
            "Formats JSON",
            null,
            ToolType.JsonFormatter,
            inputSchema,
            null,
            requiresAuthentication: false,
            allowHistory: false,
            displayOrder: 0,
            Now);
}

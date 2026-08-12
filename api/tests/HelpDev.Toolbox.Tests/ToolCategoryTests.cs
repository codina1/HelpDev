using HelpDev.Modules.Toolbox.Domain;
using HelpDev.Modules.Toolbox.Domain.Categories;
using HelpDev.SharedKernel.Exceptions;

namespace HelpDev.Toolbox.Tests;

public sealed class ToolCategoryTests
{
    private static readonly DateTime Now = new(2026, 7, 19, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Create_trims_name_and_sets_active()
    {
        var category = ToolCategory.Create(
            Guid.NewGuid(),
            "  Encoding  ",
            "encoding",
            "  Helpers  ",
            "  wrench  ",
            1,
            Now);

        Assert.Equal("Encoding", category.Name);
        Assert.Equal("encoding", category.Slug.Value);
        Assert.Equal("Helpers", category.Description);
        Assert.Equal("wrench", category.Icon);
        Assert.Equal(1, category.DisplayOrder);
        Assert.True(category.IsActive);
        Assert.Equal(Now, category.CreatedAtUtc);
        Assert.Equal(Now, category.UpdatedAtUtc);
    }

    [Fact]
    public void Create_rejects_invalid_name_and_slug()
    {
        var nameEx = Assert.Throws<DomainException>(() =>
            ToolCategory.Create(Guid.NewGuid(), " ", "encoding", null, null, 0, Now));
        Assert.Equal(ToolboxErrorCodes.CategoryNameRequired, nameEx.Code);

        var slugEx = Assert.Throws<DomainException>(() =>
            ToolCategory.Create(Guid.NewGuid(), "Encoding", "Bad Slug!", null, null, 0, Now));
        Assert.Equal(ToolboxErrorCodes.CategorySlugInvalid, slugEx.Code);
    }

    [Fact]
    public void Create_rejects_negative_display_order()
    {
        var ex = Assert.Throws<DomainException>(() =>
            ToolCategory.Create(Guid.NewGuid(), "Encoding", "encoding", null, null, -1, Now));

        Assert.Equal(ToolboxErrorCodes.CategoryNameInvalid, ex.Code);
    }

    [Fact]
    public void Activate_and_deactivate_are_idempotent_no_ops()
    {
        var category = ToolCategory.Create(Guid.NewGuid(), "Encoding", "encoding", null, null, 0, Now);

        Assert.False(category.Activate(Now.AddMinutes(1)));
        Assert.Equal(Now, category.UpdatedAtUtc);

        Assert.True(category.Deactivate(Now.AddMinutes(2)));
        Assert.False(category.IsActive);
        Assert.Equal(Now.AddMinutes(2), category.UpdatedAtUtc);

        Assert.False(category.Deactivate(Now.AddMinutes(3)));
        Assert.Equal(Now.AddMinutes(2), category.UpdatedAtUtc);
    }

    [Fact]
    public void UpdateDetails_noop_does_not_change_timestamp()
    {
        var category = ToolCategory.Create(
            Guid.NewGuid(),
            "Encoding",
            "encoding",
            "Helpers",
            "wrench",
            1,
            Now);

        Assert.False(category.UpdateDetails("Encoding", "Helpers", "wrench", 1, Now.AddHours(1)));
        Assert.Equal(Now, category.UpdatedAtUtc);
    }
}

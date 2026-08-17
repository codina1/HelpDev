using HelpDev.Modules.PromptLab.Domain;
using HelpDev.Modules.PromptLab.Domain.Categories;
using HelpDev.SharedKernel.Exceptions;

namespace HelpDev.PromptLab.Tests;

public sealed class PromptCategoryTests
{
    private static readonly DateTime Now = new(2026, 7, 19, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Create_trims_name_and_sets_active()
    {
        var category = PromptCategory.Create(
            Guid.NewGuid(),
            "  Coding  ",
            "coding",
            "  Helpers  ",
            "  wrench  ",
            1,
            Now);

        Assert.Equal("Coding", category.Name);
        Assert.Equal("coding", category.Slug.Value);
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
            PromptCategory.Create(Guid.NewGuid(), " ", "coding", null, null, 0, Now));
        Assert.Equal(PromptLabErrorCodes.CategoryNameRequired, nameEx.Code);

        var slugEx = Assert.Throws<DomainException>(() =>
            PromptCategory.Create(Guid.NewGuid(), "Coding", "Bad Slug!", null, null, 0, Now));
        Assert.Equal(PromptLabErrorCodes.CategorySlugInvalid, slugEx.Code);
    }

    [Fact]
    public void Create_rejects_negative_display_order()
    {
        var ex = Assert.Throws<DomainException>(() =>
            PromptCategory.Create(Guid.NewGuid(), "Coding", "coding", null, null, -1, Now));

        Assert.Equal(PromptLabErrorCodes.CategoryNameInvalid, ex.Code);
    }

    [Fact]
    public void Activate_and_deactivate_are_idempotent_no_ops()
    {
        var category = PromptCategory.Create(Guid.NewGuid(), "Coding", "coding", null, null, 0, Now);

        Assert.False(category.Activate(Now.AddMinutes(1)));
        Assert.Equal(Now, category.UpdatedAtUtc);

        Assert.True(category.Deactivate(Now.AddMinutes(2)));
        Assert.False(category.IsActive);
        Assert.Equal(Now.AddMinutes(2), category.UpdatedAtUtc);

        Assert.False(category.Deactivate(Now.AddMinutes(3)));
        Assert.Equal(Now.AddMinutes(2), category.UpdatedAtUtc);
    }

    [Fact]
    public void EnsureActive_rejects_inactive_category()
    {
        var category = PromptCategory.Create(Guid.NewGuid(), "Coding", "coding", null, null, 0, Now);
        category.EnsureActive();

        category.Deactivate(Now.AddMinutes(1));
        var ex = Assert.Throws<DomainException>(category.EnsureActive);
        Assert.Equal(PromptLabErrorCodes.CategoryInactive, ex.Code);
    }

    [Fact]
    public void Catalog_creates_the_default_prompt_categories()
    {
        var categories = PromptCategoryCatalog.CreateDefaults(Now);

        Assert.Equal(
            new[] { "Image", "Video", "Coding", "Writing", "Marketing", "Design", "Education" },
            categories.Select(category => category.Name));
        Assert.Equal(
            new[] { "image", "video", "coding", "writing", "marketing", "design", "education" },
            categories.Select(category => category.Slug.Value));
        Assert.All(categories, category => Assert.True(category.IsActive));
        Assert.All(categories, category => Assert.Equal(Now, category.CreatedAtUtc));
        Assert.Equal(PromptCategoryCatalog.Defaults.Count, categories.Count);
        Assert.Equal(categories.Count, categories.Select(category => category.Id).Distinct().Count());
    }
}

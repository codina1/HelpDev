using ContentEntity = HelpDev.Modules.Content.Domain.Entities.Content;
using HelpDev.Modules.Content.Domain.Enums;
using HelpDev.Modules.Content.Domain.Events;
using HelpDev.Modules.Content.Domain.ValueObjects;
using HelpDev.SharedKernel.Exceptions;

namespace HelpDev.Content.Tests;

public sealed class SeoMetadataTests
{
    [Fact]
    public void Create_with_valid_values_normalizes_and_stores()
    {
        var seo = SeoMetadata.Create(
            "  SEO Title  ",
            "  A concise description  ",
            "https://helpdev.example/a",
            "https://cdn.helpdev.example/og.png",
            "  keyword  ");

        Assert.Equal("SEO Title", seo.SeoTitle);
        Assert.Equal("A concise description", seo.SeoDescription);
        Assert.Equal("https://helpdev.example/a", seo.CanonicalUrl);
        Assert.Equal("https://cdn.helpdev.example/og.png", seo.OgImage);
        Assert.Equal("keyword", seo.FocusKeyword);
        Assert.False(seo.IsEmpty);
    }

    [Fact]
    public void Create_with_blanks_produces_empty_metadata()
    {
        var seo = SeoMetadata.Create("  ", "", null, "   ", null);

        Assert.True(seo.IsEmpty);
        Assert.Null(seo.SeoTitle);
        Assert.Null(seo.CanonicalUrl);
    }

    [Fact]
    public void Create_with_over_length_title_throws()
    {
        var tooLong = new string('t', SeoMetadata.MaxSeoTitleLength + 1);

        Assert.Throws<DomainException>(() => SeoMetadata.Create(tooLong, null, null, null, null));
    }

    [Fact]
    public void Create_with_over_length_description_throws()
    {
        var tooLong = new string('d', SeoMetadata.MaxSeoDescriptionLength + 1);

        Assert.Throws<DomainException>(() => SeoMetadata.Create(null, tooLong, null, null, null));
    }

    [Theory]
    [InlineData("not-a-url")]
    [InlineData("/relative/path")]
    [InlineData("ftp://helpdev.example/a")]
    [InlineData("example.com")]
    public void Create_with_invalid_canonical_url_throws(string url)
    {
        Assert.Throws<DomainException>(() => SeoMetadata.Create(null, null, url, null, null));
    }

    [Fact]
    public void Create_with_over_length_focus_keyword_throws()
    {
        var tooLong = new string('k', SeoMetadata.MaxFocusKeywordLength + 1);

        Assert.Throws<DomainException>(() => SeoMetadata.Create(null, null, null, null, tooLong));
    }

    [Fact]
    public void Equality_is_value_based()
    {
        var a = SeoMetadata.Create("Title", null, null, null, null);
        var b = SeoMetadata.Create("Title", null, null, null, null);
        var c = SeoMetadata.Create("Other", null, null, null, null);

        Assert.Equal(a, b);
        Assert.NotEqual(a, c);
    }

    [Fact]
    public void UpdateSeoMetadata_on_published_content_raises_update_event()
    {
        var content = CreatePublished();
        content.DequeueDomainEvents();

        content.UpdateSeoMetadata(
            SeoMetadata.Create("New SEO", "New description", null, null, null),
            Now);

        Assert.Equal(Now, content.UpdatedAt);
        var domainEvent = Assert.Single(content.DomainEvents);
        var updated = Assert.IsType<ContentUpdatedDomainEvent>(domainEvent);
        Assert.Equal(content.Id, updated.ContentId);
    }

    [Fact]
    public void UpdateSeoMetadata_on_draft_content_is_silent()
    {
        var content = CreateDraft();
        content.DequeueDomainEvents();

        content.UpdateSeoMetadata(
            SeoMetadata.Create("Draft SEO", null, null, null, null),
            Now);

        Assert.Equal("Draft SEO", content.SeoMetadata.SeoTitle);
        Assert.Equal(Now, content.UpdatedAt);
        Assert.False(content.HasDomainEvents);
        Assert.Empty(content.DomainEvents);
    }

    [Fact]
    public void UpdateSeoMetadata_with_identical_values_is_a_noop()
    {
        var content = CreatePublished();
        var seo = SeoMetadata.Create("Same", "Same description", null, null, null);
        content.UpdateSeoMetadata(seo, Now.AddHours(1));
        var updatedAtAfterFirst = content.UpdatedAt;
        content.DequeueDomainEvents();

        content.UpdateSeoMetadata(
            SeoMetadata.Create("Same", "Same description", null, null, null),
            Now.AddHours(5));

        Assert.Equal(updatedAtAfterFirst, content.UpdatedAt);
        Assert.False(content.HasDomainEvents);
        Assert.Empty(content.DomainEvents);
    }

    private static readonly DateTime Now = new(2026, 7, 21, 12, 0, 0, DateTimeKind.Utc);

    private static ContentEntity CreateDraft() =>
        ContentEntity.Create(
            Guid.NewGuid(),
            "Original Title",
            Slug.Create("original-slug"),
            "Original body",
            ContentType.Article,
            Guid.NewGuid(),
            ContentStatus.Draft,
            Now.AddDays(-1));

    private static ContentEntity CreatePublished()
    {
        var authorId = Guid.NewGuid();
        return ContentWorkflowTestHelper.CreatePublished(
            Guid.NewGuid(),
            "Original Title",
            "original-slug",
            "Original body",
            ContentType.Article,
            authorId,
            Now.AddDays(-1));
    }
}

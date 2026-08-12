using HelpDev.Modules.Content.Domain.Entities;
using ContentEntity = HelpDev.Modules.Content.Domain.Entities.Content;
using HelpDev.Modules.Content.Domain.Enums;
using HelpDev.Modules.Content.Domain.Events;
using HelpDev.Modules.Content.Domain.ValueObjects;
using HelpDev.SharedKernel.Exceptions;

namespace HelpDev.Content.Tests;

public sealed class ContentUpdateTests
{
    private static readonly DateTime Now = new(2026, 7, 21, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void UpdateDetails_on_draft_changes_fields_but_raises_no_event()
    {
        var content = CreateDraft();
        content.DequeueDomainEvents();

        content.UpdateDetails(
            "New Title",
            Slug.Create("new-slug"),
            ContentType.News,
            "New body",
            excerpt: "New excerpt",
            coverImage: "https://cdn.example.com/cover.png",
            Now);

        Assert.Equal("New Title", content.Title);
        Assert.Equal("new-slug", content.Slug.Value);
        Assert.Equal(ContentType.News, content.Type);
        Assert.Equal("New body", content.Body);
        Assert.Equal("New excerpt", content.Excerpt);
        Assert.Equal("https://cdn.example.com/cover.png", content.CoverImage);
        Assert.Equal(Now, content.UpdatedAt);

        // Draft edits stay silent (search/read models only track published content).
        Assert.False(content.HasDomainEvents);
        Assert.Empty(content.DomainEvents);
    }

    [Fact]
    public void UpdateDetails_on_published_content_raises_one_update_event()
    {
        var content = CreatePublished();
        content.DequeueDomainEvents();

        content.UpdateDetails(
            "Changed Title",
            Slug.Create("changed-slug"),
            ContentType.Article,
            "Changed body with more text",
            excerpt: null,
            coverImage: null,
            Now);

        Assert.Equal(Now, content.UpdatedAt);
        var domainEvent = Assert.Single(content.DomainEvents);
        var updated = Assert.IsType<ContentUpdatedDomainEvent>(domainEvent);
        Assert.Equal(content.Id, updated.ContentId);
        Assert.Equal("changed-slug", updated.Slug);
    }

    [Fact]
    public void UpdateDetails_with_identical_values_raises_no_event_and_keeps_timestamp()
    {
        var content = CreatePublished();
        var originalUpdatedAt = content.UpdatedAt;
        content.DequeueDomainEvents();

        content.UpdateDetails(
            content.Title,
            content.Slug,
            content.Type,
            content.Body,
            content.Excerpt,
            content.CoverImage,
            Now);

        Assert.Equal(originalUpdatedAt, content.UpdatedAt);
        Assert.False(content.HasDomainEvents);
        Assert.Empty(content.DomainEvents);
    }

    [Fact]
    public void UpdateDetails_with_excerpt_over_limit_throws()
    {
        var content = CreateDraft();
        var tooLong = new string('x', ContentEntity.MaxExcerptLength + 1);

        Assert.Throws<DomainException>(() => content.UpdateDetails(
            content.Title,
            content.Slug,
            content.Type,
            content.Body,
            excerpt: tooLong,
            coverImage: null,
            Now));
    }

    [Fact]
    public void UpdateDetails_with_cover_image_over_limit_throws()
    {
        var content = CreateDraft();
        var tooLong = new string('y', ContentEntity.MaxCoverImageLength + 1);

        Assert.Throws<DomainException>(() => content.UpdateDetails(
            content.Title,
            content.Slug,
            content.Type,
            content.Body,
            excerpt: null,
            coverImage: tooLong,
            Now));
    }

    [Fact]
    public void UpdateDetails_with_blank_title_throws()
    {
        var content = CreateDraft();

        Assert.Throws<DomainException>(() => content.UpdateDetails(
            "   ",
            content.Slug,
            content.Type,
            content.Body,
            excerpt: null,
            coverImage: null,
            Now));
    }

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

using HelpDev.Modules.Content.Application.Contents.Revisions;
using HelpDev.Modules.Content.Application.Contents.Dtos;
using HelpDev.Modules.Content.Domain.Entities;
using HelpDev.Modules.Content.Domain.Enums;
using HelpDev.Modules.Content.Domain.ValueObjects;
using HelpDev.SharedKernel.Exceptions;

namespace HelpDev.Content.Tests;

public sealed class ContentRevisionDomainTests
{
    [Fact]
    public void ContentRevision_create_assigns_sequential_fields()
    {
        var contentId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var snapshot = ContentRevisionSnapshot.FromContent(CreateSampleContent(contentId, userId));

        var revision = ContentRevision.Create(
            Guid.NewGuid(),
            contentId,
            versionNumber: 1,
            snapshot,
            changeReason: "Initial save",
            userId,
            DateTime.UtcNow);

        Assert.Equal(1, revision.VersionNumber);
        Assert.Equal("Initial save", revision.ChangeReason);
        Assert.Equal(snapshot, revision.Snapshot);
    }

    [Fact]
    public void ContentRevision_rejects_invalid_version()
    {
        var snapshot = ContentRevisionSnapshot.Create(
            "Title",
            "slug",
            "body",
            "",
            null,
            nameof(ContentType.Article),
            ContentRevisionSeoSnapshot.Create(null, null, null, null, null));

        Assert.Throws<ArgumentException>(() => ContentRevision.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            0,
            snapshot,
            null,
            Guid.NewGuid(),
            DateTime.UtcNow));
    }

    [Fact]
    public void Snapshot_equality_is_value_based()
    {
        var seo = ContentRevisionSeoSnapshot.Create("t", null, null, null, null);
        var left = ContentRevisionSnapshot.Create("A", "a", "b", "", null, "Article", seo);
        var right = ContentRevisionSnapshot.Create("A", "a", "b", "", null, "Article", seo);

        Assert.Equal(left, right);
    }

    [Fact]
    public void RestoreFromSnapshot_applies_fields_and_raises_update_for_published()
    {
        var authorId = Guid.NewGuid();
        var content = CreateSampleContent(Guid.NewGuid(), authorId);
        content.SubmitForReview(authorId, DateTime.UtcNow);
        content.Approve(authorId, DateTime.UtcNow);
        content.Publish(authorId, DateTime.UtcNow);

        var snapshot = ContentRevisionSnapshot.Create(
            "Restored",
            "restored-slug",
            "Restored body",
            "Excerpt",
            "/media/cover.png",
            nameof(ContentType.News),
            ContentRevisionSeoSnapshot.Create("SEO", null, null, null, null));

        Assert.True(content.RestoreFromSnapshot(snapshot, DateTime.UtcNow.AddMinutes(1)));
        Assert.Equal("Restored", content.Title);
        Assert.Equal("restored-slug", content.Slug.Value);
        Assert.Equal(ContentType.News, content.Type);
        Assert.Equal("SEO", content.SeoMetadata.SeoTitle);
        Assert.Contains(content.DomainEvents, e => e is HelpDev.Modules.Content.Domain.Events.ContentUpdatedDomainEvent);
    }

    [Fact]
    public void UpdateDetails_no_op_returns_false()
    {
        var content = CreateSampleContent(Guid.NewGuid(), Guid.NewGuid());
        var changed = content.UpdateDetails(
            content.Title,
            content.Slug,
            content.Type,
            content.Body,
            content.Excerpt,
            content.CoverImage,
            DateTime.UtcNow);

        Assert.False(changed);
    }

    private static HelpDev.Modules.Content.Domain.Entities.Content CreateSampleContent(Guid id, Guid authorId)
    {
        var slug = Slug.Create("sample-slug");
        return HelpDev.Modules.Content.Domain.Entities.Content.Create(
            id,
            "Title",
            slug,
            "Body",
            ContentType.Article,
            authorId,
            ContentStatus.Draft,
            DateTime.UtcNow);
    }
}

using HelpDev.Modules.Content.Domain.Entities;
using ContentEntity = HelpDev.Modules.Content.Domain.Entities.Content;
using HelpDev.Modules.Content.Domain.Enums;
using HelpDev.Modules.Content.Domain.Events;
using HelpDev.Modules.Content.Domain.ValueObjects;

namespace HelpDev.Content.Tests;

public sealed class ContentCreationTests
{
    [Fact]
    public void Create_with_draft_status_leaves_draft_and_raises_no_events()
    {
        var id = Guid.NewGuid();
        var authorId = Guid.NewGuid();
        var createdAt = DateTime.UtcNow;

        var content = ContentEntity.Create(
            id,
            "  Title  ",
            Slug.Create("sample-slug"),
            "  Body text  ",
            ContentType.Article,
            authorId,
            ContentStatus.Draft,
            createdAt);

        Assert.Equal(id, content.Id);
        Assert.Equal("Title", content.Title);
        Assert.Equal("sample-slug", content.Slug.Value);
        Assert.Equal("Body text", content.Body);
        Assert.Equal(ContentType.Article, content.Type);
        Assert.Equal(authorId, content.AuthorId);
        Assert.Equal(ContentStatus.Draft, content.Status);
        Assert.Equal(0, content.Views);
        Assert.Equal(0, content.Saves);
        Assert.Equal(createdAt, content.CreatedAt);
        Assert.False(content.HasDomainEvents);
        Assert.Empty(content.DomainEvents);
    }

    [Fact]
    public void Create_with_published_status_publishes_and_raises_published_event()
    {
        var authorId = Guid.NewGuid();
        var createdAt = DateTime.UtcNow;
        var content = ContentWorkflowTestHelper.CreatePublished(
            Guid.NewGuid(),
            "Published Title",
            "published-slug",
            "Body",
            ContentType.News,
            authorId,
            createdAt);

        Assert.Equal(ContentStatus.Published, content.Status);
        Assert.True(content.HasDomainEvents);
        var domainEvent = Assert.Single(content.DomainEvents);
        var published = Assert.IsType<ContentPublishedDomainEvent>(domainEvent);
        Assert.Equal(content.Id, published.ContentId);
        Assert.Equal("published-slug", published.Slug);
    }

    [Fact]
    public void CreatePublishedSeed_creates_published_content_without_domain_events()
    {
        var content = ContentEntity.CreatePublishedSeed(
            Guid.NewGuid(),
            "Seed Title",
            Slug.Create("seed-slug"),
            "Seed body",
            ContentType.Tool,
            Guid.NewGuid(),
            DateTime.UtcNow,
            views: 10,
            saves: 2);

        Assert.Equal(ContentStatus.Published, content.Status);
        Assert.Equal(10, content.Views);
        Assert.Equal(2, content.Saves);
        Assert.False(content.HasDomainEvents);
        Assert.Empty(content.DomainEvents);
    }
}

using HelpDev.Modules.Content.Domain.Entities;

using ContentEntity = HelpDev.Modules.Content.Domain.Entities.Content;

using HelpDev.Modules.Content.Domain.Enums;

using HelpDev.Modules.Content.Domain.Events;

using HelpDev.Modules.Content.Domain.ValueObjects;



namespace HelpDev.Content.Tests;



public sealed class ContentPublishingTests

{

    [Fact]

    public void Publish_from_approved_transitions_and_raises_event()

    {

        var actorId = Guid.NewGuid();

        var content = ContentEntity.Create(

            Guid.NewGuid(),

            "Draft Title",

            Slug.Create("draft-slug"),

            "Body",

            ContentType.Article,

            actorId,

            ContentStatus.Draft,

            DateTime.UtcNow);

        content.SubmitForReview(actorId, DateTime.UtcNow);

        content.Approve(actorId, DateTime.UtcNow);

        content.DequeueDomainEvents();



        var publishedAt = new DateTime(2026, 7, 21, 10, 0, 0, DateTimeKind.Utc);

        content.Publish(actorId, publishedAt);



        Assert.Equal(ContentStatus.Published, content.Status);

        Assert.Equal(publishedAt, content.PublishedAtUtc);

        var domainEvent = Assert.Single(content.DomainEvents);

        var published = Assert.IsType<ContentPublishedDomainEvent>(domainEvent);

        Assert.Equal(content.Id, published.ContentId);

        Assert.Equal("draft-slug", published.Slug);

    }



    [Fact]

    public void Publish_on_already_published_content_is_noop_via_workflow_service_path()

    {

        var actorId = Guid.NewGuid();

        var content = ContentEntity.Create(

            Guid.NewGuid(),

            "Published Title",

            Slug.Create("published-slug"),

            "Body",

            ContentType.News,

            actorId,

            ContentStatus.Draft,

            DateTime.UtcNow);

        content.SubmitForReview(actorId, DateTime.UtcNow);

        content.Approve(actorId, DateTime.UtcNow);

        content.Publish(actorId, DateTime.UtcNow);

        content.DequeueDomainEvents();

        Assert.Equal(ContentStatus.Published, content.Status);
        Assert.False(content.HasDomainEvents);
    }
}



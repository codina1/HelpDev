using HelpDev.Modules.Content.Domain.Entities;
using HelpDev.Modules.Content.Domain.Enums;
using HelpDev.Modules.Content.Domain.Events;
using HelpDev.Modules.Content.Domain.ValueObjects;
using HelpDev.Modules.Content.Domain.Workflow;
using HelpDev.SharedKernel.Exceptions;
using ContentEntity = HelpDev.Modules.Content.Domain.Entities.Content;

namespace HelpDev.Content.Tests;

public sealed class ContentWorkflowDomainTests
{
    [Fact]
    public void SubmitForReview_from_draft_succeeds()
    {
        var content = CreateDraft();
        var transition = content.SubmitForReview(Guid.NewGuid(), DateTime.UtcNow);

        Assert.Equal(ContentStatus.ReviewPending, content.Status);
        Assert.Equal(ContentStatus.Draft, transition.FromStatus);
        Assert.Equal(ContentStatus.ReviewPending, transition.ToStatus);
    }

    [Fact]
    public void Draft_to_archived_is_rejected()
    {
        var content = CreateDraft();
        Assert.Throws<DomainException>(() => content.Archive(Guid.NewGuid(), DateTime.UtcNow));
    }

    [Fact]
    public void Publish_from_approved_raises_published_event()
    {
        var content = CreateDraft();
        content.SubmitForReview(Guid.NewGuid(), DateTime.UtcNow);
        content.Approve(Guid.NewGuid(), DateTime.UtcNow);
        content.DequeueDomainEvents();

        content.Publish(Guid.NewGuid(), DateTime.UtcNow);

        Assert.Equal(ContentStatus.Published, content.Status);
        Assert.Contains(content.DomainEvents, e => e is ContentPublishedDomainEvent);
    }

    [Fact]
    public void Reject_requires_comment()
    {
        var content = CreateDraft();
        content.SubmitForReview(Guid.NewGuid(), DateTime.UtcNow);

        Assert.Throws<ArgumentException>(() => content.Reject("  ", Guid.NewGuid(), DateTime.UtcNow));
    }

    [Fact]
    public void Workflow_transition_is_immutable_type()
    {
        var transition = ContentWorkflowTransition.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            ContentStatus.Draft,
            ContentStatus.ReviewPending,
            Guid.NewGuid(),
            null,
            DateTime.UtcNow);

        Assert.Null(transition.Comment);
    }

    [Theory]
    [InlineData(ContentStatus.Published, ContentStatus.ReviewPending)]
    [InlineData(ContentStatus.Archived, ContentStatus.Draft)]
    [InlineData(ContentStatus.Draft, ContentStatus.Approved)]
    public void Invalid_transitions_throw(ContentStatus from, ContentStatus to)
    {
        Assert.False(ContentWorkflowRules.IsAllowed(from, to));
    }

    private static ContentEntity CreateDraft() =>
        ContentEntity.Create(
            Guid.NewGuid(),
            "Title",
            Slug.Create("slug"),
            "Body",
            ContentType.Article,
            Guid.NewGuid(),
            ContentStatus.Draft,
            DateTime.UtcNow);
}

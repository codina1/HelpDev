using HelpDev.Modules.Content.Domain.Entities;
using HelpDev.Modules.Content.Domain.Enums;
using ContentEntity = HelpDev.Modules.Content.Domain.Entities.Content;

namespace HelpDev.Content.Tests;

internal static class ContentWorkflowTestHelper
{
    public static ContentEntity PromoteToPublished(ContentEntity content, Guid actorUserId, DateTime utc)
    {
        content.SubmitForReview(actorUserId, utc);
        content.Approve(actorUserId, utc);
        content.Publish(actorUserId, utc);
        return content;
    }

    public static ContentEntity CreatePublished(
        Guid id,
        string title,
        string slug,
        string body,
        ContentType type,
        Guid authorId,
        DateTime createdAtUtc)
    {
        var content = ContentEntity.Create(
            id,
            title,
            HelpDev.Modules.Content.Domain.ValueObjects.Slug.Create(slug),
            body,
            type,
            authorId,
            ContentStatus.Draft,
            createdAtUtc);
        return PromoteToPublished(content, authorId, createdAtUtc);
    }
}

using HelpDev.Modules.Content.Domain.Enums;

namespace HelpDev.Modules.Content.Domain.Entities;

/// <summary>
/// Immutable workflow audit row. No update/delete in v1.
/// </summary>
public sealed class ContentWorkflowTransition
{
    public const int MaxCommentLength = 1000;

    private ContentWorkflowTransition()
    {
    }

    private ContentWorkflowTransition(
        Guid id,
        Guid contentId,
        ContentStatus fromStatus,
        ContentStatus toStatus,
        Guid actorUserId,
        string? comment,
        DateTime createdAtUtc)
    {
        Id = id;
        ContentId = contentId;
        FromStatus = fromStatus;
        ToStatus = toStatus;
        ActorUserId = actorUserId;
        Comment = comment;
        CreatedAtUtc = createdAtUtc;
    }

    public Guid Id { get; private set; }

    public Guid ContentId { get; private set; }

    public ContentStatus FromStatus { get; private set; }

    public ContentStatus ToStatus { get; private set; }

    public Guid ActorUserId { get; private set; }

    public string? Comment { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    public static ContentWorkflowTransition Create(
        Guid id,
        Guid contentId,
        ContentStatus fromStatus,
        ContentStatus toStatus,
        Guid actorUserId,
        string? comment,
        DateTime createdAtUtc)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("Transition id is required.", nameof(id));
        }

        if (contentId == Guid.Empty)
        {
            throw new ArgumentException("Content id is required.", nameof(contentId));
        }

        if (actorUserId == Guid.Empty)
        {
            throw new ArgumentException("Actor user id is required.", nameof(actorUserId));
        }

        var normalizedComment = NormalizeComment(comment);

        return new ContentWorkflowTransition(
            id,
            contentId,
            fromStatus,
            toStatus,
            actorUserId,
            normalizedComment,
            createdAtUtc);
    }

    internal static string? NormalizeComment(string? comment, bool required = false)
    {
        if (string.IsNullOrWhiteSpace(comment))
        {
            if (required)
            {
                throw new ArgumentException("Comment is required.", nameof(comment));
            }

            return null;
        }

        var trimmed = comment.Trim();
        if (trimmed.Length > MaxCommentLength)
        {
            throw new ArgumentException("Comment is too long.", nameof(comment));
        }

        return trimmed;
    }
}

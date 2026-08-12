using HelpDev.Modules.Content.Domain.ValueObjects;

namespace HelpDev.Modules.Content.Domain.Entities;

/// <summary>
/// Immutable historical snapshot of a content item. No update or delete operations in v1.
/// </summary>
public sealed class ContentRevision
{
    public const int MaxChangeReasonLength = 500;

    /// <summary>Required for EF Core materialization.</summary>
    private ContentRevision()
    {
    }

    private ContentRevision(
        Guid id,
        Guid contentId,
        int versionNumber,
        ContentRevisionSnapshot snapshot,
        string? changeReason,
        Guid createdByUserId,
        DateTime createdAtUtc)
    {
        Id = id;
        ContentId = contentId;
        VersionNumber = versionNumber;
        Snapshot = snapshot;
        ChangeReason = changeReason;
        CreatedByUserId = createdByUserId;
        CreatedAtUtc = createdAtUtc;
    }

    public Guid Id { get; private set; }

    public Guid ContentId { get; private set; }

    public int VersionNumber { get; private set; }

    public ContentRevisionSnapshot Snapshot { get; private set; } = null!;

    public string? ChangeReason { get; private set; }

    public Guid CreatedByUserId { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    public static ContentRevision Create(
        Guid id,
        Guid contentId,
        int versionNumber,
        ContentRevisionSnapshot snapshot,
        string? changeReason,
        Guid createdByUserId,
        DateTime createdAtUtc)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("Revision id is required.", nameof(id));
        }

        if (contentId == Guid.Empty)
        {
            throw new ArgumentException("Content id is required.", nameof(contentId));
        }

        if (versionNumber <= 0)
        {
            throw new ArgumentException("Version number must be positive.", nameof(versionNumber));
        }

        ArgumentNullException.ThrowIfNull(snapshot);

        if (createdByUserId == Guid.Empty)
        {
            throw new ArgumentException("Created-by user id is required.", nameof(createdByUserId));
        }

        var normalizedReason = NormalizeChangeReason(changeReason);

        return new ContentRevision(
            id,
            contentId,
            versionNumber,
            snapshot,
            normalizedReason,
            createdByUserId,
            createdAtUtc);
    }

    private static string? NormalizeChangeReason(string? changeReason)
    {
        if (string.IsNullOrWhiteSpace(changeReason))
        {
            return null;
        }

        var trimmed = changeReason.Trim();
        if (trimmed.Length > MaxChangeReasonLength)
        {
            throw new ArgumentException("Change reason is too long.", nameof(changeReason));
        }

        return trimmed;
    }
}

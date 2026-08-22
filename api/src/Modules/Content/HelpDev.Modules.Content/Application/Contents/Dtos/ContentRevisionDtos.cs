namespace HelpDev.Modules.Content.Application.Contents.Dtos;

public sealed record ContentRevisionListItemDto(
    int VersionNumber,
    Guid CreatedByUserId,
    DateTime CreatedAtUtc,
    string? ChangeReason);

public sealed record ContentRevisionSnapshotDto(
    string Title,
    string Slug,
    string Body,
    string Excerpt,
    string? CoverImage,
    string ContentType,
    SeoMetadataDto SeoMetadata,
    string? ContentJson = null,
    string? ContentHtml = null,
    string? ContentFormat = null,
    string? EditorVersion = null,
    int? WordCount = null,
    int? ReadingTimeMinutes = null);

public sealed record ContentRevisionDetailDto(
    Guid ContentId,
    int VersionNumber,
    ContentRevisionSnapshotDto Snapshot,
    string? ChangeReason,
    Guid CreatedByUserId,
    DateTime CreatedAtUtc);

public sealed record RestoreContentRevisionRequest(string? ChangeReason);

namespace HelpDev.Modules.Content.Application.Contents.Dtos;

/// <summary>
/// Admin CMS list projection. Exposes only read-model fields, never domain entities or EF models.
/// </summary>
public sealed record AdminContentListItemDto(
    Guid Id,
    string Title,
    string Slug,
    string ContentType,
    string ContentStatus,
    Guid AuthorId,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc,
    DateTime? PublishedAtUtc);

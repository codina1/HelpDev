namespace HelpDev.Modules.Content.Application.Contents.Dtos;

/// <summary>
/// Full management projection returned after admin update/publish operations.
/// </summary>
public sealed record AdminContentDetailDto(
    Guid Id,
    string Title,
    string Slug,
    string Body,
    string Excerpt,
    string? CoverImage,
    string ContentType,
    string ContentStatus,
    Guid AuthorId,
    int Views,
    int Saves,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc,
    DateTime? PublishedAtUtc,
    SeoMetadataDto Seo);

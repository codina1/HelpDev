namespace HelpDev.Modules.Content.Application.Contents.Dtos;

public sealed record ContentListItemDto(
    Guid Id,
    string Title,
    string Slug,
    string Type,
    Guid AuthorId,
    int Views,
    int Saves,
    DateTime CreatedAt,
    string? CoverImage = null,
    string? AuthorName = null,
    string? AuthorRole = null,
    string? AuthorAvatarUrl = null);

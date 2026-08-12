namespace HelpDev.Modules.Content.Application.Articles.Dtos;

public sealed record ArticleMetadataDto(
    Guid Id,
    Guid ContentId,
    Guid? CategoryId,
    string DifficultyLevel,
    int ReadingTimeMinutes,
    bool IsFeatured,
    bool AllowComments,
    bool TableOfContentsEnabled,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc);

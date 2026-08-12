namespace HelpDev.Modules.Search.Application.Contracts;

public sealed record SearchSourceDocument(
    Guid SourceId,
    string SourceType,
    string Title,
    string Slug,
    string Summary,
    string Url,
    bool IsPublished,
    DateTime? PublishedAtUtc,
    DateTime UpdatedAtUtc,
    string? Body = null);

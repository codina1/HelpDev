namespace HelpDev.Modules.Search.Application.Contracts;

/// <summary>
/// Unified searchable knowledge entity (Search-side DTO — no source-module types).
/// </summary>
public sealed record KnowledgeDocument(
    string SourceType,
    Guid SourceId,
    string Title,
    string Slug,
    string Summary,
    string Url,
    bool Published,
    DateTime UpdatedAtUtc,
    string? Body = null,
    DateTime? PublishedAtUtc = null)
{
    public SearchSourceDocument ToSearchSourceDocument() =>
        new(
            SourceId,
            SourceType,
            Title,
            Slug,
            Summary,
            Url,
            Published,
            PublishedAtUtc,
            UpdatedAtUtc,
            Body);

    public static KnowledgeDocument From(SearchSourceDocument document) =>
        new(
            document.SourceType,
            document.SourceId,
            document.Title,
            document.Slug,
            document.Summary,
            document.Url,
            document.IsPublished,
            document.UpdatedAtUtc,
            document.Body,
            document.PublishedAtUtc);
}

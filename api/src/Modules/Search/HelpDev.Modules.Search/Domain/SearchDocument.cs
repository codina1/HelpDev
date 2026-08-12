namespace HelpDev.Modules.Search.Domain;

/// <summary>
/// PostgreSQL-backed search read model. Not a Domain Aggregate for other modules.
/// </summary>
public sealed class SearchDocument
{
    public Guid Id { get; set; }

    public string SourceType { get; set; } = string.Empty;

    public Guid SourceId { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Slug { get; set; } = string.Empty;

    public string Summary { get; set; } = string.Empty;

    public string Url { get; set; } = string.Empty;

    public bool IsPublished { get; set; }

    public DateTime? SourcePublishedAtUtc { get; set; }

    public DateTime SourceUpdatedAtUtc { get; set; }

    public DateTime IndexedAtUtc { get; set; }

    public Guid LastEventId { get; set; }
}

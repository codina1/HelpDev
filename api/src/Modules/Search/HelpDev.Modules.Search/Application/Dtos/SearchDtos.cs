namespace HelpDev.Modules.Search.Application.Dtos;

public sealed record SearchResultDto(
    string Query,
    int Page,
    int PageSize,
    int Total,
    IReadOnlyList<SearchItemDto> Items);

public sealed record SearchItemDto(
    string SourceType,
    Guid SourceId,
    string Title,
    string Slug,
    string Summary,
    string Url,
    DateTime? PublishedAtUtc,
    DateTime UpdatedAtUtc);

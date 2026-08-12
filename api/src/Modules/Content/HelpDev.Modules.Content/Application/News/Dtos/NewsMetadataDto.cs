namespace HelpDev.Modules.Content.Application.News.Dtos;

public sealed record NewsMetadataDto(
    Guid Id,
    Guid ContentId,
    string SourceName,
    string? SourceUrl,
    DateTime NewsDateUtc,
    string Priority,
    string? ExternalReference,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc);

namespace HelpDev.Modules.Content.Application.Contents.Dtos;

public sealed record ContentDetailDto(
    Guid Id,
    string Title,
    string Slug,
    string Body,
    string Type,
    Guid AuthorId,
    string Status,
    int Views,
    int Saves,
    DateTime CreatedAt);

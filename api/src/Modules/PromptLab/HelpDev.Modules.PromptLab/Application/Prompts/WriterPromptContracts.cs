namespace HelpDev.Modules.PromptLab.Application.Prompts;

public sealed record WriterPromptListItemDto(
    Guid Id,
    string Title,
    string Slug,
    string? Description,
    string? CoverImage,
    string MediaType,
    Guid CategoryId,
    Guid AiModelId,
    string Status,
    int Views,
    int CopyCount,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    DateTime? PublishedAt);

public sealed record WriterPromptDetailsDto(
    Guid Id,
    string Title,
    string Slug,
    string? Description,
    string Content,
    string? CoverImage,
    string MediaType,
    Guid CategoryId,
    Guid AiModelId,
    string Status,
    int Views,
    int CopyCount,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    DateTime? PublishedAt);

public sealed record WriterPromptPageDto(
    int Page,
    int PageSize,
    int Total,
    IReadOnlyList<WriterPromptListItemDto> Items);

public sealed record WriterPromptFilter(
    string? Status,
    int Page,
    int PageSize);

public sealed record CreateWriterPromptRequest(
    string Title,
    string Slug,
    string? Description,
    string Content,
    string? CoverImage,
    string MediaType,
    Guid CategoryId,
    Guid AiModelId);

public sealed record UpdateWriterPromptRequest(
    string Title,
    string Slug,
    string? Description,
    string Content,
    string? CoverImage,
    string MediaType,
    Guid CategoryId,
    Guid AiModelId);

public interface IPromptWriterQueries
{
    Task<WriterPromptPageDto> GetMyPromptsAsync(
        Guid authorId,
        WriterPromptFilter filter,
        CancellationToken cancellationToken = default);

    Task<WriterPromptDetailsDto?> GetMyByIdAsync(
        Guid authorId,
        Guid id,
        CancellationToken cancellationToken = default);
}

public interface IPromptWriterService
{
    Task<WriterPromptDetailsDto> CreateAsync(
        Guid authorId,
        CreateWriterPromptRequest request,
        CancellationToken cancellationToken = default);

    Task<WriterPromptDetailsDto> UpdateAsync(
        Guid authorId,
        Guid id,
        UpdateWriterPromptRequest request,
        CancellationToken cancellationToken = default);

    Task<WriterPromptDetailsDto> SubmitAsync(
        Guid authorId,
        Guid id,
        CancellationToken cancellationToken = default);
}

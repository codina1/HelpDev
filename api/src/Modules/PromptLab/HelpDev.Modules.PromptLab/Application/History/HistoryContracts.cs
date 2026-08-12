namespace HelpDev.Modules.PromptLab.Application.History;

public sealed record PromptRenderHistoryFilter(
    Guid? PromptId,
    bool? Succeeded,
    int Page,
    int PageSize);

public sealed record PromptRenderHistoryItemDto(
    Guid Id,
    Guid PromptId,
    string PromptSlug,
    string PromptName,
    int VersionNumber,
    bool Succeeded,
    int DurationMilliseconds,
    string? InputPreview,
    string? RenderedPreview,
    string? ErrorCode,
    DateTime RenderedAtUtc);

public sealed record PromptRenderHistoryPageDto(
    int Page,
    int PageSize,
    int Total,
    IReadOnlyList<PromptRenderHistoryItemDto> Items);

public interface IPromptRenderHistoryQueries
{
    Task<PromptRenderHistoryPageDto> GetMyHistoryAsync(
        Guid userId,
        PromptRenderHistoryFilter filter,
        CancellationToken cancellationToken = default);

    Task<PromptRenderHistoryItemDto?> GetMyRenderAsync(
        Guid userId,
        Guid id,
        CancellationToken cancellationToken = default);
}

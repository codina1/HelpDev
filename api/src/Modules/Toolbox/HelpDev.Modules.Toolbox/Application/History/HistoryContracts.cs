namespace HelpDev.Modules.Toolbox.Application.History;

public sealed record ToolExecutionHistoryFilter(
    Guid? ToolId,
    bool? Succeeded,
    int Page,
    int PageSize);

public sealed record ToolExecutionHistoryItemDto(
    Guid Id,
    Guid ToolId,
    string ToolSlug,
    string ToolName,
    string Type,
    bool Succeeded,
    int DurationMilliseconds,
    string? InputPreview,
    string? OutputPreview,
    string? ErrorCode,
    DateTime ExecutedAtUtc);

public sealed record ToolExecutionHistoryPageDto(
    int Page,
    int PageSize,
    int Total,
    IReadOnlyList<ToolExecutionHistoryItemDto> Items);

public interface IToolExecutionHistoryQueries
{
    Task<ToolExecutionHistoryPageDto> GetMyHistoryAsync(
        Guid userId,
        ToolExecutionHistoryFilter filter,
        CancellationToken cancellationToken = default);

    Task<ToolExecutionHistoryItemDto?> GetMyExecutionAsync(
        Guid userId,
        Guid id,
        CancellationToken cancellationToken = default);
}

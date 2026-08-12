using System.Text.Json;

namespace HelpDev.Modules.Toolbox.Application.Execution;

public sealed record ExecuteToolRequest(JsonElement Input);

public sealed record ToolExecutionResultDto(
    Guid? ExecutionId,
    string ToolSlug,
    string Type,
    bool Succeeded,
    JsonElement Output,
    string? ErrorCode,
    string? ErrorMessage,
    int DurationMilliseconds,
    bool IsTruncated,
    DateTime CompletedAtUtc);

public interface IToolExecutionService
{
    Task<ToolExecutionResultDto> ExecuteAsync(
        string slug,
        ExecuteToolRequest request,
        Guid? userId = null,
        CancellationToken cancellationToken = default);
}

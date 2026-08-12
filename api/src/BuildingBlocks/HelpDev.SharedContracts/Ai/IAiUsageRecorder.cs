namespace HelpDev.SharedContracts.Ai;

/// <summary>Persistence port for AI usage telemetry (no prompts or generated text).</summary>
public interface IAiUsageRecorder
{
    Task RecordAsync(AiUsageRecordInput input, CancellationToken cancellationToken = default);
}

public sealed record AiUsageRecordInput(
    Guid? UserId,
    string TaskType,
    string Provider,
    string Model,
    int InputTokens,
    int OutputTokens,
    Guid? ContentId,
    bool Success = true,
    int DurationMs = 0,
    string? ErrorCode = null);

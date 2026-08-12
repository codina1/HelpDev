namespace HelpDev.Modules.Analytics.Domain.AiUsage;

/// <summary>
/// Persisted AI usage telemetry. Never stores prompts, generated text, or secrets.
/// </summary>
public sealed class AiUsageRecord
{
    private AiUsageRecord()
    {
    }

    public Guid Id { get; private set; }

    /// <summary>Nullable for anonymous RAG / system operations.</summary>
    public Guid? UserId { get; private set; }

    public string TaskType { get; private set; } = string.Empty;

    public string Provider { get; private set; } = string.Empty;

    public string Model { get; private set; } = string.Empty;

    public int InputTokens { get; private set; }

    public int OutputTokens { get; private set; }

    public Guid? ContentId { get; private set; }

    public bool Success { get; private set; }

    public int DurationMs { get; private set; }

    public string? ErrorCode { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    public static AiUsageRecord Create(
        Guid id,
        Guid? userId,
        string taskType,
        string provider,
        string model,
        int inputTokens,
        int outputTokens,
        Guid? contentId,
        DateTime createdAtUtc,
        bool success = true,
        int durationMs = 0,
        string? errorCode = null)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("Id is required.");
        }

        if (userId == Guid.Empty)
        {
            userId = null;
        }

        if (string.IsNullOrWhiteSpace(taskType) || taskType.Length > 64)
        {
            throw new ArgumentException("TaskType is invalid.");
        }

        if (string.IsNullOrWhiteSpace(provider) || provider.Length > 64)
        {
            throw new ArgumentException("Provider is invalid.");
        }

        if (string.IsNullOrWhiteSpace(model) || model.Length > 100)
        {
            throw new ArgumentException("Model is invalid.");
        }

        if (inputTokens < 0 || outputTokens < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(inputTokens));
        }

        if (durationMs < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(durationMs));
        }

        if (errorCode is { Length: > 64 })
        {
            throw new ArgumentException("ErrorCode is invalid.");
        }

        return new AiUsageRecord
        {
            Id = id,
            UserId = userId,
            TaskType = taskType.Trim(),
            Provider = provider.Trim(),
            Model = model.Trim(),
            InputTokens = inputTokens,
            OutputTokens = outputTokens,
            ContentId = contentId,
            Success = success,
            DurationMs = durationMs,
            ErrorCode = string.IsNullOrWhiteSpace(errorCode) ? null : errorCode.Trim(),
            CreatedAtUtc = createdAtUtc,
        };
    }
}

using HelpDev.Modules.Toolbox.Domain.Tools;
using HelpDev.SharedKernel.Common;
using HelpDev.SharedKernel.Exceptions;

namespace HelpDev.Modules.Toolbox.Domain.Execution;

public sealed class ToolExecutionRecord : AggregateRoot<Guid>
{
    private ToolExecutionRecord()
    {
    }

    private ToolExecutionRecord(Guid id)
        : base(id)
    {
    }

    public Guid ToolId { get; private set; }

    public Guid? UserId { get; private set; }

    public ToolType ToolType { get; private set; }

    public bool Succeeded { get; private set; }

    public int DurationMilliseconds { get; private set; }

    public string? InputPreview { get; private set; }

    public string? OutputPreview { get; private set; }

    public string? ErrorCode { get; private set; }

    public DateTime ExecutedAtUtc { get; private set; }

    public static ToolExecutionRecord Create(
        Guid id,
        Guid toolId,
        Guid userId,
        ToolType toolType,
        bool succeeded,
        int durationMilliseconds,
        string? inputPreview,
        string? outputPreview,
        string? errorCode,
        DateTime executedAtUtc)
    {
        if (id == Guid.Empty || toolId == Guid.Empty || userId == Guid.Empty)
        {
            throw new DomainException("Execution record identifiers are invalid.", ToolboxErrorCodes.ExecutionFailed);
        }

        if (durationMilliseconds < 0)
        {
            throw new DomainException("Duration must be non-negative.", ToolboxErrorCodes.ExecutionFailed);
        }

        return new ToolExecutionRecord(id)
        {
            ToolId = toolId,
            UserId = userId,
            ToolType = toolType,
            Succeeded = succeeded,
            DurationMilliseconds = durationMilliseconds,
            InputPreview = BoundPreview(inputPreview),
            OutputPreview = BoundPreview(outputPreview),
            ErrorCode = string.IsNullOrWhiteSpace(errorCode) ? null : errorCode.Trim(),
            ExecutedAtUtc = executedAtUtc,
        };
    }

    private static string? BoundPreview(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var trimmed = value.Trim();
        if (trimmed.Length <= ToolboxLimits.MaxHistoryInputPreview)
        {
            return Redact(trimmed);
        }

        return Redact(trimmed[..ToolboxLimits.MaxHistoryInputPreview]);
    }

    private static string Redact(string value)
    {
        // Conservative redaction for secret-looking keys in previews.
        return value
            .Replace("password", "***", StringComparison.OrdinalIgnoreCase)
            .Replace("secret", "***", StringComparison.OrdinalIgnoreCase)
            .Replace("apikey", "***", StringComparison.OrdinalIgnoreCase)
            .Replace("token", "***", StringComparison.OrdinalIgnoreCase);
    }
}

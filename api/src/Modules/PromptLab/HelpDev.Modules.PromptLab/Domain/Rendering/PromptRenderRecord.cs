using HelpDev.SharedKernel.Common;
using HelpDev.SharedKernel.Exceptions;

namespace HelpDev.Modules.PromptLab.Domain.Rendering;

public sealed class PromptRenderRecord : AggregateRoot<Guid>
{
    private PromptRenderRecord()
    {
    }

    private PromptRenderRecord(Guid id)
        : base(id)
    {
    }

    public Guid PromptDefinitionId { get; private set; }

    public Guid PromptVersionId { get; private set; }

    public int VersionNumber { get; private set; }

    public Guid UserId { get; private set; }

    public bool Succeeded { get; private set; }

    public int DurationMilliseconds { get; private set; }

    public string? InputPreview { get; private set; }

    public string? RenderedPreview { get; private set; }

    public string? ErrorCode { get; private set; }

    public DateTime RenderedAtUtc { get; private set; }

    public static PromptRenderRecord Create(
        Guid id,
        Guid promptDefinitionId,
        Guid promptVersionId,
        int versionNumber,
        Guid userId,
        bool succeeded,
        int durationMilliseconds,
        string? inputPreview,
        string? renderedPreview,
        string? errorCode,
        DateTime renderedAtUtc)
    {
        if (id == Guid.Empty
            || promptDefinitionId == Guid.Empty
            || promptVersionId == Guid.Empty
            || userId == Guid.Empty)
        {
            throw new DomainException("Render record identifiers are invalid.", PromptLabErrorCodes.RenderFailed);
        }

        if (versionNumber < 1)
        {
            throw new DomainException("Version number must be >= 1.", PromptLabErrorCodes.RenderFailed);
        }

        if (durationMilliseconds < 0)
        {
            throw new DomainException("Duration must be non-negative.", PromptLabErrorCodes.RenderFailed);
        }

        return new PromptRenderRecord(id)
        {
            PromptDefinitionId = promptDefinitionId,
            PromptVersionId = promptVersionId,
            VersionNumber = versionNumber,
            UserId = userId,
            Succeeded = succeeded,
            DurationMilliseconds = durationMilliseconds,
            InputPreview = BoundPreview(inputPreview, PromptLabLimits.MaxHistoryInputPreview),
            RenderedPreview = BoundPreview(renderedPreview, PromptLabLimits.MaxHistoryOutputPreview),
            ErrorCode = string.IsNullOrWhiteSpace(errorCode) ? null : errorCode.Trim(),
            RenderedAtUtc = renderedAtUtc,
        };
    }

    private static string? BoundPreview(string? value, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var trimmed = value.Trim();
        if (trimmed.Length <= maxLength)
        {
            return Redact(trimmed);
        }

        return Redact(trimmed[..maxLength]);
    }

    private static string Redact(string value)
    {
        return value
            .Replace("password", "***", StringComparison.OrdinalIgnoreCase)
            .Replace("secret", "***", StringComparison.OrdinalIgnoreCase)
            .Replace("apikey", "***", StringComparison.OrdinalIgnoreCase)
            .Replace("token", "***", StringComparison.OrdinalIgnoreCase);
    }
}

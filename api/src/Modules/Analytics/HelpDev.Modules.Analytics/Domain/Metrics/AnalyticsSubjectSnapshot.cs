using HelpDev.SharedKernel.Exceptions;

namespace HelpDev.Modules.Analytics.Domain.Metrics;

public sealed class AnalyticsSubjectSnapshot
{
    private AnalyticsSubjectSnapshot()
    {
    }

    public Guid Id { get; private set; }

    public string SubjectType { get; private set; } = string.Empty;

    public Guid SubjectId { get; private set; }

    public string DisplayName { get; private set; } = string.Empty;

    public string? Slug { get; private set; }

    public DateTime UpdatedAtUtc { get; private set; }

    public static AnalyticsSubjectSnapshot Create(
        Guid id,
        string subjectType,
        Guid subjectId,
        string displayName,
        string? slug,
        DateTime updatedAtUtc)
    {
        if (id == Guid.Empty || subjectId == Guid.Empty)
        {
            throw new DomainException(AnalyticsErrorCodes.EventProcessingFailed, "Snapshot id is required.");
        }

        if (!AnalyticsSubjectTypes.IsSupported(subjectType))
        {
            throw new DomainException(AnalyticsErrorCodes.SubjectTypeInvalid, "Subject type is invalid.");
        }

        var trimmedName = displayName?.Trim() ?? string.Empty;
        if (trimmedName.Length == 0 || trimmedName.Length > AnalyticsLimits.MaxDisplayNameLength)
        {
            throw new DomainException(AnalyticsErrorCodes.EventProcessingFailed, "Display name is invalid.");
        }

        string? normalizedSlug = null;
        if (!string.IsNullOrWhiteSpace(slug))
        {
            normalizedSlug = slug.Trim().ToLowerInvariant();
            if (normalizedSlug.Length > AnalyticsLimits.MaxSlugLength)
            {
                throw new DomainException(AnalyticsErrorCodes.EventProcessingFailed, "Slug is invalid.");
            }
        }

        return new AnalyticsSubjectSnapshot
        {
            Id = id,
            SubjectType = subjectType,
            SubjectId = subjectId,
            DisplayName = trimmedName,
            Slug = normalizedSlug,
            UpdatedAtUtc = updatedAtUtc,
        };
    }

    public bool Update(string displayName, string? slug, DateTime updatedAtUtc)
    {
        var trimmedName = displayName?.Trim() ?? string.Empty;
        if (trimmedName.Length == 0 || trimmedName.Length > AnalyticsLimits.MaxDisplayNameLength)
        {
            throw new DomainException(AnalyticsErrorCodes.EventProcessingFailed, "Display name is invalid.");
        }

        string? normalizedSlug = null;
        if (!string.IsNullOrWhiteSpace(slug))
        {
            normalizedSlug = slug.Trim().ToLowerInvariant();
            if (normalizedSlug.Length > AnalyticsLimits.MaxSlugLength)
            {
                throw new DomainException(AnalyticsErrorCodes.EventProcessingFailed, "Slug is invalid.");
            }
        }

        var changed = !string.Equals(DisplayName, trimmedName, StringComparison.Ordinal)
            || !string.Equals(Slug, normalizedSlug, StringComparison.Ordinal);

        if (!changed)
        {
            return false;
        }

        DisplayName = trimmedName;
        Slug = normalizedSlug;
        UpdatedAtUtc = updatedAtUtc;
        return true;
    }
}

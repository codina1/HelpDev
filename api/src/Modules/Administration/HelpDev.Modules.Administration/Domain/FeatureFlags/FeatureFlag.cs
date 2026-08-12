using HelpDev.SharedKernel.Common;
using HelpDev.SharedKernel.Exceptions;

namespace HelpDev.Modules.Administration.Domain.FeatureFlags;

public sealed class FeatureFlag : AggregateRoot<Guid>
{
    public const int KeyMaxLength = 100;
    public const int DescriptionMaxLength = 500;

    /// <summary>Required for EF Core materialization.</summary>
    private FeatureFlag()
    {
    }

    private FeatureFlag(Guid id)
        : base(id)
    {
    }

    public string Key { get; private set; } = string.Empty;

    public bool IsEnabled { get; private set; }

    public string? Description { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    public DateTime UpdatedAtUtc { get; private set; }

    public static FeatureFlag Create(
        Guid id,
        string key,
        bool isEnabled,
        string? description,
        DateTime utcNow)
    {
        if (id == Guid.Empty)
        {
            throw new DomainException("Feature flag id must not be empty.", AdministrationErrorCodes.FeatureKeyInvalid);
        }

        var normalizedKey = NormalizeKey(key);
        var normalizedDescription = NormalizeDescription(description);

        return new FeatureFlag(id)
        {
            Key = normalizedKey,
            IsEnabled = isEnabled,
            Description = normalizedDescription,
            CreatedAtUtc = utcNow,
            UpdatedAtUtc = utcNow,
        };
    }

    public bool Enable(DateTime utcNow)
    {
        if (IsEnabled)
        {
            return false;
        }

        IsEnabled = true;
        UpdatedAtUtc = utcNow;
        return true;
    }

    public bool Disable(DateTime utcNow)
    {
        if (!IsEnabled)
        {
            return false;
        }

        IsEnabled = false;
        UpdatedAtUtc = utcNow;
        return true;
    }

    public bool UpdateState(bool isEnabled, DateTime utcNow) =>
        isEnabled ? Enable(utcNow) : Disable(utcNow);

    public bool UpdateDescription(string? description, DateTime utcNow)
    {
        var normalized = NormalizeDescription(description);
        if (string.Equals(Description, normalized, StringComparison.Ordinal))
        {
            return false;
        }

        Description = normalized;
        UpdatedAtUtc = utcNow;
        return true;
    }

    public static string NormalizeKey(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            throw new DomainException("Feature flag key is required.", AdministrationErrorCodes.FeatureKeyRequired);
        }

        var trimmed = key.Trim();
        if (trimmed.Length > KeyMaxLength)
        {
            throw new DomainException(
                $"Feature flag key must be at most {KeyMaxLength} characters.",
                AdministrationErrorCodes.FeatureKeyInvalid);
        }

        if (trimmed.Any(ch => !(char.IsLetterOrDigit(ch) || ch is '_' or '-' or '.')))
        {
            throw new DomainException(
                "Feature flag key may contain only letters, digits, underscore, hyphen, or period.",
                AdministrationErrorCodes.FeatureKeyInvalid);
        }

        return trimmed;
    }

    private static string? NormalizeDescription(string? description)
    {
        if (description is null)
        {
            return null;
        }

        var trimmed = description.Trim();
        if (trimmed.Length == 0)
        {
            return null;
        }

        if (trimmed.Length > DescriptionMaxLength)
        {
            throw new DomainException(
                $"Feature flag description must be at most {DescriptionMaxLength} characters.",
                AdministrationErrorCodes.FeatureDescriptionInvalid);
        }

        return trimmed;
    }
}

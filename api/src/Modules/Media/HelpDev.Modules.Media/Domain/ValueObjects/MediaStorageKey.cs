using HelpDev.SharedKernel.Exceptions;

namespace HelpDev.Modules.Media.Domain.ValueObjects;

/// <summary>Server-owned storage key. Never equals the raw user filename.</summary>
public sealed class MediaStorageKey : IEquatable<MediaStorageKey>
{
    public const int MaxLength = 260;

    private MediaStorageKey(string value) => Value = value;

    public string Value { get; }

    public static MediaStorageKey Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new DomainException("کلید ذخیره‌سازی الزامی است.");
        }

        var normalized = value.Replace('\\', '/').Trim().TrimStart('/');
        if (normalized.Length == 0 || normalized.Length > MaxLength)
        {
            throw new DomainException("کلید ذخیره‌سازی معتبر نیست.");
        }

        if (normalized.Contains("..", StringComparison.Ordinal)
            || normalized.Contains(':', StringComparison.Ordinal)
            || normalized.StartsWith("/", StringComparison.Ordinal))
        {
            throw new DomainException("کلید ذخیره‌سازی معتبر نیست.");
        }

        return new MediaStorageKey(normalized);
    }

    public bool Equals(MediaStorageKey? other) =>
        other is not null && string.Equals(Value, other.Value, StringComparison.Ordinal);

    public override bool Equals(object? obj) => obj is MediaStorageKey other && Equals(other);

    public override int GetHashCode() => StringComparer.Ordinal.GetHashCode(Value);

    public override string ToString() => Value;
}

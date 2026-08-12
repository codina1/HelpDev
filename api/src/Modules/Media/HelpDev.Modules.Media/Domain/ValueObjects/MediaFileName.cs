using HelpDev.SharedKernel.Exceptions;

namespace HelpDev.Modules.Media.Domain.ValueObjects;

/// <summary>Normalized original client filename (display only — never used as a storage path).</summary>
public sealed class MediaFileName : IEquatable<MediaFileName>
{
    public const int MaxLength = 200;

    private MediaFileName(string value) => Value = value;

    public string Value { get; }

    public static MediaFileName Create(string? raw, int maxLength = MaxLength)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            throw new DomainException("نام فایل الزامی است.");
        }

        var trimmed = raw.Trim();
        // Strip path segments that clients may include.
        trimmed = Path.GetFileName(trimmed.Replace('\\', '/'));
        if (string.IsNullOrWhiteSpace(trimmed))
        {
            throw new DomainException("نام فایل معتبر نیست.");
        }

        // Remove control characters and nulls.
        var cleaned = new string(trimmed.Where(ch => !char.IsControl(ch) && ch != '\0').ToArray());
        if (cleaned.Length == 0)
        {
            throw new DomainException("نام فایل معتبر نیست.");
        }

        if (cleaned.Length > maxLength)
        {
            cleaned = cleaned[..maxLength];
        }

        return new MediaFileName(cleaned);
    }

    public bool Equals(MediaFileName? other) =>
        other is not null && string.Equals(Value, other.Value, StringComparison.Ordinal);

    public override bool Equals(object? obj) => obj is MediaFileName other && Equals(other);

    public override int GetHashCode() => StringComparer.Ordinal.GetHashCode(Value);

    public override string ToString() => Value;
}

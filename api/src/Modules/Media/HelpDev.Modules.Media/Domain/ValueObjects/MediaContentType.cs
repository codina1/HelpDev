using HelpDev.SharedKernel.Exceptions;

namespace HelpDev.Modules.Media.Domain.ValueObjects;

/// <summary>Verified image content type from the allowlist (not client-declared MIME alone).</summary>
public sealed class MediaContentType : IEquatable<MediaContentType>
{
    public const string Jpeg = "image/jpeg";
    public const string Png = "image/png";
    public const string Webp = "image/webp";

    public static readonly IReadOnlySet<string> Allowed =
        new HashSet<string>(StringComparer.OrdinalIgnoreCase) { Jpeg, Png, Webp };

    private MediaContentType(string value) => Value = value;

    public string Value { get; }

    public string SafeExtension => Value.ToLowerInvariant() switch
    {
        Jpeg => ".jpg",
        Png => ".png",
        Webp => ".webp",
        _ => throw new DomainException("نوع محتوا پشتیبانی نمی‌شود."),
    };

    public static MediaContentType Create(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new DomainException("نوع محتوا الزامی است.");
        }

        var normalized = value.Trim().ToLowerInvariant();
        if (!Allowed.Contains(normalized))
        {
            throw new DomainException("نوع تصویر پشتیبانی نمی‌شود.");
        }

        return new MediaContentType(normalized);
    }

    public bool Equals(MediaContentType? other) =>
        other is not null && string.Equals(Value, other.Value, StringComparison.OrdinalIgnoreCase);

    public override bool Equals(object? obj) => obj is MediaContentType other && Equals(other);

    public override int GetHashCode() => StringComparer.OrdinalIgnoreCase.GetHashCode(Value);

    public override string ToString() => Value;
}

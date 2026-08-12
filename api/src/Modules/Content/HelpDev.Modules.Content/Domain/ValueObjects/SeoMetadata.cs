using HelpDev.SharedKernel.Common;
using HelpDev.SharedKernel.Exceptions;

namespace HelpDev.Modules.Content.Domain.ValueObjects;

/// <summary>
/// Optional SEO metadata for a content item. All members are optional; an all-null instance
/// represents "no SEO configured". Values are normalized (trimmed, blanks → null) and validated.
/// </summary>
public sealed class SeoMetadata : ValueObject
{
    public const int MaxSeoTitleLength = 70;
    public const int MaxSeoDescriptionLength = 160;
    public const int MaxCanonicalUrlLength = 2048;
    public const int MaxOgImageLength = 2048;
    public const int MaxFocusKeywordLength = 100;

    /// <summary>Non-validating constructor used by EF Core complex-type materialization.</summary>
    private SeoMetadata(
        string? seoTitle,
        string? seoDescription,
        string? canonicalUrl,
        string? ogImage,
        string? focusKeyword)
    {
        SeoTitle = seoTitle;
        SeoDescription = seoDescription;
        CanonicalUrl = canonicalUrl;
        OgImage = ogImage;
        FocusKeyword = focusKeyword;
    }

    public static SeoMetadata Empty { get; } = new(null, null, null, null, null);

    public string? SeoTitle { get; private set; }

    public string? SeoDescription { get; private set; }

    public string? CanonicalUrl { get; private set; }

    public string? OgImage { get; private set; }

    public string? FocusKeyword { get; private set; }

    public bool IsEmpty =>
        SeoTitle is null
        && SeoDescription is null
        && CanonicalUrl is null
        && OgImage is null
        && FocusKeyword is null;

    /// <summary>
    /// Creates a validated, normalized SEO metadata value. Throws <see cref="DomainException"/>
    /// for invalid input (over-length fields or a non-absolute canonical URL).
    /// </summary>
    public static SeoMetadata Create(
        string? seoTitle,
        string? seoDescription,
        string? canonicalUrl,
        string? ogImage,
        string? focusKeyword)
    {
        var normalizedTitle = NormalizeLength(seoTitle, MaxSeoTitleLength, "عنوان سئو");
        var normalizedDescription = NormalizeLength(seoDescription, MaxSeoDescriptionLength, "توضیحات سئو");
        var normalizedCanonical = NormalizeCanonicalUrl(canonicalUrl);
        var normalizedOgImage = NormalizeLength(ogImage, MaxOgImageLength, "تصویر OG");
        var normalizedFocusKeyword = NormalizeLength(focusKeyword, MaxFocusKeywordLength, "کلمه کلیدی");

        return new SeoMetadata(
            normalizedTitle,
            normalizedDescription,
            normalizedCanonical,
            normalizedOgImage,
            normalizedFocusKeyword);
    }

    private static string? NormalizeLength(string? value, int maxLength, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var normalized = value.Trim();
        if (normalized.Length > maxLength)
        {
            throw new DomainException($"{fieldName} بیش از حد مجاز است.");
        }

        return normalized;
    }

    private static string? NormalizeCanonicalUrl(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var normalized = value.Trim();
        if (normalized.Length > MaxCanonicalUrlLength)
        {
            throw new DomainException("آدرس کاننیکال بیش از حد مجاز است.");
        }

        if (!Uri.TryCreate(normalized, UriKind.Absolute, out var uri)
            || (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
        {
            throw new DomainException("آدرس کاننیکال معتبر نیست.");
        }

        return normalized;
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return SeoTitle;
        yield return SeoDescription;
        yield return CanonicalUrl;
        yield return OgImage;
        yield return FocusKeyword;
    }
}

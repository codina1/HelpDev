using HelpDev.Modules.Content.Domain.Enums;
using HelpDev.SharedKernel.Common;
using ContentEntity = HelpDev.Modules.Content.Domain.Entities.Content;

namespace HelpDev.Modules.Content.Domain.ValueObjects;

/// <summary>
/// Immutable point-in-time content state stored inside a revision. Independent from the
/// <see cref="ContentEntity"/> aggregate shape so historical rows remain interpretable.
/// </summary>
public sealed class ContentRevisionSnapshot : ValueObject
{
    private ContentRevisionSnapshot(
        string title,
        string slug,
        string body,
        string excerpt,
        string? coverImage,
        string contentType,
        ContentRevisionSeoSnapshot seoMetadata)
    {
        Title = title;
        Slug = slug;
        Body = body;
        Excerpt = excerpt;
        CoverImage = coverImage;
        ContentType = contentType;
        SeoMetadata = seoMetadata;
    }

    public string Title { get; }

    public string Slug { get; }

    public string Body { get; }

    public string Excerpt { get; }

    public string? CoverImage { get; }

    public string ContentType { get; }

    public ContentRevisionSeoSnapshot SeoMetadata { get; }

    public static ContentRevisionSnapshot FromContent(ContentEntity content)
    {
        ArgumentNullException.ThrowIfNull(content);

        return new ContentRevisionSnapshot(
            content.Title,
            content.Slug.Value,
            content.Body,
            content.Excerpt,
            content.CoverImage,
            content.Type.ToString(),
            ContentRevisionSeoSnapshot.From(content.SeoMetadata));
    }

    public static ContentRevisionSnapshot Create(
        string title,
        string slug,
        string body,
        string excerpt,
        string? coverImage,
        string contentType,
        ContentRevisionSeoSnapshot seoMetadata)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new ArgumentException("Title is required.", nameof(title));
        }

        if (string.IsNullOrWhiteSpace(slug))
        {
            throw new ArgumentException("Slug is required.", nameof(slug));
        }

        if (string.IsNullOrWhiteSpace(body))
        {
            throw new ArgumentException("Body is required.", nameof(body));
        }

        if (string.IsNullOrWhiteSpace(contentType))
        {
            throw new ArgumentException("Content type is required.", nameof(contentType));
        }

        if (!Enum.TryParse<ContentType>(contentType, ignoreCase: true, out _))
        {
            throw new ArgumentException("Content type is invalid.", nameof(contentType));
        }

        ArgumentNullException.ThrowIfNull(seoMetadata);

        return new ContentRevisionSnapshot(
            title.Trim(),
            slug.Trim(),
            body.Trim(),
            excerpt?.Trim() ?? string.Empty,
            string.IsNullOrWhiteSpace(coverImage) ? null : coverImage.Trim(),
            contentType.Trim(),
            seoMetadata);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Title;
        yield return Slug;
        yield return Body;
        yield return Excerpt;
        yield return CoverImage;
        yield return ContentType;
        yield return SeoMetadata;
    }
}

public sealed class ContentRevisionSeoSnapshot : ValueObject
{
    private ContentRevisionSeoSnapshot(
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

    public string? SeoTitle { get; }

    public string? SeoDescription { get; }

    public string? CanonicalUrl { get; }

    public string? OgImage { get; }

    public string? FocusKeyword { get; }

    public static ContentRevisionSeoSnapshot From(SeoMetadata metadata)
    {
        ArgumentNullException.ThrowIfNull(metadata);

        return new ContentRevisionSeoSnapshot(
            metadata.SeoTitle,
            metadata.SeoDescription,
            metadata.CanonicalUrl,
            metadata.OgImage,
            metadata.FocusKeyword);
    }

    public static ContentRevisionSeoSnapshot Create(
        string? seoTitle,
        string? seoDescription,
        string? canonicalUrl,
        string? ogImage,
        string? focusKeyword) =>
        new(
            Normalize(seoTitle),
            Normalize(seoDescription),
            Normalize(canonicalUrl),
            Normalize(ogImage),
            Normalize(focusKeyword));

    private static string? Normalize(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return SeoTitle;
        yield return SeoDescription;
        yield return CanonicalUrl;
        yield return OgImage;
        yield return FocusKeyword;
    }
}

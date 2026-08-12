namespace HelpDev.Modules.Content.Application.Contents.Dtos;

/// <summary>
/// SEO metadata read projection. Admin/editor-only; never surfaced through public content APIs.
/// </summary>
public sealed record SeoMetadataDto(
    string? SeoTitle,
    string? SeoDescription,
    string? CanonicalUrl,
    string? OgImage,
    string? FocusKeyword);

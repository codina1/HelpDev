namespace HelpDev.Modules.Content.Application.SeoAnalysis;

/// <summary>
/// Immutable analysis input mapped from the Admin read model. Never a Domain/EF entity.
/// Blank strings are treated as missing by the analyzer after normalization.
/// </summary>
public sealed record SeoAnalysisInput(
    string Title,
    string Slug,
    string Body,
    string Excerpt,
    string? CoverImage,
    string ContentType,
    string? SeoTitle,
    string? SeoDescription,
    string? CanonicalUrl,
    string? OgImage,
    string? FocusKeyword);

namespace HelpDev.Modules.Content.Application.Contents.Dtos;

/// <summary>
/// Admin/editor request to update SEO metadata for a content item. All fields optional.
/// </summary>
public sealed class UpdateSeoMetadataRequest
{
    public string? SeoTitle { get; set; }

    public string? SeoDescription { get; set; }

    public string? CanonicalUrl { get; set; }

    public string? OgImage { get; set; }

    public string? FocusKeyword { get; set; }
}

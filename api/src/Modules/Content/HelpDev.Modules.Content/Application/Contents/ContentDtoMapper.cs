using HelpDev.Modules.Content.Application.Contents.Dtos;
using ContentEntity = HelpDev.Modules.Content.Domain.Entities.Content;

namespace HelpDev.Modules.Content.Application.Contents;

internal static class ContentDtoMapper
{
    public static AdminContentDetailDto ToAdminDetail(ContentEntity content) =>
        new(
            content.Id,
            content.Title,
            content.Slug.Value,
            content.Body,
            content.Excerpt,
            content.CoverImage,
            content.Type.ToString(),
            content.Status.ToString(),
            content.AuthorId,
            content.Views,
            content.Saves,
            content.CreatedAt,
            content.UpdatedAt,
            content.PublishedAtUtc,
            new SeoMetadataDto(
                content.SeoMetadata.SeoTitle,
                content.SeoMetadata.SeoDescription,
                content.SeoMetadata.CanonicalUrl,
                content.SeoMetadata.OgImage,
                content.SeoMetadata.FocusKeyword),
            content.ContentJson,
            content.ContentHtml,
            content.ContentFormat,
            content.EditorVersion,
            content.WordCount,
            content.ReadingTimeMinutes,
            content.LastAutosavedAtUtc);

    public static ContentDetailDto ToPublicDetail(ContentEntity content) =>
        new(
            content.Id,
            content.Title,
            content.Slug.Value,
            content.Body,
            content.Type.ToString(),
            content.AuthorId,
            content.Status.ToString(),
            content.Views,
            content.Saves,
            content.CreatedAt,
            content.ContentHtml,
            content.ContentFormat,
            content.WordCount,
            content.ReadingTimeMinutes);
}

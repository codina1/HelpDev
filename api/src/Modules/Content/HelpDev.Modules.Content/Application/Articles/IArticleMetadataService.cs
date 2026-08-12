using HelpDev.Modules.Content.Application.Articles.Dtos;
using HelpDev.Modules.Content.Application.Contents;

namespace HelpDev.Modules.Content.Application.Articles;

public interface IArticleMetadataService
{
    Task<ArticleMetadataDto?> GetByContentIdAsync(
        ContentManagementActor actor,
        Guid contentId,
        CancellationToken cancellationToken = default);

    Task<ArticleMetadataDto> CreateAsync(
        ContentManagementActor actor,
        Guid contentId,
        UpdateArticleMetadataRequest request,
        CancellationToken cancellationToken = default);

    Task<ArticleMetadataDto> UpdateAsync(
        ContentManagementActor actor,
        Guid contentId,
        UpdateArticleMetadataRequest request,
        CancellationToken cancellationToken = default);
}

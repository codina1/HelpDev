using HelpDev.Modules.Content.Domain.Articles;

namespace HelpDev.Modules.Content.Application.Articles;

public interface IArticleMetadataRepository
{
    Task<ArticleMetadata?> GetByContentIdAsync(Guid contentId, CancellationToken cancellationToken = default);

    Task AddAsync(ArticleMetadata metadata, CancellationToken cancellationToken = default);
}

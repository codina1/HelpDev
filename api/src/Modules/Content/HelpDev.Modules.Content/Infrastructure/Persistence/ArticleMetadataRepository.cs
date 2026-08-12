using HelpDev.Modules.Content.Application.Articles;
using HelpDev.Modules.Content.Application.Persistence;
using HelpDev.Modules.Content.Domain.Articles;
using Microsoft.EntityFrameworkCore;

namespace HelpDev.Modules.Content.Infrastructure.Persistence;

public sealed class ArticleMetadataRepository : IArticleMetadataRepository
{
    private readonly IContentDbContext _dbContext;

    public ArticleMetadataRepository(IContentDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<ArticleMetadata?> GetByContentIdAsync(
        Guid contentId,
        CancellationToken cancellationToken = default) =>
        _dbContext.ArticleMetadata
            .FirstOrDefaultAsync(metadata => metadata.ContentId == contentId, cancellationToken);

    public Task AddAsync(ArticleMetadata metadata, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(metadata);
        _dbContext.ArticleMetadata.Add(metadata);
        return Task.CompletedTask;
    }
}

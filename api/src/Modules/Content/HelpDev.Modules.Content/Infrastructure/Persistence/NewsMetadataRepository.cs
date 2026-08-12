using HelpDev.Modules.Content.Application.News;
using HelpDev.Modules.Content.Application.Persistence;
using HelpDev.Modules.Content.Domain.News;
using Microsoft.EntityFrameworkCore;

namespace HelpDev.Modules.Content.Infrastructure.Persistence;

public sealed class NewsMetadataRepository : INewsMetadataRepository
{
    private readonly IContentDbContext _dbContext;

    public NewsMetadataRepository(IContentDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<NewsMetadata?> GetByContentIdAsync(
        Guid contentId,
        CancellationToken cancellationToken = default) =>
        _dbContext.NewsMetadata
            .FirstOrDefaultAsync(metadata => metadata.ContentId == contentId, cancellationToken);

    public Task AddAsync(NewsMetadata metadata, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(metadata);
        _dbContext.NewsMetadata.Add(metadata);
        return Task.CompletedTask;
    }
}

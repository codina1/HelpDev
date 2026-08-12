using HelpDev.Modules.Content.Domain.News;

namespace HelpDev.Modules.Content.Application.News;

public interface INewsMetadataRepository
{
    Task<NewsMetadata?> GetByContentIdAsync(Guid contentId, CancellationToken cancellationToken = default);

    Task AddAsync(NewsMetadata metadata, CancellationToken cancellationToken = default);
}

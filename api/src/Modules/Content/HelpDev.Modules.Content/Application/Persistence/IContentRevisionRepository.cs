using HelpDev.Modules.Content.Domain.Entities;

namespace HelpDev.Modules.Content.Application.Persistence;

public interface IContentRevisionRepository
{
    Task AddAsync(ContentRevision revision, CancellationToken cancellationToken = default);

    Task<ContentRevision?> GetByContentIdAndVersionAsync(
        Guid contentId,
        int versionNumber,
        CancellationToken cancellationToken = default);

    Task<int> GetMaxVersionNumberAsync(Guid contentId, CancellationToken cancellationToken = default);
}

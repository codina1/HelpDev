using HelpDev.Modules.Content.Application.Persistence;
using HelpDev.Modules.Content.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace HelpDev.Modules.Content.Infrastructure.Persistence;

public sealed class ContentRevisionRepository : IContentRevisionRepository
{
    private readonly IContentDbContext _dbContext;

    public ContentRevisionRepository(IContentDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task AddAsync(ContentRevision revision, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(revision);
        _dbContext.ContentRevisions.Add(revision);
        return Task.CompletedTask;
    }

    public Task<ContentRevision?> GetByContentIdAndVersionAsync(
        Guid contentId,
        int versionNumber,
        CancellationToken cancellationToken = default) =>
        _dbContext.ContentRevisions
            .FirstOrDefaultAsync(
                revision => revision.ContentId == contentId && revision.VersionNumber == versionNumber,
                cancellationToken);

    public async Task<int> GetMaxVersionNumberAsync(Guid contentId, CancellationToken cancellationToken = default)
    {
        var max = await _dbContext.ContentRevisions
            .Where(revision => revision.ContentId == contentId)
            .MaxAsync(revision => (int?)revision.VersionNumber, cancellationToken)
            .ConfigureAwait(false);

        return max ?? 0;
    }
}

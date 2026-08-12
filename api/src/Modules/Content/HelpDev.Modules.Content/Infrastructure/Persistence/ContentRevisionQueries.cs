using HelpDev.Modules.Content.Application.Common;
using HelpDev.Modules.Content.Application.Contents;
using HelpDev.Modules.Content.Application.Contents.Dtos;
using HelpDev.Modules.Content.Application.Contents.Revisions;
using HelpDev.Modules.Content.Application.Persistence;
using HelpDev.Modules.Content.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace HelpDev.Modules.Content.Infrastructure.Persistence;

public sealed class ContentRevisionQueries : IContentRevisionQueries
{
    private const int DefaultPageSize = 20;

    private readonly IContentDbContext _dbContext;

    public ContentRevisionQueries(IContentDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PagedResult<ContentRevisionListItemDto>> GetPagedAsync(
        ContentManagementActor actor,
        Guid contentId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(actor);

        await EnsureCanAccessContentAsync(actor, contentId, cancellationToken).ConfigureAwait(false);

        var normalizedPage = page < 1 ? 1 : page;
        var normalizedPageSize = pageSize switch
        {
            < 1 => DefaultPageSize,
            > ContentSearchFilter.MaxPageSize => ContentSearchFilter.MaxPageSize,
            _ => pageSize,
        };

        var query = _dbContext.ContentRevisions.AsNoTracking()
            .Where(revision => revision.ContentId == contentId);

        var totalCount = await query.CountAsync(cancellationToken).ConfigureAwait(false);

        var items = await query
            .OrderByDescending(revision => revision.VersionNumber)
            .Skip((normalizedPage - 1) * normalizedPageSize)
            .Take(normalizedPageSize)
            .Select(revision => new ContentRevisionListItemDto(
                revision.VersionNumber,
                revision.CreatedByUserId,
                revision.CreatedAtUtc,
                revision.ChangeReason))
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return new PagedResult<ContentRevisionListItemDto>(
            items,
            normalizedPage,
            normalizedPageSize,
            totalCount);
    }

    public async Task<ContentRevisionDetailDto?> GetByVersionAsync(
        ContentManagementActor actor,
        Guid contentId,
        int versionNumber,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(actor);

        if (versionNumber <= 0)
        {
            return null;
        }

        await EnsureCanAccessContentAsync(actor, contentId, cancellationToken).ConfigureAwait(false);

        var revision = await _dbContext.ContentRevisions.AsNoTracking()
            .Where(row => row.ContentId == contentId && row.VersionNumber == versionNumber)
            .Select(row => new
            {
                row.ContentId,
                row.VersionNumber,
                row.Snapshot,
                row.ChangeReason,
                row.CreatedByUserId,
                row.CreatedAtUtc,
            })
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);

        if (revision is null)
        {
            return null;
        }

        return new ContentRevisionDetailDto(
            revision.ContentId,
            revision.VersionNumber,
            MapSnapshot(revision.Snapshot),
            revision.ChangeReason,
            revision.CreatedByUserId,
            revision.CreatedAtUtc);
    }

    private async Task EnsureCanAccessContentAsync(
        ContentManagementActor actor,
        Guid contentId,
        CancellationToken cancellationToken)
    {
        var authorId = await _dbContext.Contents.AsNoTracking()
            .Where(content => content.Id == contentId)
            .Select(content => (Guid?)content.AuthorId)
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);

        if (authorId is null)
        {
            throw new ContentException("محتوا یافت نشد.", ContentErrorCodes.NotFound);
        }

        ContentService.EnsureCanManage(authorId.Value, actor);
    }

    private static ContentRevisionSnapshotDto MapSnapshot(ContentRevisionSnapshot snapshot) =>
        new(
            snapshot.Title,
            snapshot.Slug,
            snapshot.Body,
            snapshot.Excerpt,
            snapshot.CoverImage,
            snapshot.ContentType,
            new SeoMetadataDto(
                snapshot.SeoMetadata.SeoTitle,
                snapshot.SeoMetadata.SeoDescription,
                snapshot.SeoMetadata.CanonicalUrl,
                snapshot.SeoMetadata.OgImage,
                snapshot.SeoMetadata.FocusKeyword));
}

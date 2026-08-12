using HelpDev.Modules.Media.Application.Assets;
using HelpDev.Modules.Media.Application.Common;
using HelpDev.Modules.Media.Application.Persistence;
using HelpDev.Modules.Media.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace HelpDev.Modules.Media.Infrastructure.Queries;

public sealed class MediaAssetQueries : IMediaAssetQueries
{
    private readonly IMediaDbContext _dbContext;

    public MediaAssetQueries(IMediaDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PagedResult<MediaAssetListItemDto>> GetPagedAsync(
        MediaManagementActor actor,
        MediaAssetListQuery query,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(actor);
        ArgumentNullException.ThrowIfNull(query);

        var source = _dbContext.MediaAssets.AsNoTracking()
            .Where(asset => asset.Status == MediaAssetStatus.Active);

        if (!actor.CanManageAllAssets)
        {
            source = source.Where(asset => asset.UploadedByUserId == actor.UserId);
        }
        else if (query.UploadedByUserId is Guid filterUserId)
        {
            source = source.Where(asset => asset.UploadedByUserId == filterUserId);
        }

        if (!string.IsNullOrWhiteSpace(query.ContentType))
        {
            var contentType = query.ContentType;
            source = source.Where(asset => asset.ContentType == contentType);
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var term = query.Search.ToLowerInvariant();
            source = source.Where(asset =>
                asset.OriginalFileName.ToLower().Contains(term)
                || (asset.AltText != null && asset.AltText.ToLower().Contains(term))
                || (asset.Caption != null && asset.Caption.ToLower().Contains(term)));
        }

        var total = await source.CountAsync(cancellationToken).ConfigureAwait(false);

        var items = await source
            .OrderByDescending(asset => asset.CreatedAtUtc)
            .ThenByDescending(asset => asset.Id)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(asset => new MediaAssetListItemDto(
                asset.Id,
                asset.OriginalFileName,
                asset.ContentType,
                asset.SizeBytes,
                asset.Width,
                asset.Height,
                asset.PublicUrl,
                asset.AltText,
                asset.UploadedByUserId,
                asset.CreatedAtUtc,
                asset.Status.ToString()))
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return new PagedResult<MediaAssetListItemDto>(items, query.Page, query.PageSize, total);
    }

    public async Task<MediaAssetDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.MediaAssets.AsNoTracking()
            .Where(asset => asset.Id == id)
            .Select(asset => new MediaAssetDto(
                asset.Id,
                asset.OriginalFileName,
                asset.ContentType,
                asset.SizeBytes,
                asset.Width,
                asset.Height,
                asset.PublicUrl,
                asset.AltText,
                asset.Caption,
                asset.UploadedByUserId,
                asset.CreatedAtUtc,
                asset.UpdatedAtUtc,
                asset.Status.ToString()))
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);
    }
}

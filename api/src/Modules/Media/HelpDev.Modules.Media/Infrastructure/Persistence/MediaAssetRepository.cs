using HelpDev.Modules.Media.Application.Persistence;
using HelpDev.Modules.Media.Domain.Assets;
using Microsoft.EntityFrameworkCore;

namespace HelpDev.Modules.Media.Infrastructure.Persistence;

public sealed class MediaAssetRepository : IMediaAssetRepository
{
    private readonly IMediaDbContext _dbContext;

    public MediaAssetRepository(IMediaDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(MediaAsset asset, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(asset);
        await _dbContext.MediaAssets.AddAsync(asset, cancellationToken).ConfigureAwait(false);
    }

    public Task<MediaAsset?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        _dbContext.MediaAssets.FirstOrDefaultAsync(asset => asset.Id == id, cancellationToken);
}

using HelpDev.Modules.Media.Domain.Assets;
using Microsoft.EntityFrameworkCore;

namespace HelpDev.Modules.Media.Application.Persistence;

public interface IMediaDbContext
{
    DbSet<MediaAsset> MediaAssets { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

public interface IMediaAssetRepository
{
    Task AddAsync(MediaAsset asset, CancellationToken cancellationToken = default);

    Task<MediaAsset?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
}

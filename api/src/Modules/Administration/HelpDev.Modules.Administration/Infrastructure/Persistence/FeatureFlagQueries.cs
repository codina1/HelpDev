using HelpDev.Modules.Administration.Application.FeatureFlags;
using HelpDev.Modules.Administration.Application.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HelpDev.Modules.Administration.Infrastructure.Persistence;

public sealed class FeatureFlagQueries : IFeatureFlagQueries
{
    private readonly IAdministrationDbContext _dbContext;

    public FeatureFlagQueries(IAdministrationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<FeatureFlagDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.FeatureFlags
            .AsNoTracking()
            .OrderBy(flag => flag.Key)
            .Select(flag => new FeatureFlagDto(
                flag.Id,
                flag.Key,
                flag.IsEnabled,
                flag.Description,
                flag.CreatedAtUtc,
                flag.UpdatedAtUtc))
            .ToListAsync(cancellationToken);
    }

    public async Task<FeatureFlagDto?> GetByKeyAsync(string key, CancellationToken cancellationToken = default)
    {
        return await _dbContext.FeatureFlags
            .AsNoTracking()
            .Where(flag => flag.Key.ToLower() == key.ToLower())
            .Select(flag => new FeatureFlagDto(
                flag.Id,
                flag.Key,
                flag.IsEnabled,
                flag.Description,
                flag.CreatedAtUtc,
                flag.UpdatedAtUtc))
            .FirstOrDefaultAsync(cancellationToken);
    }
}

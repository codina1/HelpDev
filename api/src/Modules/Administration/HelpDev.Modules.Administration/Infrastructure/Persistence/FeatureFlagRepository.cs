using HelpDev.Modules.Administration.Application.Persistence;
using HelpDev.Modules.Administration.Domain.FeatureFlags;
using Microsoft.EntityFrameworkCore;

namespace HelpDev.Modules.Administration.Infrastructure.Persistence;

public sealed class FeatureFlagRepository : IFeatureFlagRepository
{
    private readonly IAdministrationDbContext _dbContext;

    public FeatureFlagRepository(IAdministrationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<FeatureFlag?> GetByKeyAsync(string key, CancellationToken cancellationToken = default) =>
        _dbContext.FeatureFlags.FirstOrDefaultAsync(
            flag => flag.Key.ToLower() == key.ToLower(),
            cancellationToken);

    public Task<bool> ExistsByKeyAsync(string key, CancellationToken cancellationToken = default) =>
        _dbContext.FeatureFlags.AnyAsync(
            flag => flag.Key.ToLower() == key.ToLower(),
            cancellationToken);

    public async Task AddAsync(FeatureFlag featureFlag, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(featureFlag);
        await _dbContext.FeatureFlags.AddAsync(featureFlag, cancellationToken);
    }
}

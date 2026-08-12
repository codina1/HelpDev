using HelpDev.Modules.Administration.Domain.FeatureFlags;

namespace HelpDev.Modules.Administration.Application.Persistence;

public interface IFeatureFlagRepository
{
    Task<FeatureFlag?> GetByKeyAsync(string key, CancellationToken cancellationToken = default);

    Task<bool> ExistsByKeyAsync(string key, CancellationToken cancellationToken = default);

    Task AddAsync(FeatureFlag featureFlag, CancellationToken cancellationToken = default);
}

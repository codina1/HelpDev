using HelpDev.Modules.Administration.Domain.Settings;

namespace HelpDev.Modules.Administration.Application.Persistence;

public interface ISystemSettingRepository
{
    Task<SystemSetting?> GetByKeyAsync(string key, CancellationToken cancellationToken = default);

    Task<bool> ExistsByKeyAsync(string key, CancellationToken cancellationToken = default);

    Task AddAsync(SystemSetting setting, CancellationToken cancellationToken = default);
}

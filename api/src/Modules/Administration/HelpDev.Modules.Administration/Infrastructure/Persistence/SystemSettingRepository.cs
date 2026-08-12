using HelpDev.Modules.Administration.Application.Persistence;
using HelpDev.Modules.Administration.Domain.Settings;
using Microsoft.EntityFrameworkCore;

namespace HelpDev.Modules.Administration.Infrastructure.Persistence;

public sealed class SystemSettingRepository : ISystemSettingRepository
{
    private readonly IAdministrationDbContext _dbContext;

    public SystemSettingRepository(IAdministrationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<SystemSetting?> GetByKeyAsync(string key, CancellationToken cancellationToken = default) =>
        _dbContext.SystemSettings.FirstOrDefaultAsync(
            setting => setting.Key.ToLower() == key.ToLower(),
            cancellationToken);

    public Task<bool> ExistsByKeyAsync(string key, CancellationToken cancellationToken = default) =>
        _dbContext.SystemSettings.AnyAsync(
            setting => setting.Key.ToLower() == key.ToLower(),
            cancellationToken);

    public async Task AddAsync(SystemSetting setting, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(setting);
        await _dbContext.SystemSettings.AddAsync(setting, cancellationToken);
    }
}

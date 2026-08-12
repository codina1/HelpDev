using HelpDev.Modules.Administration.Application.Persistence;
using HelpDev.Modules.Administration.Application.Settings;
using Microsoft.EntityFrameworkCore;

namespace HelpDev.Modules.Administration.Infrastructure.Persistence;

public sealed class SystemSettingQueries : ISystemSettingQueries
{
    private readonly IAdministrationDbContext _dbContext;

    public SystemSettingQueries(IAdministrationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<SystemSettingDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.SystemSettings
            .AsNoTracking()
            .OrderBy(setting => setting.Key)
            .Select(setting => new SystemSettingDto(
                setting.Id,
                setting.Key,
                setting.Value,
                setting.ValueType.ToString(),
                setting.Description,
                setting.IsPublic,
                setting.CreatedAtUtc,
                setting.UpdatedAtUtc))
            .ToListAsync(cancellationToken);
    }

    public async Task<SystemSettingDto?> GetByKeyAsync(string key, CancellationToken cancellationToken = default)
    {
        return await _dbContext.SystemSettings
            .AsNoTracking()
            .Where(setting => setting.Key.ToLower() == key.ToLower())
            .Select(setting => new SystemSettingDto(
                setting.Id,
                setting.Key,
                setting.Value,
                setting.ValueType.ToString(),
                setting.Description,
                setting.IsPublic,
                setting.CreatedAtUtc,
                setting.UpdatedAtUtc))
            .FirstOrDefaultAsync(cancellationToken);
    }
}

using HelpDev.Modules.Administration.Application.Persistence;
using HelpDev.Modules.Administration.Application.Settings;
using Microsoft.EntityFrameworkCore;

namespace HelpDev.Modules.Administration.Infrastructure.Persistence;

public sealed class PublicSystemSettingQueries : IPublicSystemSettingQueries
{
    private readonly IAdministrationDbContext _dbContext;

    public PublicSystemSettingQueries(IAdministrationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<PublicSystemSettingDto>> GetPublicAsync(
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.SystemSettings
            .AsNoTracking()
            .Where(setting => setting.IsPublic)
            .OrderBy(setting => setting.Key)
            .Select(setting => new PublicSystemSettingDto(
                setting.Key,
                setting.Value,
                setting.ValueType.ToString()))
            .ToListAsync(cancellationToken);
    }
}

using HelpDev.Modules.Administration.Domain.FeatureFlags;
using Microsoft.EntityFrameworkCore;

namespace HelpDev.Modules.Administration.Application.Persistence;

public interface IAdministrationDbContext
{
    DbSet<FeatureFlag> FeatureFlags { get; }

    DbSet<Domain.Settings.SystemSetting> SystemSettings { get; }

    DbSet<Domain.Announcements.Announcement> Announcements { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

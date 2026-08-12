using HelpDev.Modules.Administration.Application.Persistence;
using HelpDev.Modules.Administration.Domain.Announcements;
using Microsoft.EntityFrameworkCore;

namespace HelpDev.Modules.Administration.Infrastructure.Persistence;

public sealed class AnnouncementRepository : IAnnouncementRepository
{
    private readonly IAdministrationDbContext _dbContext;

    public AnnouncementRepository(IAdministrationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<Announcement?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        _dbContext.Announcements.FirstOrDefaultAsync(item => item.Id == id, cancellationToken);

    public async Task AddAsync(Announcement announcement, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(announcement);
        await _dbContext.Announcements.AddAsync(announcement, cancellationToken);
    }

    public void Remove(Announcement announcement)
    {
        ArgumentNullException.ThrowIfNull(announcement);
        _dbContext.Announcements.Remove(announcement);
    }
}

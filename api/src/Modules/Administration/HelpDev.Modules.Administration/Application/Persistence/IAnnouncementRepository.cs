using HelpDev.Modules.Administration.Domain.Announcements;

namespace HelpDev.Modules.Administration.Application.Persistence;

public interface IAnnouncementRepository
{
    Task<Announcement?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task AddAsync(Announcement announcement, CancellationToken cancellationToken = default);

    void Remove(Announcement announcement);
}

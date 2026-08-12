using HelpDev.Modules.Identity.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace HelpDev.Modules.Identity.Application.Persistence;

/// <summary>
/// Persistence port for Identity. Implemented by the shared ApplicationDbContext
/// during incremental migration so the module does not reference legacy Infrastructure.
/// </summary>
public interface IIdentityDbContext
{
    DbSet<User> Users { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

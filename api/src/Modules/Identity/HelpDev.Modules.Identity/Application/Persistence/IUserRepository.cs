using HelpDev.Modules.Identity.Domain.Entities;

namespace HelpDev.Modules.Identity.Application.Persistence;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<User?> GetByMobileAsync(string mobile, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<User>> ListAllAsync(CancellationToken cancellationToken = default);

    Task<User> AddAsync(User user, CancellationToken cancellationToken = default);

    Task UpdateAsync(User user, CancellationToken cancellationToken = default);
}

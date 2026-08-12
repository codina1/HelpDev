using HelpDev.Modules.Identity.Application.Persistence;
using HelpDev.Modules.Identity.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace HelpDev.Modules.Identity.Infrastructure.Persistence;

public sealed class UserRepository : IUserRepository
{
    private readonly IIdentityDbContext _dbContext;

    public UserRepository(IIdentityDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        _dbContext.Users.FirstOrDefaultAsync(user => user.Id == id, cancellationToken);

    public Task<User?> GetByMobileAsync(string mobile, CancellationToken cancellationToken = default) =>
        _dbContext.Users.FirstOrDefaultAsync(user => user.Mobile == mobile, cancellationToken);

    public async Task<IReadOnlyList<User>> ListAllAsync(CancellationToken cancellationToken = default) =>
        await _dbContext.Users
            .AsNoTracking()
            .OrderByDescending(user => user.CreatedAt)
            .ToListAsync(cancellationToken);

    public async Task<User> AddAsync(User user, CancellationToken cancellationToken = default)
    {
        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return user;
    }

    public async Task UpdateAsync(User user, CancellationToken cancellationToken = default)
    {
        _dbContext.Users.Update(user);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}

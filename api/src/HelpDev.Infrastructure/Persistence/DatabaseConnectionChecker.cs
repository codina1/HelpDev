using Microsoft.EntityFrameworkCore;

namespace HelpDev.Infrastructure.Persistence;

public sealed class DatabaseConnectionChecker : IDatabaseConnectionChecker
{
    private readonly ApplicationDbContext _dbContext;

    public DatabaseConnectionChecker(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<bool> CanConnectAsync(CancellationToken cancellationToken = default) =>
        _dbContext.Database.CanConnectAsync(cancellationToken);
}

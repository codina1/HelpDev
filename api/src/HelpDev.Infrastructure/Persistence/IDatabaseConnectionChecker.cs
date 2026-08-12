namespace HelpDev.Infrastructure.Persistence;

public interface IDatabaseConnectionChecker
{
    Task<bool> CanConnectAsync(CancellationToken cancellationToken = default);
}

using HelpDev.Infrastructure.Persistence;
using HelpDev.Modules.Search.Application.Reindex;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;

namespace HelpDev.Infrastructure.Search;

/// <summary>
/// PostgreSQL session-level advisory lock for multi-instance reindex exclusion.
/// Lock is released via pg_advisory_unlock and when the connection drops (process crash safe).
/// </summary>
public sealed class PostgresSearchReindexLock : ISearchReindexLock
{
    /// <summary>Stable bigint key for HelpDev Search reindex (session advisory lock).</summary>
    public const long AdvisoryLockKey = 0x4844535243583031L; // "HDSRCX01"

    public const string TryAcquireSql = "SELECT pg_try_advisory_lock({0})";
    public const string ReleaseSql = "SELECT pg_advisory_unlock({0})";

    private readonly ApplicationDbContext _dbContext;

    public PostgresSearchReindexLock(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IAsyncDisposable?> TryAcquireAsync(CancellationToken cancellationToken = default)
    {
        var database = _dbContext.Database;
        await database.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);

        try
        {
            var acquired = await ExecuteBoolAsync(database, TryAcquireSql, AdvisoryLockKey, cancellationToken)
                .ConfigureAwait(false);
            if (!acquired)
            {
                await database.CloseConnectionAsync().ConfigureAwait(false);
                return null;
            }

            return new AdvisoryLockLease(database, AdvisoryLockKey);
        }
        catch
        {
            await database.CloseConnectionAsync().ConfigureAwait(false);
            throw;
        }
    }

    internal static async Task<bool> ExecuteBoolAsync(
        DatabaseFacade database,
        string sqlFormat,
        long key,
        CancellationToken cancellationToken)
    {
        var connection = database.GetDbConnection();
        await using var command = connection.CreateCommand();
        if (database.CurrentTransaction is not null)
        {
            command.Transaction = database.CurrentTransaction.GetDbTransaction();
        }

        command.CommandText = string.Format(
            System.Globalization.CultureInfo.InvariantCulture,
            sqlFormat,
            key);

        var result = await command.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);
        return result is bool boolean
            ? boolean
            : Convert.ToBoolean(result, System.Globalization.CultureInfo.InvariantCulture);
    }

    private sealed class AdvisoryLockLease : IAsyncDisposable
    {
        private readonly DatabaseFacade _database;
        private readonly long _key;
        private bool _disposed;

        public AdvisoryLockLease(DatabaseFacade database, long key)
        {
            _database = database;
            _key = key;
        }

        public async ValueTask DisposeAsync()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            try
            {
                await ExecuteBoolAsync(_database, ReleaseSql, _key, CancellationToken.None)
                    .ConfigureAwait(false);
            }
            finally
            {
                await _database.CloseConnectionAsync().ConfigureAwait(false);
            }
        }
    }
}

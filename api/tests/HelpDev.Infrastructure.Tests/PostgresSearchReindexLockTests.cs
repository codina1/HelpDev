using HelpDev.Infrastructure.Search;

namespace HelpDev.Infrastructure.Tests;

public sealed class PostgresSearchReindexLockTests
{
    [Fact]
    public void Lock_sql_targets_session_advisory_lock_with_stable_key()
    {
        Assert.Equal(0x4844535243583031L, PostgresSearchReindexLock.AdvisoryLockKey);
        Assert.Equal("SELECT pg_try_advisory_lock({0})", PostgresSearchReindexLock.TryAcquireSql);
        Assert.Equal("SELECT pg_advisory_unlock({0})", PostgresSearchReindexLock.ReleaseSql);
        Assert.Contains("pg_try_advisory_lock", PostgresSearchReindexLock.TryAcquireSql, StringComparison.Ordinal);
        Assert.DoesNotContain("pg_try_advisory_xact_lock", PostgresSearchReindexLock.TryAcquireSql, StringComparison.Ordinal);
    }
}

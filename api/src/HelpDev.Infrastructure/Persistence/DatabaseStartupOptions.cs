using Microsoft.Extensions.Options;

namespace HelpDev.Infrastructure.Persistence;

/// <summary>
/// Controls how database schema migrations are handled at application startup.
/// </summary>
public enum DatabaseMigrationMode
{
    /// <summary>Do not inspect or apply migrations.</summary>
    None = 0,

    /// <summary>Connect, verify reachability, and fail startup if pending migrations exist.</summary>
    Validate = 1,

    /// <summary>Apply pending migrations under an advisory lock. Controlled environments only.</summary>
    Apply = 2,
}

/// <summary>
/// Controls which data is seeded at application startup.
/// </summary>
public enum DatabaseSeedMode
{
    /// <summary>No seed is executed.</summary>
    None = 0,

    /// <summary>Only idempotent, non-secret required system data is applied.</summary>
    RequiredSystemData = 1,

    /// <summary>Full development/demo data. Forbidden in Production.</summary>
    DevelopmentDemo = 2,
}

/// <summary>
/// Startup database policy. Bound from the "Database" configuration section.
/// </summary>
public sealed class DatabaseStartupOptions
{
    public const string SectionName = "Database";

    /// <summary>
    /// Migration mode. When null, the environment-based default is used
    /// (Production =&gt; Validate, other environments =&gt; Apply).
    /// </summary>
    public DatabaseMigrationMode? MigrationMode { get; set; }

    /// <summary>
    /// Seed mode. When null, the environment-based default is used
    /// (Development =&gt; DevelopmentDemo, other environments =&gt; None).
    /// </summary>
    public DatabaseSeedMode? SeedMode { get; set; }

    /// <summary>
    /// Bounded time, in seconds, to wait for the migration advisory lock in Apply mode.
    /// </summary>
    public int MigrationLockTimeoutSeconds { get; set; } = 60;

    public PostgreSqlRuntimeOptions Postgres { get; set; } = new();

    public DatabaseMigrationMode ResolveMigrationMode(bool isProduction) =>
        MigrationMode ?? (isProduction ? DatabaseMigrationMode.Validate : DatabaseMigrationMode.Apply);

    public DatabaseSeedMode ResolveSeedMode(bool isDevelopment) =>
        SeedMode ?? (isDevelopment ? DatabaseSeedMode.DevelopmentDemo : DatabaseSeedMode.None);
}

/// <summary>
/// PostgreSQL runtime and connection-pool tuning. Bound from "Database:Postgres".
/// </summary>
public sealed class PostgreSqlRuntimeOptions
{
    public int CommandTimeoutSeconds { get; set; } = 30;

    public int ConnectionTimeoutSeconds { get; set; } = 15;

    public int MinPoolSize { get; set; }

    public int MaxPoolSize { get; set; } = 50;

    public int KeepAliveSeconds { get; set; } = 30;

    // Off by default: the application uses explicit, service-owned transactions. Enabling the
    // Npgsql retrying execution strategy requires execution-strategy-aware transaction handling.
    public bool EnableRetryOnFailure { get; set; }

    public int MaxRetryCount { get; set; } = 5;

    public int MaxRetryDelaySeconds { get; set; } = 10;
}

public sealed class DatabaseStartupOptionsValidator : IValidateOptions<DatabaseStartupOptions>
{
    public ValidateOptionsResult Validate(string? name, DatabaseStartupOptions options)
    {
        if (options.MigrationLockTimeoutSeconds is < 1 or > 600)
        {
            return ValidateOptionsResult.Fail("Database migration lock timeout must be between 1 and 600 seconds.");
        }

        var postgres = options.Postgres;

        if (postgres.CommandTimeoutSeconds is < 1 or > 600)
        {
            return ValidateOptionsResult.Fail("PostgreSQL command timeout must be between 1 and 600 seconds.");
        }

        if (postgres.ConnectionTimeoutSeconds is < 1 or > 300)
        {
            return ValidateOptionsResult.Fail("PostgreSQL connection timeout must be between 1 and 300 seconds.");
        }

        if (postgres.MinPoolSize < 0)
        {
            return ValidateOptionsResult.Fail("PostgreSQL MinPoolSize must be zero or greater.");
        }

        if (postgres.MaxPoolSize < 1)
        {
            return ValidateOptionsResult.Fail("PostgreSQL MaxPoolSize must be greater than zero.");
        }

        if (postgres.MaxPoolSize > 500)
        {
            return ValidateOptionsResult.Fail("PostgreSQL MaxPoolSize is unreasonably high (max 500).");
        }

        if (postgres.MinPoolSize > postgres.MaxPoolSize)
        {
            return ValidateOptionsResult.Fail("PostgreSQL MinPoolSize must not exceed MaxPoolSize.");
        }

        if (postgres.KeepAliveSeconds is < 0 or > 600)
        {
            return ValidateOptionsResult.Fail("PostgreSQL KeepAlive must be between 0 and 600 seconds.");
        }

        if (postgres.MaxRetryCount is < 0 or > 20)
        {
            return ValidateOptionsResult.Fail("PostgreSQL MaxRetryCount must be between 0 and 20.");
        }

        if (postgres.MaxRetryDelaySeconds is < 1 or > 120)
        {
            return ValidateOptionsResult.Fail("PostgreSQL MaxRetryDelay must be between 1 and 120 seconds.");
        }

        return ValidateOptionsResult.Success;
    }
}

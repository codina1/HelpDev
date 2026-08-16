using System.Diagnostics;
using HelpDev.Infrastructure.Persistence.Seed;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Npgsql;

namespace HelpDev.Infrastructure.Persistence;

public static class DatabaseStartupEventNames
{
    public const string MigrationValidationStarted = "DatabaseMigrationValidationStarted";
    public const string MigrationPending = "DatabaseMigrationPending";
    public const string MigrationApplyStarted = "DatabaseMigrationApplyStarted";
    public const string MigrationCompleted = "DatabaseMigrationCompleted";
    public const string MigrationFailed = "DatabaseMigrationFailed";
    public const string MigrationSkipped = "DatabaseMigrationSkipped";
    public const string MigrationLockAcquired = "DatabaseMigrationLockAcquired";
    public const string MigrationLockTimedOut = "DatabaseMigrationLockTimedOut";
    public const string SeedStarted = "DatabaseSeedStarted";
    public const string SeedCompleted = "DatabaseSeedCompleted";
}

/// <summary>
/// Coordinates safe database migration and seeding at application startup according to the
/// configured <see cref="DatabaseMigrationMode"/> and <see cref="DatabaseSeedMode"/>.
/// Never logs SQL, connection strings, or secrets.
/// </summary>
public static class DatabaseStartupManager
{
    /// <summary>
    /// Stable, application-specific advisory lock key used to serialize migration across instances.
    /// Documented internally; not derived from a random process value.
    /// </summary>
    public const long MigrationAdvisoryLockKey = 4207770001L;

    public static async Task RunAsync(
        IServiceProvider serviceProvider,
        IHostEnvironment environment,
        CancellationToken cancellationToken = default)
    {
        using var scope = serviceProvider.CreateScope();
        var services = scope.ServiceProvider;
        var logger = services.GetRequiredService<ILoggerFactory>().CreateLogger("DatabaseStartup");
        var options = services.GetRequiredService<IOptions<DatabaseStartupOptions>>().Value;
        var context = services.GetRequiredService<ApplicationDbContext>();

        var migrationMode = options.ResolveMigrationMode(environment.IsProduction());
        var seedMode = options.ResolveSeedMode(environment.IsDevelopment());

        GuardSeedMode(environment, seedMode);

        if (!context.Database.IsNpgsql())
        {
            throw new InvalidOperationException("The configured database provider is not PostgreSQL.");
        }

        switch (migrationMode)
        {
            case DatabaseMigrationMode.None:
                logger.LogInformation("Event={Event}", DatabaseStartupEventNames.MigrationSkipped);
                break;

            case DatabaseMigrationMode.Validate:
                await ValidateAsync(context, logger, cancellationToken).ConfigureAwait(false);
                break;

            case DatabaseMigrationMode.Apply:
                await ApplyWithAdvisoryLockAsync(context, options, logger, cancellationToken).ConfigureAwait(false);
                break;

            default:
                throw new InvalidOperationException($"Unsupported migration mode '{migrationMode}'.");
        }

        await SeedAsync(context, seedMode, logger, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Controlled migration entry point for the out-of-band <c>--apply-migrations</c> command.
    /// Forces <see cref="DatabaseMigrationMode.Apply"/> for this process only, acquires the advisory
    /// lock, applies pending migrations, and optionally seeds idempotent required system data.
    /// The HTTP server and hosted services are never started by this path.
    /// </summary>
    public static async Task ApplyMigrationsAsync(
        IServiceProvider serviceProvider,
        IHostEnvironment environment,
        bool seedRequiredSystemData,
        CancellationToken cancellationToken = default)
    {
        using var scope = serviceProvider.CreateScope();
        var services = scope.ServiceProvider;
        var logger = services.GetRequiredService<ILoggerFactory>().CreateLogger("DatabaseStartup");
        var options = services.GetRequiredService<IOptions<DatabaseStartupOptions>>().Value;
        var context = services.GetRequiredService<ApplicationDbContext>();

        if (!context.Database.IsNpgsql())
        {
            throw new InvalidOperationException("The configured database provider is not PostgreSQL.");
        }

        await ApplyWithAdvisoryLockAsync(context, options, logger, cancellationToken).ConfigureAwait(false);

        // DevelopmentDemo seeding is never permitted through the controlled migration command.
        var seedMode = seedRequiredSystemData ? DatabaseSeedMode.RequiredSystemData : DatabaseSeedMode.None;
        await SeedAsync(context, seedMode, logger, cancellationToken).ConfigureAwait(false);
    }

    private static void GuardSeedMode(IHostEnvironment environment, DatabaseSeedMode seedMode)
    {
        if (environment.IsProduction() && seedMode == DatabaseSeedMode.DevelopmentDemo)
        {
            throw new InvalidOperationException(
                "DevelopmentDemo seed mode is forbidden in Production. Use None or RequiredSystemData.");
        }
    }

    private static async Task ValidateAsync(
        ApplicationDbContext context,
        ILogger logger,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Event={Event}", DatabaseStartupEventNames.MigrationValidationStarted);

        var canConnect = await context.Database.CanConnectAsync(cancellationToken).ConfigureAwait(false);
        if (!canConnect)
        {
            logger.LogError("Event={Event}", DatabaseStartupEventNames.MigrationFailed);
            throw new InvalidOperationException("The database is not reachable.");
        }

        var pending = (await context.Database.GetPendingMigrationsAsync(cancellationToken).ConfigureAwait(false)).ToList();
        if (pending.Count > 0)
        {
            logger.LogError(
                "Event={Event} PendingMigrationCount={PendingMigrationCount}",
                DatabaseStartupEventNames.MigrationPending,
                pending.Count);
            throw new InvalidOperationException(
                $"There are {pending.Count} pending database migration(s). Startup is blocked in Validate mode.");
        }

        logger.LogInformation("Event={Event}", DatabaseStartupEventNames.MigrationCompleted);
    }

    private static async Task ApplyWithAdvisoryLockAsync(
        ApplicationDbContext context,
        DatabaseStartupOptions options,
        ILogger logger,
        CancellationToken cancellationToken)
    {
        var connection = (NpgsqlConnection)context.Database.GetDbConnection();
        var openedHere = false;

        if (connection.State != System.Data.ConnectionState.Open)
        {
            await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
            openedHere = true;
        }

        var lockAcquired = false;
        try
        {
            lockAcquired = await TryAcquireAdvisoryLockAsync(
                connection,
                options.MigrationLockTimeoutSeconds,
                cancellationToken).ConfigureAwait(false);

            if (!lockAcquired)
            {
                logger.LogError("Event={Event}", DatabaseStartupEventNames.MigrationLockTimedOut);
                throw new InvalidOperationException(
                    "Could not acquire the database migration advisory lock within the configured timeout.");
            }

            logger.LogInformation("Event={Event}", DatabaseStartupEventNames.MigrationLockAcquired);

            var pending = (await context.Database.GetPendingMigrationsAsync(cancellationToken).ConfigureAwait(false)).ToList();
            if (pending.Count == 0)
            {
                logger.LogInformation("Event={Event}", DatabaseStartupEventNames.MigrationCompleted);
                return;
            }

            logger.LogInformation(
                "Event={Event} PendingMigrationCount={PendingMigrationCount}",
                DatabaseStartupEventNames.MigrationApplyStarted,
                pending.Count);

            await context.Database.MigrateAsync(cancellationToken).ConfigureAwait(false);

            logger.LogInformation("Event={Event}", DatabaseStartupEventNames.MigrationCompleted);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogError(ex, "Event={Event}", DatabaseStartupEventNames.MigrationFailed);
            throw;
        }
        finally
        {
            if (lockAcquired)
            {
                await ReleaseAdvisoryLockAsync(connection, cancellationToken).ConfigureAwait(false);
            }

            if (openedHere && connection.State == System.Data.ConnectionState.Open)
            {
                await connection.CloseAsync().ConfigureAwait(false);
            }
        }
    }

    private static async Task<bool> TryAcquireAdvisoryLockAsync(
        NpgsqlConnection connection,
        int timeoutSeconds,
        CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();
        var timeout = TimeSpan.FromSeconds(timeoutSeconds);

        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();

            await using (var command = connection.CreateCommand())
            {
                command.CommandText = "SELECT pg_try_advisory_lock(@key);";
                command.Parameters.AddWithValue("key", MigrationAdvisoryLockKey);
                var result = await command.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);
                if (result is true)
                {
                    return true;
                }
            }

            if (stopwatch.Elapsed >= timeout)
            {
                return false;
            }

            await Task.Delay(TimeSpan.FromMilliseconds(250), cancellationToken).ConfigureAwait(false);
        }
    }

    private static async Task ReleaseAdvisoryLockAsync(
        NpgsqlConnection connection,
        CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT pg_advisory_unlock(@key);";
        command.Parameters.AddWithValue("key", MigrationAdvisoryLockKey);
        await command.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);
    }

    private static async Task SeedAsync(
        ApplicationDbContext context,
        DatabaseSeedMode seedMode,
        ILogger logger,
        CancellationToken cancellationToken)
    {
        if (seedMode == DatabaseSeedMode.None)
        {
            return;
        }

        logger.LogInformation("Event={Event} SeedMode={SeedMode}", DatabaseStartupEventNames.SeedStarted, seedMode);

        switch (seedMode)
        {
            case DatabaseSeedMode.DevelopmentDemo:
                await ApplicationDbContextSeed.SeedAsync(context, logger, cancellationToken).ConfigureAwait(false);
                await ApplicationDbContextSeed.EnsureBootstrapAdminsAsync(context, logger, cancellationToken).ConfigureAwait(false);
                break;

            case DatabaseSeedMode.RequiredSystemData:
                await ApplicationDbContextSeed.EnsureBootstrapAdminsAsync(context, logger, cancellationToken).ConfigureAwait(false);
                break;
        }

        logger.LogInformation("Event={Event} SeedMode={SeedMode}", DatabaseStartupEventNames.SeedCompleted, seedMode);
    }
}

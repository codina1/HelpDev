using HelpDev.Infrastructure.Persistence;
using HelpDev.Testing.PostgreSQL;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Npgsql;

namespace HelpDev.Integration.Tests.Deployment;

internal sealed class TestHostEnvironment : IHostEnvironment
{
    public string EnvironmentName { get; set; } = "Testing";
    public string ApplicationName { get; set; } = "HelpDev.API";
    public string ContentRootPath { get; set; } = AppContext.BaseDirectory;
    public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
}

[Collection(PostgreSqlCollection.Name)]
[Trait("Category", "Integration")]
[Trait("Category", "PostgreSQL")]
[Trait("Category", "Deployment")]
public sealed class MigrationModeAndAdvisoryLockTests : PostgreSqlIntegrationTestBase
{
    public MigrationModeAndAdvisoryLockTests(PostgreSqlFixture fixture)
        : base(fixture)
    {
    }

    private static ServiceProvider BuildProvider(
        string connectionString,
        DatabaseMigrationMode migrationMode,
        int lockTimeoutSeconds = 60)
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.Configure<DatabaseStartupOptions>(options =>
        {
            options.MigrationMode = migrationMode;
            options.SeedMode = DatabaseSeedMode.None;
            options.MigrationLockTimeoutSeconds = lockTimeoutSeconds;
        });
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(
                connectionString,
                npgsql => npgsql.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.GetName().Name)));

        return services.BuildServiceProvider();
    }

    [PostgreSqlFact]
    public async Task Validate_mode_with_pending_migrations_fails_startup()
    {
        var connectionString = await Fixture.CreateIsolatedDatabaseAsync();
        await using var provider = BuildProvider(connectionString, DatabaseMigrationMode.Validate);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => DatabaseStartupManager.RunAsync(provider, new TestHostEnvironment()));

        Assert.Contains("pending database migration", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [PostgreSqlFact]
    public async Task Validate_mode_without_pending_migrations_succeeds()
    {
        var connectionString = await CreateDatabaseAndMigrateAsync();
        await using var provider = BuildProvider(connectionString, DatabaseMigrationMode.Validate);

        await DatabaseStartupManager.RunAsync(provider, new TestHostEnvironment());
    }

    [PostgreSqlFact]
    public async Task None_mode_does_not_apply_migrations()
    {
        var connectionString = await Fixture.CreateIsolatedDatabaseAsync();
        await using var provider = BuildProvider(connectionString, DatabaseMigrationMode.None);

        await DatabaseStartupManager.RunAsync(provider, new TestHostEnvironment());

        await using var context = new ApplicationDbContext(
            new DbContextOptionsBuilder<ApplicationDbContext>().UseNpgsql(connectionString).Options);
        var pending = await context.Database.GetPendingMigrationsAsync();
        Assert.NotEmpty(pending);
    }

    [PostgreSqlFact]
    public async Task Apply_mode_applies_pending_migrations()
    {
        var connectionString = await Fixture.CreateIsolatedDatabaseAsync();
        await using var provider = BuildProvider(connectionString, DatabaseMigrationMode.Apply);

        await DatabaseStartupManager.RunAsync(provider, new TestHostEnvironment());

        await using var context = new ApplicationDbContext(
            new DbContextOptionsBuilder<ApplicationDbContext>().UseNpgsql(connectionString).Options);
        var pending = await context.Database.GetPendingMigrationsAsync();
        Assert.Empty(pending);
    }

    [PostgreSqlFact]
    public async Task Apply_mode_fails_when_advisory_lock_is_held_by_another_session()
    {
        var connectionString = await Fixture.CreateIsolatedDatabaseAsync();

        await using var holder = new NpgsqlConnection(connectionString);
        await holder.OpenAsync();
        await using (var lockCommand = holder.CreateCommand())
        {
            lockCommand.CommandText = "SELECT pg_advisory_lock(@key);";
            lockCommand.Parameters.AddWithValue("key", DatabaseStartupManager.MigrationAdvisoryLockKey);
            await lockCommand.ExecuteScalarAsync();
        }

        await using (var provider = BuildProvider(connectionString, DatabaseMigrationMode.Apply, lockTimeoutSeconds: 2))
        {
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => DatabaseStartupManager.RunAsync(provider, new TestHostEnvironment()));
            Assert.Contains("advisory lock", exception.Message, StringComparison.OrdinalIgnoreCase);
        }

        // Release the lock and confirm Apply can now proceed.
        await using (var unlock = holder.CreateCommand())
        {
            unlock.CommandText = "SELECT pg_advisory_unlock(@key);";
            unlock.Parameters.AddWithValue("key", DatabaseStartupManager.MigrationAdvisoryLockKey);
            await unlock.ExecuteScalarAsync();
        }

        await using var retryProvider = BuildProvider(connectionString, DatabaseMigrationMode.Apply);
        await DatabaseStartupManager.RunAsync(retryProvider, new TestHostEnvironment());

        await using var context = new ApplicationDbContext(
            new DbContextOptionsBuilder<ApplicationDbContext>().UseNpgsql(connectionString).Options);
        Assert.Empty(await context.Database.GetPendingMigrationsAsync());
    }

    [PostgreSqlFact]
    public async Task Development_demo_seed_is_forbidden_in_production()
    {
        var connectionString = await CreateDatabaseAndMigrateAsync();
        var services = new ServiceCollection();
        services.AddLogging();
        services.Configure<DatabaseStartupOptions>(options =>
        {
            options.MigrationMode = DatabaseMigrationMode.None;
            options.SeedMode = DatabaseSeedMode.DevelopmentDemo;
        });
        services.AddDbContext<ApplicationDbContext>(options => options.UseNpgsql(connectionString));
        await using var provider = services.BuildServiceProvider();

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => DatabaseStartupManager.RunAsync(provider, new TestHostEnvironment { EnvironmentName = Environments.Production }));
    }
}

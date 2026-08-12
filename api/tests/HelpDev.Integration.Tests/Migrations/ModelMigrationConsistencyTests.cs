using HelpDev.Infrastructure.Persistence;
using HelpDev.Testing.PostgreSQL;
using Microsoft.EntityFrameworkCore;

namespace HelpDev.Integration.Tests.Migrations;

[Collection(PostgreSqlCollection.Name)]
[Trait("Category", "Integration")]
[Trait("Category", "PostgreSQL")]
public sealed class ModelMigrationConsistencyTests : PostgreSqlIntegrationTestBase
{
    public ModelMigrationConsistencyTests(PostgreSqlFixture fixture)
        : base(fixture)
    {
    }

    [PostgreSqlFact]
    public async Task Model_matches_latest_migration_snapshot_with_no_pending_changes()
    {
        var connectionString = await CreateDatabaseAndMigrateAsync();
        await using var context = await CreateContextAsync(connectionString);

        var pendingMigrations = await context.Database.GetPendingMigrationsAsync();
        Assert.Empty(pendingMigrations);

        var hasPendingModelChanges = context.Database.HasPendingModelChanges();
        Assert.False(hasPendingModelChanges);
    }
}

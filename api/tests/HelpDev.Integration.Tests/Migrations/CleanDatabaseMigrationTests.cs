using HelpDev.Infrastructure.Persistence;
using HelpDev.Testing.PostgreSQL;
using HelpDev.Testing.PostgreSQL.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;

namespace HelpDev.Integration.Tests.Migrations;

[Collection(PostgreSqlCollection.Name)]
[Trait("Category", "Integration")]
[Trait("Category", "PostgreSQL")]
public sealed class CleanDatabaseMigrationTests : PostgreSqlIntegrationTestBase
{
    public CleanDatabaseMigrationTests(PostgreSqlFixture fixture)
        : base(fixture)
    {
    }

    [PostgreSqlFact]
    public async Task Empty_database_migrates_all_modules_with_expected_schema()
    {
        var connectionString = await Fixture.CreateIsolatedDatabaseAsync();
        await using var context = await CreateContextAsync(connectionString);

        var pendingMigrations = await context.Database.GetPendingMigrationsAsync();
        Assert.Empty(pendingMigrations);

        var existingTables = await PostgreSqlDatabaseHelper.GetExistingModuleTablesAsync(connectionString);
        Assert.Equal(PostgreSqlDatabaseHelper.ExpectedModuleTables.Count, existingTables.Count);
        Assert.All(
            PostgreSqlDatabaseHelper.ExpectedModuleTables,
            expected => Assert.Contains(expected, existingTables));

        var migrationCount = await PostgreSqlDatabaseHelper.GetAppliedMigrationCountAsync(connectionString);
        Assert.Equal(PostgreSqlDatabaseHelper.ExpectedMigrationCount, migrationCount);

        var crossModuleForeignKeys = await PostgreSqlDatabaseHelper.GetCrossModuleForeignKeysAsync(connectionString);
        Assert.Empty(crossModuleForeignKeys);

        await using var indexConnection = new NpgsqlConnection(connectionString);
        await indexConnection.OpenAsync();
        await using var indexCommand = new NpgsqlCommand(
            """
            SELECT COUNT(*)
            FROM pg_indexes
            WHERE schemaname = 'public'
              AND tablename = ANY(@tables)
            """,
            indexConnection);
        indexCommand.Parameters.AddWithValue("tables", PostgreSqlDatabaseHelper.ExpectedModuleTables.ToArray());
        var indexCount = Convert.ToInt32(await indexCommand.ExecuteScalarAsync());
        Assert.True(indexCount > PostgreSqlDatabaseHelper.ExpectedModuleTables.Count);
    }

    [PostgreSqlFact]
    public async Task Truncate_all_module_tables_leaves_migrations_intact()
    {
        var connectionString = await CreateDatabaseAndMigrateAsync();
        await PostgreSqlDatabaseHelper.TruncateAllModuleTablesAsync(connectionString);

        await using var context = await CreateContextAsync(connectionString);
        Assert.Equal(0, await context.Users.CountAsync());
        Assert.Equal(0, await context.Contents.CountAsync());

        var migrationCount = await PostgreSqlDatabaseHelper.GetAppliedMigrationCountAsync(connectionString);
        Assert.Equal(PostgreSqlDatabaseHelper.ExpectedMigrationCount, migrationCount);
    }
}

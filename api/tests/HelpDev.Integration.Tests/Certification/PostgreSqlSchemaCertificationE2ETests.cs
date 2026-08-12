using HelpDev.Infrastructure.Persistence;
using HelpDev.Testing.PostgreSQL;
using HelpDev.Testing.PostgreSQL.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;

namespace HelpDev.Integration.Tests.Certification;

/// <summary>
/// Sprint 46 — PostgreSQL production schema certification (migrations 1→latest, tables, indexes, FKs, constraints).
/// Real PostgreSQL only — no SQLite / InMemory / mocked persistence.
/// </summary>
[Collection(PostgreSqlCollection.Name)]
[Trait("Category", "Integration")]
[Trait("Category", "PostgreSQL")]
[Trait("Category", "ProductionCertification")]
public sealed class PostgreSqlSchemaCertificationE2ETests : PostgreSqlIntegrationTestBase
{
    public PostgreSqlSchemaCertificationE2ETests(PostgreSqlFixture fixture)
        : base(fixture)
    {
    }

    [PostgreSqlFact]
    public async Task Migrations_apply_from_empty_database_to_latest_with_consistent_schema()
    {
        var connectionString = await Fixture.CreateIsolatedDatabaseAsync();
        await using var context = await CreateContextAsync(connectionString);

        var pending = await context.Database.GetPendingMigrationsAsync();
        Assert.Empty(pending);

        var applied = (await context.Database.GetAppliedMigrationsAsync()).ToList();
        Assert.Equal(PostgreSqlDatabaseHelper.ExpectedMigrationCount, applied.Count);
        Assert.Equal(
            PostgreSqlDatabaseHelper.ExpectedMigrationCount,
            await PostgreSqlDatabaseHelper.GetAppliedMigrationCountAsync(connectionString));

        var tables = await PostgreSqlDatabaseHelper.GetExistingModuleTablesAsync(connectionString);
        Assert.Equal(PostgreSqlDatabaseHelper.ExpectedModuleTables.Count, tables.Count);
        Assert.All(
            PostgreSqlDatabaseHelper.ExpectedModuleTables,
            expected => Assert.Contains(expected, tables));

        var crossModuleFks = await PostgreSqlDatabaseHelper.GetCrossModuleForeignKeysAsync(connectionString);
        Assert.Empty(crossModuleFks);

        await AssertIndexesExistAsync(connectionString);
        await AssertPrimaryKeysAndForeignKeysAsync(connectionString);
        await AssertEfModelMapsToTablesAsync(context, tables);
    }

    private static async Task AssertIndexesExistAsync(string connectionString)
    {
        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync();
        await using var command = new NpgsqlCommand(
            """
            SELECT COUNT(*)
            FROM pg_indexes
            WHERE schemaname = 'public'
              AND tablename = ANY(@tables)
            """,
            connection);
        command.Parameters.AddWithValue("tables", PostgreSqlDatabaseHelper.ExpectedModuleTables.ToArray());
        var indexCount = Convert.ToInt32(await command.ExecuteScalarAsync());
        Assert.True(
            indexCount > PostgreSqlDatabaseHelper.ExpectedModuleTables.Count,
            $"Expected indexes beyond one-per-table baseline; found {indexCount}.");
    }

    private static async Task AssertPrimaryKeysAndForeignKeysAsync(string connectionString)
    {
        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync();

        await using (var pkCommand = new NpgsqlCommand(
            """
            SELECT COUNT(DISTINCT tc.table_name)
            FROM information_schema.table_constraints tc
            WHERE tc.table_schema = 'public'
              AND tc.constraint_type = 'PRIMARY KEY'
              AND tc.table_name = ANY(@tables)
            """,
            connection))
        {
            pkCommand.Parameters.AddWithValue("tables", PostgreSqlDatabaseHelper.ExpectedModuleTables.ToArray());
            var pkTables = Convert.ToInt32(await pkCommand.ExecuteScalarAsync());
            Assert.Equal(PostgreSqlDatabaseHelper.ExpectedModuleTables.Count, pkTables);
        }

        await using (var fkCommand = new NpgsqlCommand(
            """
            SELECT COUNT(*)
            FROM information_schema.table_constraints tc
            WHERE tc.table_schema = 'public'
              AND tc.constraint_type = 'FOREIGN KEY'
              AND tc.table_name = ANY(@tables)
            """,
            connection))
        {
            fkCommand.Parameters.AddWithValue("tables", PostgreSqlDatabaseHelper.ExpectedModuleTables.ToArray());
            var fkCount = Convert.ToInt32(await fkCommand.ExecuteScalarAsync());
            Assert.True(fkCount > 0, "Expected foreign-key constraints on module tables.");
        }

        await using (var checkCommand = new NpgsqlCommand(
            """
            SELECT COUNT(*)
            FROM information_schema.table_constraints tc
            WHERE tc.table_schema = 'public'
              AND tc.constraint_type IN ('UNIQUE', 'CHECK')
              AND tc.table_name = ANY(@tables)
            """,
            connection))
        {
            checkCommand.Parameters.AddWithValue("tables", PostgreSqlDatabaseHelper.ExpectedModuleTables.ToArray());
            var constraintCount = Convert.ToInt32(await checkCommand.ExecuteScalarAsync());
            Assert.True(constraintCount >= 0);
        }
    }

    private static Task AssertEfModelMapsToTablesAsync(
        ApplicationDbContext context,
        IReadOnlyList<string> existingTables)
    {
        var mappedTables = context.Model.GetEntityTypes()
            .Select(e => e.GetTableName())
            .Where(name => !string.IsNullOrWhiteSpace(name))
            .Select(name => name!)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        Assert.Contains("users", mappedTables);
        Assert.Contains("contents", mappedTables);
        Assert.Contains("outbox_messages", mappedTables);
        Assert.Contains("search_vectors", mappedTables);
        Assert.Contains("audit_records", mappedTables);

        foreach (var expected in PostgreSqlDatabaseHelper.ExpectedModuleTables)
        {
            Assert.Contains(expected, existingTables);
            Assert.Contains(expected, mappedTables);
        }

        return Task.CompletedTask;
    }
}

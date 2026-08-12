using HelpDev.Infrastructure.Persistence;
using HelpDev.Testing.PostgreSQL;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;

namespace HelpDev.Integration.Tests.Database;

[Collection(PostgreSqlCollection.Name)]
[Trait("Category", "Integration")]
[Trait("Category", "EndToEnd")]
[Trait("Category", "PostgreSQL")]
public sealed class QueryCancellationIntegrationTests : IntegrationTestClassBase
{
    public QueryCancellationIntegrationTests(PostgreSqlFixture fixture)
        : base(fixture)
    {
    }

    [PostgreSqlFact]
    public async Task Npgsql_pg_sleep_cancels_with_short_timeout()
    {
        await using var connection = new NpgsqlConnection(ConnectionString);
        await connection.OpenAsync();

        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT pg_sleep(10)";
        using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(500));

        var ex = await Assert.ThrowsAnyAsync<Exception>(() => command.ExecuteNonQueryAsync(cts.Token));
        Assert.True(
            ex is OperationCanceledException or TaskCanceledException or PostgresException,
            $"Unexpected exception type: {ex.GetType().FullName}");
    }

    [PostgreSqlFact]
    public async Task Connection_remains_usable_after_cancellation()
    {
        await using var connection = new NpgsqlConnection(ConnectionString);
        await connection.OpenAsync();

        await using (var sleep = connection.CreateCommand())
        {
            sleep.CommandText = "SELECT pg_sleep(10)";
            using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(500));
            try
            {
                await sleep.ExecuteNonQueryAsync(cts.Token);
            }
            catch (Exception)
            {
                // expected
            }
        }

        await using var ping = connection.CreateCommand();
        ping.CommandText = "SELECT 1";
        var result = await ping.ExecuteScalarAsync();
        Assert.Equal(1, Convert.ToInt32(result));
    }

    [PostgreSqlFact]
    public async Task Ef_execute_sql_raw_respects_cancellation()
    {
        await using var scope = Factory.Services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(500));

        var ex = await Assert.ThrowsAnyAsync<Exception>(() =>
            context.Database.ExecuteSqlRawAsync("SELECT pg_sleep(10)", cts.Token));

        Assert.True(
            ex is OperationCanceledException or TaskCanceledException or PostgresException,
            $"Unexpected exception type: {ex.GetType().FullName}");

        var alive = await context.Database.ExecuteSqlRawAsync("SELECT 1");
        Assert.Equal(-1, alive); // SELECT returns no rows affected
    }
}

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
public sealed class CommandTimeoutIntegrationTests : IntegrationTestClassBase
{
    public CommandTimeoutIntegrationTests(PostgreSqlFixture fixture)
        : base(fixture)
    {
    }

    [PostgreSqlFact]
    public async Task Npgsql_command_timeout_one_second_fails_on_pg_sleep_five()
    {
        await using var connection = new NpgsqlConnection(ConnectionString);
        await connection.OpenAsync();

        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT pg_sleep(5)";
        command.CommandTimeout = 1;

        await Assert.ThrowsAsync<NpgsqlException>(() => command.ExecuteNonQueryAsync());
    }

    [PostgreSqlFact]
    public async Task Connection_still_works_after_command_timeout()
    {
        await using var connection = new NpgsqlConnection(ConnectionString);
        await connection.OpenAsync();

        await using (var timedOut = connection.CreateCommand())
        {
            timedOut.CommandText = "SELECT pg_sleep(5)";
            timedOut.CommandTimeout = 1;
            try
            {
                await timedOut.ExecuteNonQueryAsync();
            }
            catch (NpgsqlException)
            {
                // expected
            }
        }

        await using var ping = connection.CreateCommand();
        ping.CommandText = "SELECT 1";
        ping.CommandTimeout = 5;
        var result = await ping.ExecuteScalarAsync();
        Assert.Equal(1, Convert.ToInt32(result));
    }

    [PostgreSqlFact]
    public async Task Ef_command_timeout_one_second_fails_on_pg_sleep_five()
    {
        await using var scope = Factory.Services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        context.Database.SetCommandTimeout(1);

        await Assert.ThrowsAnyAsync<Exception>(() =>
            context.Database.ExecuteSqlRawAsync("SELECT pg_sleep(5)"));

        context.Database.SetCommandTimeout(30);
        var alive = await context.Database.ExecuteSqlRawAsync("SELECT 1");
        Assert.Equal(-1, alive);
    }
}

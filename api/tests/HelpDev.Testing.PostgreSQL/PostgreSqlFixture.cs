using HelpDev.Infrastructure;
using HelpDev.Infrastructure.Persistence;
using HelpDev.SharedInfrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Pgvector.EntityFrameworkCore;
using Testcontainers.PostgreSql;
using Xunit;

namespace HelpDev.Testing.PostgreSQL;

public sealed class PostgreSqlFixture : IAsyncLifetime
{
    private PostgreSqlContainer? _container;
    private string? _adminConnectionString;
    private readonly List<string> _createdDatabases = [];
    private readonly object _createdLock = new();

    public string ConnectionString { get; private set; } = string.Empty;

    public bool IsAvailable { get; private set; }

    public async Task InitializeAsync()
    {
        var explicitUrl = Environment.GetEnvironmentVariable("TEST_DATABASE_URL");
        if (!string.IsNullOrWhiteSpace(explicitUrl))
        {
            ConnectionString = ApplyPoolLimits(explicitUrl);
            _adminConnectionString = BuildAdminConnectionString(ConnectionString);
            IsAvailable = await CanConnectAsync(ConnectionString);
            return;
        }

        try
        {
            _container = new PostgreSqlBuilder()
                .WithImage("pgvector/pgvector:pg16")
                .WithDatabase("helpdev_test")
                .WithUsername("postgres")
                .WithPassword("postgres")
                .WithCommand("-c", "max_connections=200")
                .Build();

            await _container.StartAsync();
            ConnectionString = ApplyPoolLimits(_container.GetConnectionString());
            _adminConnectionString = BuildAdminConnectionString(ConnectionString);
            IsAvailable = true;
        }
        catch (Exception ex)
        {
            IsAvailable = false;
            throw new InvalidOperationException(
                "PostgreSQL test infrastructure is unavailable. Start Docker or set TEST_DATABASE_URL.",
                ex);
        }
    }

    public async Task DisposeAsync()
    {
        try
        {
            NpgsqlConnection.ClearAllPools();
            string[] databases;
            lock (_createdLock)
            {
                databases = _createdDatabases.ToArray();
                _createdDatabases.Clear();
            }

            foreach (var database in databases)
            {
                await DropDatabaseByNameAsync(database);
            }
        }
        catch
        {
            // Best-effort cleanup before container disposal.
        }

        if (_container is not null)
        {
            await _container.DisposeAsync();
        }
    }

    public async Task<string> CreateIsolatedDatabaseAsync()
    {
        var databaseName = $"helpdev_{Guid.NewGuid():N}";
        await using var connection = new NpgsqlConnection(_adminConnectionString ?? ConnectionString);
        await connection.OpenAsync();
        await using var command = new NpgsqlCommand(
            $"CREATE DATABASE \"{databaseName}\"",
            connection);
        await command.ExecuteNonQueryAsync();

        lock (_createdLock)
        {
            _createdDatabases.Add(databaseName);
        }

        var builder = new NpgsqlConnectionStringBuilder(ConnectionString)
        {
            Database = databaseName,
            MaxPoolSize = 8,
            MinPoolSize = 0,
            Timeout = 15,
            CommandTimeout = 30,
        };
        return builder.ConnectionString;
    }

    public async Task DropIsolatedDatabaseAsync(string connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return;
        }

        try
        {
            NpgsqlConnection.ClearPool(new NpgsqlConnection(connectionString));
        }
        catch
        {
            // Pool may already be disposed.
        }

        var databaseName = new NpgsqlConnectionStringBuilder(connectionString).Database;
        if (string.IsNullOrWhiteSpace(databaseName))
        {
            return;
        }

        await DropDatabaseByNameAsync(databaseName);
        lock (_createdLock)
        {
            _createdDatabases.Remove(databaseName);
        }
    }

    private async Task DropDatabaseByNameAsync(string databaseName)
    {
        if (string.Equals(databaseName, "postgres", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(databaseName, "helpdev_test", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        await using var connection = new NpgsqlConnection(_adminConnectionString ?? ConnectionString);
        await connection.OpenAsync();
        await using (var terminate = new NpgsqlCommand(
            """
            SELECT pg_terminate_backend(pid)
            FROM pg_stat_activity
            WHERE datname = @db
              AND pid <> pg_backend_pid()
            """,
            connection))
        {
            terminate.Parameters.AddWithValue("db", databaseName);
            await terminate.ExecuteNonQueryAsync();
        }

        await using var drop = new NpgsqlCommand(
            $"DROP DATABASE IF EXISTS \"{databaseName}\"",
            connection);
        await drop.ExecuteNonQueryAsync();
    }

    public static IConfiguration BuildConfiguration(string connectionString) =>
        new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = connectionString,
                ["Jwt:Secret"] = "HelpDev_Integration_Test_Secret_Key_32+",
                ["Jwt:Issuer"] = "HelpDev",
                ["Jwt:Audience"] = "HelpDev.Client",
                ["Jwt:ExpirationMinutes"] = "60",
                ["Auth:ExposeOtpInResponse"] = "true",
                ["Outbox:BatchSize"] = "20",
                ["Outbox:PollIntervalSeconds"] = "5",
                ["Outbox:LockDurationSeconds"] = "30",
                ["Outbox:MaxAttempts"] = "3",
                ["Security:PartitionHashKey"] = "HelpDev_Integration_Partition_Hash_Key_32+",
                ["Cors:FrontendOrigins:0"] = "http://localhost:3000",
            })
            .Build();

    private static string ApplyPoolLimits(string connectionString)
    {
        var builder = new NpgsqlConnectionStringBuilder(connectionString)
        {
            MaxPoolSize = 8,
            MinPoolSize = 0,
            Timeout = 15,
            CommandTimeout = 30,
        };
        return builder.ConnectionString;
    }

    private static string BuildAdminConnectionString(string connectionString)
    {
        var builder = new NpgsqlConnectionStringBuilder(connectionString)
        {
            Database = "postgres",
            MaxPoolSize = 4,
            MinPoolSize = 0,
        };
        return builder.ConnectionString;
    }

    private static async Task<bool> CanConnectAsync(string connectionString)
    {
        try
        {
            await using var connection = new NpgsqlConnection(connectionString);
            await connection.OpenAsync();
            return true;
        }
        catch
        {
            return false;
        }
    }
}

public abstract class PostgreSqlIntegrationTestBase
{
    protected PostgreSqlIntegrationTestBase(PostgreSqlFixture fixture)
    {
        Fixture = fixture;
    }

    protected PostgreSqlFixture Fixture { get; }

    protected async Task<string> CreateDatabaseAndMigrateAsync()
    {
        var connectionString = await Fixture.CreateIsolatedDatabaseAsync();
        var services = new ServiceCollection();
        var configuration = PostgreSqlFixture.BuildConfiguration(connectionString);
        services.AddSingleton<IConfiguration>(configuration);
        services.AddLogging();
        services.AddSharedInfrastructure();
        services.AddInfrastructure(configuration);

        await using var provider = services.BuildServiceProvider();
        var context = provider.GetRequiredService<ApplicationDbContext>();
        await context.Database.MigrateAsync();
        return connectionString;
    }

    protected static async Task<ApplicationDbContext> CreateContextAsync(
        string connectionString)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseNpgsql(
                connectionString,
                npgsql =>
                {
                    npgsql.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.GetName().Name);
                    npgsql.UseVector();
                })
            .Options;

        var context = new ApplicationDbContext(options);
        await context.Database.MigrateAsync();
        return context;
    }
}

public sealed class PostgreSqlFactAttribute : FactAttribute
{
    public PostgreSqlFactAttribute()
    {
        if (string.Equals(Environment.GetEnvironmentVariable("CI"), "true", StringComparison.OrdinalIgnoreCase) &&
            string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("TEST_DATABASE_URL")))
        {
            // CI must provide Docker/Testcontainers or TEST_DATABASE_URL.
        }
    }
}

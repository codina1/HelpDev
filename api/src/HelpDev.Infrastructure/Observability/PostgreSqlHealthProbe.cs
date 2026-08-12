using HelpDev.SharedContracts.Observability;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Npgsql;

namespace HelpDev.Infrastructure.Observability;

public sealed class PostgreSqlHealthProbe : IPostgreSqlHealthProbe
{
    private readonly string _connectionString;
    private readonly int _timeoutSeconds;
    private readonly int _degradedLatencyMs;
    private readonly int _unhealthyLatencyMs;

    public PostgreSqlHealthProbe(
        IConfiguration configuration,
        IOptions<ObservabilityOptions> options)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("DefaultConnection is not configured.");
        _timeoutSeconds = options.Value.PostgreSql.TimeoutSeconds;
        _degradedLatencyMs = options.Value.PostgreSql.DegradedLatencyMilliseconds;
        _unhealthyLatencyMs = options.Value.PostgreSql.UnhealthyLatencyMilliseconds;
    }

    public async Task<PostgreSqlHealthProbeResult> CheckAsync(CancellationToken cancellationToken = default)
    {
        using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeoutCts.CancelAfter(TimeSpan.FromSeconds(_timeoutSeconds));

        var started = DateTime.UtcNow;
        try
        {
            await using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync(timeoutCts.Token);
            await using var command = new NpgsqlCommand("SELECT 1", connection)
            {
                CommandTimeout = _timeoutSeconds,
            };
            await command.ExecuteScalarAsync(timeoutCts.Token);

            var latency = (long)(DateTime.UtcNow - started).TotalMilliseconds;
            if (latency >= _unhealthyLatencyMs)
            {
                return new PostgreSqlHealthProbeResult(
                    true,
                    latency,
                    OperationalHealthStates.Degraded,
                    HealthCheckCodes.PostgreSqlSlow);
            }

            if (latency >= _degradedLatencyMs)
            {
                return new PostgreSqlHealthProbeResult(
                    true,
                    latency,
                    OperationalHealthStates.Degraded,
                    HealthCheckCodes.PostgreSqlSlow);
            }

            return new PostgreSqlHealthProbeResult(
                true,
                latency,
                OperationalHealthStates.Healthy,
                null);
        }
        catch (OperationCanceledException) when (timeoutCts.IsCancellationRequested && !cancellationToken.IsCancellationRequested)
        {
            return new PostgreSqlHealthProbeResult(
                false,
                (long)(DateTime.UtcNow - started).TotalMilliseconds,
                OperationalHealthStates.Unhealthy,
                HealthCheckCodes.Timeout);
        }
        catch (Exception)
        {
            return new PostgreSqlHealthProbeResult(
                false,
                (long)(DateTime.UtcNow - started).TotalMilliseconds,
                OperationalHealthStates.Unhealthy,
                HealthCheckCodes.PostgreSqlUnavailable);
        }
    }
}

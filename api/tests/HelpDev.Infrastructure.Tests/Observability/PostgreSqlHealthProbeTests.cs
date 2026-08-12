using HelpDev.Infrastructure.Observability;
using HelpDev.SharedContracts.Observability;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace HelpDev.Infrastructure.Tests.Observability;

public sealed class PostgreSqlHealthProbeTests
{
    [Fact]
    public void Constructor_throws_when_default_connection_missing()
    {
        var configuration = new ConfigurationBuilder().Build();

        var exception = Assert.Throws<InvalidOperationException>(() =>
            new PostgreSqlHealthProbe(configuration, Options.Create(new ObservabilityOptions())));

        Assert.Contains("DefaultConnection", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Constructor_does_not_take_DbContext()
    {
        var ctor = typeof(PostgreSqlHealthProbe).GetConstructors().Single();

        Assert.DoesNotContain(
            ctor.GetParameters(),
            parameter => parameter.ParameterType.Name.Contains("DbContext", StringComparison.Ordinal));
    }

    [Fact]
    public async Task CheckAsync_returns_unhealthy_when_connection_fails()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = "Host=127.0.0.1;Port=1;Database=missing;Username=missing;Password=missing;Timeout=1",
            })
            .Build();
        var probe = new PostgreSqlHealthProbe(
            configuration,
            Options.Create(new ObservabilityOptions
            {
                PostgreSql = new PostgreSqlHealthOptions
                {
                    TimeoutSeconds = 1,
                },
            }));

        var result = await probe.CheckAsync(CancellationToken.None);

        Assert.False(result.IsAvailable);
        Assert.Equal(OperationalHealthStates.Unhealthy, result.Status);
        Assert.True(
            result.Code is HealthCheckCodes.PostgreSqlUnavailable or HealthCheckCodes.Timeout,
            $"Unexpected code: {result.Code}");
    }
}

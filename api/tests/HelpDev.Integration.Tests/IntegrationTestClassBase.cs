using System.Collections.Concurrent;
using HelpDev.Infrastructure.Observability;
using HelpDev.Integration.Tests.Helpers;
using HelpDev.Testing.PostgreSQL;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace HelpDev.Integration.Tests;

[Collection(PostgreSqlCollection.Name)]
public abstract class IntegrationTestClassBase : IAsyncLifetime, IDisposable
{
    private bool _disposed;

    protected IntegrationTestClassBase(PostgreSqlFixture fixture)
    {
        Fixture = fixture;
    }

    protected PostgreSqlFixture Fixture { get; }

    protected HelpDevWebApplicationFactory Factory { get; private set; } = null!;

    protected HttpClient Client { get; private set; } = null!;

    protected string ConnectionString { get; private set; } = string.Empty;

    protected ConcurrentBag<CapturedLogEntry> CapturedLogs => Factory.CapturedLogs;

    protected TestAuditPersistenceFailureInjector AuditFailureInjector => Factory.AuditFailureInjector;

    protected TestAnalyticsFailureInjector AnalyticsFailureInjector => Factory.AnalyticsFailureInjector;

    protected AuthenticatedClientFactory AuthClients { get; private set; } = null!;

    protected virtual IReadOnlyDictionary<string, string?>? ConfigurationOverrides => null;

    protected virtual Action<IServiceCollection>? ConfigureTestServices => null;

    public async Task InitializeAsync()
    {
        ConnectionString = await Fixture.CreateIsolatedDatabaseAsync();
        Factory = new HelpDevWebApplicationFactory(
            ConnectionString,
            ConfigureTestServices,
            ConfigurationOverrides);
        Client = Factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
        });
        AuthClients = new AuthenticatedClientFactory(Factory);
        Factory.ClearCapturedLogs();
        AuditFailureInjector.Reset();
        AnalyticsFailureInjector.Reset();

        var heartbeat = Factory.Services.GetRequiredService<OutboxProcessorHeartbeat>();
        heartbeat.MarkCycleCompleted(DateTime.UtcNow, hadSuccessfulProcessing: true);
    }

    public async Task DisposeAsync()
    {
        Dispose(true);
        if (!string.IsNullOrWhiteSpace(ConnectionString))
        {
            await Fixture.DropIsolatedDatabaseAsync(ConnectionString);
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed || !disposing)
        {
            return;
        }

        Client?.Dispose();
        Factory?.Dispose();
        _disposed = true;
    }
}

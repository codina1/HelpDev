using HelpDev.Infrastructure.Persistence;
using HelpDev.Modules.Analytics.Application.Processing;
using HelpDev.Modules.Analytics.Domain;
using HelpDev.Modules.Analytics.Domain.Metrics;
using HelpDev.SharedContracts.Analytics;
using HelpDev.Testing.PostgreSQL;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace HelpDev.Integration.Tests.Analytics;

[Collection(PostgreSqlCollection.Name)]
[Trait("Category", "Integration")]
[Trait("Category", "PostgreSQL")]
public sealed class AnalyticsPostgreSqlConcurrencyTests : IntegrationTestClassBase
{
    public AnalyticsPostgreSqlConcurrencyTests(PostgreSqlFixture fixture)
        : base(fixture)
    {
    }

    [PostgreSqlFact]
    public async Task Fifty_concurrent_distinct_event_ids_all_commit()
    {
        var now = DateTime.UtcNow;

        var tasks = Enumerable.Range(0, 50)
            .Select(_ => Task.Run(async () =>
            {
                await using var taskScope = Factory.Services.CreateAsyncScope();
                var taskProcessor = taskScope.ServiceProvider.GetRequiredService<IAnalyticsEventProcessor>();
                return await taskProcessor.ProcessAsync(
                    new AnalyticsEventEnvelope(
                        EventId: Guid.NewGuid(),
                        EventType: AnalyticsEventTypes.IdentityUserLoginSucceeded,
                        OccurredAtUtc: now,
                        ActorUserId: Guid.NewGuid(),
                        SubjectId: null,
                        SubjectType: null,
                        Dimensions: null,
                        Quantity: 1,
                        DurationMilliseconds: null,
                        SubjectDisplayName: null,
                        SubjectSlug: null,
                        SchemaVersion: 1),
                    CancellationToken.None);
            }))
            .ToArray();

        var results = await Task.WhenAll(tasks);

        Assert.All(results, result =>
        {
            Assert.False(result.WasDuplicate);
            Assert.True(result.Committed);
        });

        await using var verifyScope = Factory.Services.CreateAsyncScope();
        var context = verifyScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        Assert.Equal(50, await context.AnalyticsEventReceipts.CountAsync());
    }

    [PostgreSqlFact]
    public async Task Concurrent_duplicate_event_id_results_in_single_receipt_and_safe_metric_increment()
    {
        await using var scope = Factory.Services.CreateAsyncScope();
        var processor = scope.ServiceProvider.GetRequiredService<IAnalyticsEventProcessor>();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var eventId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var now = DateTime.UtcNow;
        var envelope = new AnalyticsEventEnvelope(
            EventId: eventId,
            EventType: AnalyticsEventTypes.IdentityUserLoginSucceeded,
            OccurredAtUtc: now,
            ActorUserId: userId,
            SubjectId: userId,
            SubjectType: null,
            Dimensions: null,
            Quantity: 1,
            DurationMilliseconds: null,
            SubjectDisplayName: null,
            SubjectSlug: null,
            SchemaVersion: 1);

        var identity = new DailyMetricIdentity(
            DateOnly.FromDateTime(now),
            AnalyticsMetricKeys.UsersLoginSucceeded,
            null,
            null,
            string.Empty,
            string.Empty,
            string.Empty,
            string.Empty);

        var concurrentTasks = Enumerable.Range(0, 20)
            .Select(_ => Task.Run(async () =>
            {
                await using var taskScope = Factory.Services.CreateAsyncScope();
                var taskProcessor = taskScope.ServiceProvider.GetRequiredService<IAnalyticsEventProcessor>();
                return await taskProcessor.ProcessAsync(envelope, CancellationToken.None);
            }))
            .ToArray();

        var results = await Task.WhenAll(concurrentTasks);

        Assert.Equal(1, await context.AnalyticsEventReceipts.CountAsync(receipt => receipt.EventId == eventId));
        Assert.Contains(results, result => result.Committed && !result.WasDuplicate);
        Assert.Contains(results, result => result.WasDuplicate && !result.Committed);

        var metric = await context.DailyMetrics.SingleAsync(
            row =>
                row.MetricKey == AnalyticsMetricKeys.UsersLoginSucceeded
                && row.DateUtc == DateOnly.FromDateTime(now));

        Assert.InRange(metric.Count, 1, 20);
    }
}

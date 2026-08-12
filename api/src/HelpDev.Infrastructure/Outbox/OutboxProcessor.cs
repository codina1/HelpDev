using HelpDev.Infrastructure.Observability;
using HelpDev.SharedApplication.Abstractions.Events;
using HelpDev.SharedContracts.Observability;
using HelpDev.SharedInfrastructure.Outbox;
using HelpDev.SharedKernel.Time;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace HelpDev.Infrastructure.Outbox;

/// <summary>
/// Hosted outbox processor. Creates a DI scope per iteration; never holds ApplicationDbContext as a field.
/// </summary>
public sealed class OutboxProcessor : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IOptions<OutboxOptions> _options;
    private readonly OutboxProcessorHeartbeat _heartbeat;
    private readonly ILogger<OutboxProcessor> _logger;

    public OutboxProcessor(
        IServiceScopeFactory scopeFactory,
        IOptions<OutboxOptions> options,
        OutboxProcessorHeartbeat heartbeat,
        ILogger<OutboxProcessor> logger)
    {
        _scopeFactory = scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory));
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _heartbeat = heartbeat ?? throw new ArgumentNullException(nameof(heartbeat));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var pollDelay = TimeSpan.FromSeconds(Math.Max(1, _options.Value.PollIntervalSeconds));

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                _heartbeat.MarkCycleStarted(DateTime.UtcNow);
                _logger.LogInformation("Event={Event}", LoggingEventNames.OutboxProcessorCycleStarted);
                await ProcessBatchAsync(stoppingToken).ConfigureAwait(false);
                _heartbeat.MarkCycleCompleted(DateTime.UtcNow, hadSuccessfulProcessing: true);
                _logger.LogInformation("Event={Event}", LoggingEventNames.OutboxProcessorCycleCompleted);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _heartbeat.MarkCycleFailed(DateTime.UtcNow, "outbox_processor_cycle_failed");
                _logger.LogError(ex, "Event={Event}", LoggingEventNames.OutboxProcessorCycleFailed);
            }

            try
            {
                await Task.Delay(pollDelay, stoppingToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
        }
    }

    public async Task ProcessBatchAsync(CancellationToken cancellationToken)
    {
        await using var scope = _scopeFactory.CreateAsyncScope();
        var store = scope.ServiceProvider.GetRequiredService<IOutboxMessageStore>();
        var serializer = scope.ServiceProvider.GetRequiredService<IOutboxEventSerializer>();
        var dispatcher = scope.ServiceProvider.GetRequiredService<IDomainEventDispatcher>();
        var clock = scope.ServiceProvider.GetRequiredService<IDateTimeProvider>();

        var lockId = $"{Environment.MachineName}:{Guid.NewGuid():N}";
        var claimed = await store.ClaimPendingAsync(lockId, cancellationToken).ConfigureAwait(false);
        if (claimed.Count == 0)
        {
            return;
        }

        _logger.LogInformation(
            "Outbox claimed {Count} message(s) at {UtcNow}.",
            claimed.Count,
            clock.UtcNow);

        foreach (var message in claimed)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                var domainEvent = serializer.Deserialize(message.Type, message.Payload);
                await dispatcher.DispatchAsync([domainEvent], cancellationToken).ConfigureAwait(false);
                await store.MarkProcessedAsync(message, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                // Do not log payloads.
                _logger.LogWarning(
                    ex,
                    "Outbox message {MessageId} of type {EventType} failed processing.",
                    message.Id,
                    message.Type);

                await store.MarkFailedAsync(
                        message,
                        $"{ex.GetType().Name}: {ex.Message}",
                        cancellationToken)
                    .ConfigureAwait(false);
            }
        }
    }
}

using HelpDev.SharedApplication.Abstractions.Events;
using HelpDev.SharedKernel.Events;

namespace HelpDev.SharedInfrastructure.Events;

/// <summary>
/// No-op dispatcher used when a real dispatcher is unavailable (for example EF design-time).
/// </summary>
public sealed class NullDomainEventDispatcher : IDomainEventDispatcher
{
    public static NullDomainEventDispatcher Instance { get; } = new();

    private NullDomainEventDispatcher()
    {
    }

    public Task DispatchAsync(
        IEnumerable<IDomainEvent> domainEvents,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(domainEvents);
        return Task.CompletedTask;
    }
}

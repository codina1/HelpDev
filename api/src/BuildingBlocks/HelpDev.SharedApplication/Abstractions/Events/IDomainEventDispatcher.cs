using HelpDev.SharedKernel.Events;

namespace HelpDev.SharedApplication.Abstractions.Events;

/// <summary>
/// Dispatches domain events raised by aggregates after a successful unit of work.
/// Implementations live in SharedInfrastructure or module infrastructure.
/// </summary>
public interface IDomainEventDispatcher
{
    Task DispatchAsync(
        IEnumerable<IDomainEvent> domainEvents,
        CancellationToken cancellationToken = default);
}

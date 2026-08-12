namespace HelpDev.SharedKernel.Events;

/// <summary>
/// Marks an aggregate that buffers domain events for later infrastructure-owned dispatch.
/// </summary>
public interface IHasDomainEvents
{
    IReadOnlyCollection<IDomainEvent> DomainEvents { get; }

    void ClearDomainEvents();
}

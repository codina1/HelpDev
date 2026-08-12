namespace HelpDev.SharedKernel.Events;

public abstract record DomainEvent : IDomainEvent
{
    protected DomainEvent()
        : this(Guid.NewGuid(), DateTime.UtcNow)
    {
    }

    protected DomainEvent(Guid eventId, DateTime occurredAtUtc)
    {
        EventId = eventId;
        OccurredAtUtc = occurredAtUtc;
    }

    public Guid EventId { get; init; }

    public DateTime OccurredAtUtc { get; init; }
}

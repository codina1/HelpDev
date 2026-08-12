namespace HelpDev.SharedContracts.IntegrationEvents;

public abstract record IntegrationEvent : IIntegrationEvent
{
    protected IntegrationEvent()
        : this(DateTime.UtcNow)
    {
    }

    protected IntegrationEvent(DateTime occurredAtUtc)
    {
        Id = Guid.NewGuid();
        OccurredAtUtc = occurredAtUtc.Kind == DateTimeKind.Utc
            ? occurredAtUtc
            : occurredAtUtc.ToUniversalTime();
    }

    public Guid Id { get; init; }

    public DateTime OccurredAtUtc { get; init; }
}

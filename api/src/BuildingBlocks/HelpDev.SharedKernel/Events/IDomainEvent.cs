namespace HelpDev.SharedKernel.Events;

public interface IDomainEvent
{
    Guid EventId { get; }

    DateTime OccurredAtUtc { get; }
}

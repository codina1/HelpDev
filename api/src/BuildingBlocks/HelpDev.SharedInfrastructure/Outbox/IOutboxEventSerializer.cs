using HelpDev.SharedKernel.Events;

namespace HelpDev.SharedInfrastructure.Outbox;

public interface IOutboxEventSerializer
{
    OutboxSerializedEvent Serialize(IDomainEvent domainEvent);

    IDomainEvent Deserialize(string stableTypeName, string payload);
}

public sealed record OutboxSerializedEvent(
    Guid Id,
    DateTime OccurredAtUtc,
    string Type,
    string Payload);

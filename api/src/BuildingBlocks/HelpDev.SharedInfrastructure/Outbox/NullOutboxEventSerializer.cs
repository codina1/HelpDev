using HelpDev.SharedInfrastructure.Outbox;
using HelpDev.SharedKernel.Events;

namespace HelpDev.SharedInfrastructure.Outbox;

/// <summary>
/// Design-time placeholder. Serialization is not available without a configured registry.
/// </summary>
public sealed class NullOutboxEventSerializer : IOutboxEventSerializer
{
    public static NullOutboxEventSerializer Instance { get; } = new();

    private NullOutboxEventSerializer()
    {
    }

    public OutboxSerializedEvent Serialize(IDomainEvent domainEvent) =>
        throw new InvalidOperationException("Outbox event serializer is not configured.");

    public IDomainEvent Deserialize(string stableTypeName, string payload) =>
        throw new InvalidOperationException("Outbox event serializer is not configured.");
}

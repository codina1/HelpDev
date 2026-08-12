using HelpDev.SharedInfrastructure.Events;
using HelpDev.SharedInfrastructure.Outbox;

namespace HelpDev.Infrastructure.Outbox;

/// <summary>
/// Builds OutboxMessage rows from captured Domain Event snapshots (no DbContext).
/// </summary>
public static class OutboxCapture
{
    public static IReadOnlyList<OutboxMessage> CreateMessages(
        IReadOnlyList<DomainEventSnapshot> snapshots,
        IOutboxEventSerializer serializer)
    {
        ArgumentNullException.ThrowIfNull(snapshots);
        ArgumentNullException.ThrowIfNull(serializer);

        if (snapshots.Count == 0)
        {
            return Array.Empty<OutboxMessage>();
        }

        var events = DomainEventCommitPipeline.Flatten(snapshots);
        var messages = new List<OutboxMessage>(events.Count);

        foreach (var domainEvent in events)
        {
            var serialized = serializer.Serialize(domainEvent);
            messages.Add(new OutboxMessage
            {
                Id = serialized.Id,
                OccurredAtUtc = serialized.OccurredAtUtc,
                Type = serialized.Type,
                Payload = serialized.Payload,
                AttemptCount = 0,
            });
        }

        return messages;
    }
}

using System.Text.Json;
using System.Text.Json.Serialization;
using HelpDev.SharedKernel.Events;

namespace HelpDev.SharedInfrastructure.Outbox;

/// <summary>
/// Serializes domain events using a controlled type registry. Never loads types from payload strings.
/// </summary>
public sealed class SystemTextJsonOutboxEventSerializer : IOutboxEventSerializer
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        WriteIndented = false,
    };

    private readonly IOutboxEventTypeRegistry _registry;

    public SystemTextJsonOutboxEventSerializer(IOutboxEventTypeRegistry registry)
    {
        _registry = registry ?? throw new ArgumentNullException(nameof(registry));
    }

    public OutboxSerializedEvent Serialize(IDomainEvent domainEvent)
    {
        ArgumentNullException.ThrowIfNull(domainEvent);

        var clrType = domainEvent.GetType();
        var stableName = _registry.GetStableName(clrType);
        var payload = JsonSerializer.Serialize(domainEvent, clrType, SerializerOptions);

        return new OutboxSerializedEvent(
            domainEvent.EventId,
            domainEvent.OccurredAtUtc,
            stableName,
            payload);
    }

    public IDomainEvent Deserialize(string stableTypeName, string payload)
    {
        if (string.IsNullOrWhiteSpace(stableTypeName))
        {
            throw new ArgumentException("Stable event type name is required.", nameof(stableTypeName));
        }

        if (string.IsNullOrWhiteSpace(payload))
        {
            throw new ArgumentException("Outbox payload is required.", nameof(payload));
        }

        // Registry lookup only — never Type.GetType / Assembly.Load from payload content.
        var clrType = _registry.GetClrType(stableTypeName);

        object? deserialized;
        try
        {
            deserialized = JsonSerializer.Deserialize(payload, clrType, SerializerOptions);
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException(
                $"Outbox payload for '{stableTypeName}' is malformed.",
                ex);
        }

        if (deserialized is not IDomainEvent domainEvent)
        {
            throw new InvalidOperationException(
                $"Deserialized outbox payload for '{stableTypeName}' is not an IDomainEvent.");
        }

        return domainEvent;
    }
}

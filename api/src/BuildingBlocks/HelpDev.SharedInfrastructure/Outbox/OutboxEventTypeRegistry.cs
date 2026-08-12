using HelpDev.SharedKernel.Events;

namespace HelpDev.SharedInfrastructure.Outbox;

public sealed class OutboxEventTypeRegistry : IOutboxEventTypeRegistry
{
    private readonly Dictionary<Type, string> _typeToName = new();
    private readonly Dictionary<string, Type> _nameToType = new(StringComparer.Ordinal);
    private bool _sealed;

    public IReadOnlyCollection<Type> RegisteredEventTypes => _typeToName.Keys;

    public void Register<TEvent>(string stableName)
        where TEvent : IDomainEvent =>
        Register(typeof(TEvent), stableName);

    public void Register(Type clrType, string stableName)
    {
        ArgumentNullException.ThrowIfNull(clrType);
        if (_sealed)
        {
            throw new InvalidOperationException("Outbox event type registry is sealed and cannot accept new mappings.");
        }

        if (!typeof(IDomainEvent).IsAssignableFrom(clrType) || clrType.IsInterface || clrType.IsAbstract)
        {
            throw new ArgumentException(
                $"Type '{clrType.FullName}' must be a concrete IDomainEvent implementation.",
                nameof(clrType));
        }

        if (string.IsNullOrWhiteSpace(stableName))
        {
            throw new ArgumentException("Stable event name is required.", nameof(stableName));
        }

        var normalized = stableName.Trim();

        if (_typeToName.ContainsKey(clrType))
        {
            throw new InvalidOperationException(
                $"Domain event type '{clrType.FullName}' is already registered.");
        }

        if (_nameToType.ContainsKey(normalized))
        {
            throw new InvalidOperationException(
                $"Stable outbox event name '{normalized}' is already registered.");
        }

        _typeToName.Add(clrType, normalized);
        _nameToType.Add(normalized, clrType);
    }

    public void Seal() => _sealed = true;

    public string GetStableName(Type clrType)
    {
        ArgumentNullException.ThrowIfNull(clrType);
        if (!_typeToName.TryGetValue(clrType, out var name))
        {
            throw new InvalidOperationException(
                $"No outbox mapping is registered for domain event type '{clrType.FullName}'.");
        }

        return name;
    }

    public Type GetClrType(string stableName)
    {
        if (!TryGetClrType(stableName, out var clrType) || clrType is null)
        {
            throw new InvalidOperationException(
                $"No outbox mapping is registered for stable event name '{stableName}'.");
        }

        return clrType;
    }

    public bool TryGetClrType(string stableName, out Type? clrType)
    {
        if (string.IsNullOrWhiteSpace(stableName))
        {
            clrType = null;
            return false;
        }

        return _nameToType.TryGetValue(stableName.Trim(), out clrType);
    }
}

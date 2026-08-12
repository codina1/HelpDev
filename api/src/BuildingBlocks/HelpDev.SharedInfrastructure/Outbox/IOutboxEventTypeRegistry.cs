using HelpDev.SharedKernel.Events;

namespace HelpDev.SharedInfrastructure.Outbox;

public interface IOutboxEventTypeRegistry
{
    void Register(Type clrType, string stableName);

    void Register<TEvent>(string stableName)
        where TEvent : IDomainEvent;

    string GetStableName(Type clrType);

    Type GetClrType(string stableName);

    bool TryGetClrType(string stableName, out Type? clrType);

    IReadOnlyCollection<Type> RegisteredEventTypes { get; }
}

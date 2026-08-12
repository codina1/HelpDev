namespace HelpDev.SharedContracts.IntegrationEvents;

public interface IIntegrationEvent
{
    Guid Id { get; }

    DateTime OccurredAtUtc { get; }
}

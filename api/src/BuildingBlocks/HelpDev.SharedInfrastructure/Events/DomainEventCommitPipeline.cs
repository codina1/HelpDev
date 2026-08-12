using HelpDev.SharedKernel.Events;

namespace HelpDev.SharedInfrastructure.Events;

/// <summary>
/// Snapshots aggregate domain events for transactional Outbox persistence.
/// Direct post-save dispatch is intentionally not performed here.
/// </summary>
public static class DomainEventCommitPipeline
{
    public static IReadOnlyList<DomainEventSnapshot> Capture(IEnumerable<IHasDomainEvents> aggregates)
    {
        ArgumentNullException.ThrowIfNull(aggregates);

        List<DomainEventSnapshot>? snapshots = null;

        foreach (var aggregate in aggregates)
        {
            if (aggregate.DomainEvents.Count == 0)
            {
                continue;
            }

            snapshots ??= [];
            snapshots.Add(new DomainEventSnapshot(aggregate, aggregate.DomainEvents.ToArray()));
        }

        return snapshots is null
            ? Array.Empty<DomainEventSnapshot>()
            : snapshots;
    }

    public static IReadOnlyList<IDomainEvent> Flatten(IReadOnlyList<DomainEventSnapshot> snapshots)
    {
        ArgumentNullException.ThrowIfNull(snapshots);

        if (snapshots.Count == 0)
        {
            return Array.Empty<IDomainEvent>();
        }

        var events = new List<IDomainEvent>(capacity: snapshots.Sum(static snapshot => snapshot.Events.Count));
        foreach (var snapshot in snapshots)
        {
            events.AddRange(snapshot.Events);
        }

        return events;
    }

    public static void ClearCaptured(IReadOnlyList<DomainEventSnapshot> snapshots)
    {
        ArgumentNullException.ThrowIfNull(snapshots);

        foreach (var snapshot in snapshots)
        {
            snapshot.Source.ClearDomainEvents();
        }
    }
}

public readonly record struct DomainEventSnapshot(
    IHasDomainEvents Source,
    IReadOnlyList<IDomainEvent> Events);

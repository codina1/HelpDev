using HelpDev.SharedKernel.Common;
using HelpDev.SharedKernel.Events;

namespace HelpDev.SharedKernel.Tests;

public sealed class AggregateRootTests
{
    [Fact]
    public void AddDomainEvent_registers_event_and_sets_HasDomainEvents()
    {
        var aggregate = new TestAggregate(Guid.NewGuid());
        var domainEvent = new TestDomainEvent("created");

        aggregate.Raise(domainEvent);

        Assert.True(aggregate.HasDomainEvents);
        Assert.Single(aggregate.DomainEvents);
        Assert.Same(domainEvent, aggregate.DomainEvents.Single());
    }

    [Fact]
    public void DomainEvents_preserves_insertion_order_for_multiple_events()
    {
        var aggregate = new TestAggregate(Guid.NewGuid());
        var first = new TestDomainEvent("first");
        var second = new TestDomainEvent("second");
        var third = new TestDomainEvent("third");

        aggregate.Raise(first);
        aggregate.Raise(second);
        aggregate.Raise(third);

        Assert.Equal(new IDomainEvent[] { first, second, third }, aggregate.DomainEvents);
    }

    [Fact]
    public void DomainEvents_exposes_read_only_collection()
    {
        var aggregate = new TestAggregate(Guid.NewGuid());
        aggregate.Raise(new TestDomainEvent("created"));

        Assert.IsAssignableFrom<IReadOnlyCollection<IDomainEvent>>(aggregate.DomainEvents);
        Assert.ThrowsAny<Exception>(() => ((ICollection<IDomainEvent>)aggregate.DomainEvents).Add(new TestDomainEvent("extra")));
    }

    [Fact]
    public void ClearDomainEvents_empties_buffer()
    {
        var aggregate = new TestAggregate(Guid.NewGuid());
        aggregate.Raise(new TestDomainEvent("created"));

        aggregate.ClearDomainEvents();

        Assert.False(aggregate.HasDomainEvents);
        Assert.Empty(aggregate.DomainEvents);
    }

    [Fact]
    public void AggregateRoot_implements_IHasDomainEvents()
    {
        IHasDomainEvents aggregate = new TestAggregate(Guid.NewGuid());
        var domainEvent = new TestDomainEvent("created");

        ((TestAggregate)aggregate).Raise(domainEvent);

        Assert.Same(domainEvent, Assert.Single(aggregate.DomainEvents));
        aggregate.ClearDomainEvents();
        Assert.Empty(aggregate.DomainEvents);
    }

    [Fact]
    public void RemoveDomainEvent_removes_registered_event()
    {
        var aggregate = new TestAggregate(Guid.NewGuid());
        var first = new TestDomainEvent("first");
        var second = new TestDomainEvent("second");
        aggregate.Raise(first);
        aggregate.Raise(second);

        aggregate.Drop(first);

        Assert.True(aggregate.HasDomainEvents);
        Assert.Single(aggregate.DomainEvents);
        Assert.Same(second, aggregate.DomainEvents.Single());
    }

    [Fact]
    public void DequeueDomainEvents_returns_events_and_clears_buffer()
    {
        var aggregate = new TestAggregate(Guid.NewGuid());
        var first = new TestDomainEvent("first");
        var second = new TestDomainEvent("second");
        aggregate.Raise(first);
        aggregate.Raise(second);

        var dequeued = aggregate.DequeueDomainEvents();

        Assert.Equal(2, dequeued.Count);
        Assert.Same(first, dequeued.ElementAt(0));
        Assert.Same(second, dequeued.ElementAt(1));
        Assert.False(aggregate.HasDomainEvents);
        Assert.Empty(aggregate.DomainEvents);
        Assert.Empty(aggregate.DequeueDomainEvents());
    }

    private sealed class TestAggregate : AggregateRoot<Guid>
    {
        public TestAggregate(Guid id)
            : base(id)
        {
        }

        public void Raise(IDomainEvent domainEvent) => AddDomainEvent(domainEvent);

        public void Drop(IDomainEvent domainEvent) => RemoveDomainEvent(domainEvent);
    }

    private sealed record TestDomainEvent(string Name) : DomainEvent;
}

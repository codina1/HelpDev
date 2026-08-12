using HelpDev.SharedApplication.Abstractions.Events;
using HelpDev.SharedInfrastructure;
using HelpDev.SharedInfrastructure.Events;
using HelpDev.SharedKernel.Common;
using HelpDev.SharedKernel.Events;
using Microsoft.Extensions.DependencyInjection;

namespace HelpDev.SharedInfrastructure.Tests;

public sealed class DomainEventDispatcherTests
{
    [Fact]
    public async Task Dispatch_with_no_handlers_succeeds()
    {
        await using var provider = BuildProvider();
        await using var scope = provider.CreateAsyncScope();
        var dispatcher = scope.ServiceProvider.GetRequiredService<IDomainEventDispatcher>();

        await dispatcher.DispatchAsync([new TestDomainEvent("a")]);
    }

    [Fact]
    public async Task Dispatch_invokes_single_handler()
    {
        var handler = new RecordingHandler();
        await using var provider = BuildProvider(services =>
        {
            services.AddSingleton<IDomainEventHandler<TestDomainEvent>>(handler);
        });
        await using var scope = provider.CreateAsyncScope();
        var dispatcher = scope.ServiceProvider.GetRequiredService<IDomainEventDispatcher>();
        var domainEvent = new TestDomainEvent("one");

        await dispatcher.DispatchAsync([domainEvent]);

        Assert.Same(domainEvent, Assert.Single(handler.Handled));
    }

    [Fact]
    public async Task Dispatch_invokes_multiple_handlers_for_one_event()
    {
        var first = new RecordingHandler("first");
        var second = new RecordingHandler("second");
        await using var provider = BuildProvider(services =>
        {
            services.AddSingleton<IDomainEventHandler<TestDomainEvent>>(first);
            services.AddSingleton<IDomainEventHandler<TestDomainEvent>>(second);
        });
        await using var scope = provider.CreateAsyncScope();
        var dispatcher = scope.ServiceProvider.GetRequiredService<IDomainEventDispatcher>();
        var domainEvent = new TestDomainEvent("shared");

        await dispatcher.DispatchAsync([domainEvent]);

        Assert.Same(domainEvent, Assert.Single(first.Handled));
        Assert.Same(domainEvent, Assert.Single(second.Handled));
    }

    [Fact]
    public async Task Dispatch_processes_multiple_events_in_order()
    {
        var handler = new RecordingHandler();
        await using var provider = BuildProvider(services =>
        {
            services.AddSingleton<IDomainEventHandler<TestDomainEvent>>(handler);
        });
        await using var scope = provider.CreateAsyncScope();
        var dispatcher = scope.ServiceProvider.GetRequiredService<IDomainEventDispatcher>();
        var first = new TestDomainEvent("first");
        var second = new TestDomainEvent("second");

        await dispatcher.DispatchAsync([first, second]);

        Assert.Equal(new[] { first, second }, handler.Handled);
    }

    [Fact]
    public async Task Dispatch_forwards_cancellation_token_to_handlers()
    {
        using var cts = new CancellationTokenSource();
        var handler = new TokenCapturingHandler();
        await using var provider = BuildProvider(services =>
        {
            services.AddSingleton<IDomainEventHandler<TestDomainEvent>>(handler);
        });
        await using var scope = provider.CreateAsyncScope();
        var dispatcher = scope.ServiceProvider.GetRequiredService<IDomainEventDispatcher>();

        await dispatcher.DispatchAsync([new TestDomainEvent("token")], cts.Token);

        Assert.Equal(cts.Token, handler.ReceivedToken);
    }

    [Fact]
    public async Task Dispatch_propagates_handler_exception_and_skips_later_handlers()
    {
        var failing = new FailingHandler();
        var later = new RecordingHandler();
        await using var provider = BuildProvider(services =>
        {
            services.AddSingleton<IDomainEventHandler<TestDomainEvent>>(failing);
            services.AddSingleton<IDomainEventHandler<TestDomainEvent>>(later);
        });
        await using var scope = provider.CreateAsyncScope();
        var dispatcher = scope.ServiceProvider.GetRequiredService<IDomainEventDispatcher>();

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => dispatcher.DispatchAsync([new TestDomainEvent("boom")]));

        Assert.Equal("handler failed", exception.Message);
        Assert.Empty(later.Handled);
    }

    [Fact]
    public void DomainEventDispatcher_type_does_not_reference_modules()
    {
        var referencedAssemblies = typeof(DomainEventDispatcher).Assembly
            .GetReferencedAssemblies()
            .Select(assembly => assembly.Name)
            .ToArray();

        Assert.DoesNotContain(referencedAssemblies, name => name is not null && name.StartsWith("HelpDev.Modules.", StringComparison.Ordinal));
    }

    private static ServiceProvider BuildProvider(Action<IServiceCollection>? configure = null)
    {
        var services = new ServiceCollection();
        services.AddSharedInfrastructure();
        configure?.Invoke(services);
        return services.BuildServiceProvider(validateScopes: true);
    }

    private sealed record TestDomainEvent(string Name) : DomainEvent;

    private sealed class RecordingHandler : IDomainEventHandler<TestDomainEvent>
    {
        public RecordingHandler(string? name = null)
        {
            Name = name;
        }

        public string? Name { get; }

        public List<TestDomainEvent> Handled { get; } = [];

        public Task HandleAsync(TestDomainEvent domainEvent, CancellationToken cancellationToken = default)
        {
            Handled.Add(domainEvent);
            return Task.CompletedTask;
        }
    }

    private sealed class TokenCapturingHandler : IDomainEventHandler<TestDomainEvent>
    {
        public CancellationToken ReceivedToken { get; private set; }

        public Task HandleAsync(TestDomainEvent domainEvent, CancellationToken cancellationToken = default)
        {
            ReceivedToken = cancellationToken;
            return Task.CompletedTask;
        }
    }

    private sealed class FailingHandler : IDomainEventHandler<TestDomainEvent>
    {
        public Task HandleAsync(TestDomainEvent domainEvent, CancellationToken cancellationToken = default) =>
            throw new InvalidOperationException("handler failed");
    }
}

public sealed class DomainEventCommitPipelineTests
{
    [Fact]
    public void Capture_snapshots_without_clearing()
    {
        var aggregate = new TestAggregate(Guid.NewGuid());
        var domainEvent = new TestDomainEvent("created");
        aggregate.Raise(domainEvent);

        var snapshots = DomainEventCommitPipeline.Capture([aggregate]);

        Assert.Same(domainEvent, Assert.Single(Assert.Single(snapshots).Events));
        Assert.True(aggregate.HasDomainEvents);
    }

    [Fact]
    public void Flatten_preserves_order_across_aggregates()
    {
        var first = new TestAggregate(Guid.NewGuid());
        var second = new TestAggregate(Guid.NewGuid());
        var a = new TestDomainEvent("a");
        var b = new TestDomainEvent("b");
        var c = new TestDomainEvent("c");
        first.Raise(a);
        first.Raise(b);
        second.Raise(c);

        var flattened = DomainEventCommitPipeline.Flatten(
            DomainEventCommitPipeline.Capture([first, second]));

        Assert.Equal(new IDomainEvent[] { a, b, c }, flattened);
    }

    [Fact]
    public void ClearCaptured_clears_aggregate_events_without_dispatch()
    {
        var aggregate = new TestAggregate(Guid.NewGuid());
        aggregate.Raise(new TestDomainEvent("created"));
        var snapshots = DomainEventCommitPipeline.Capture([aggregate]);

        DomainEventCommitPipeline.ClearCaptured(snapshots);

        Assert.False(aggregate.HasDomainEvents);
    }

    [Fact]
    public void Empty_snapshot_flatten_returns_empty()
    {
        Assert.Empty(DomainEventCommitPipeline.Flatten(Array.Empty<DomainEventSnapshot>()));
    }

    private sealed class TestAggregate : AggregateRoot<Guid>
    {
        public TestAggregate(Guid id)
            : base(id)
        {
        }

        public void Raise(IDomainEvent domainEvent) => AddDomainEvent(domainEvent);
    }

    private sealed record TestDomainEvent(string Name) : DomainEvent;
}

public sealed class DomainEventDispatcherDiTests
{
    [Fact]
    public void AddSharedInfrastructure_registers_scoped_dispatcher()
    {
        var services = new ServiceCollection();
        services.AddSharedInfrastructure();

        var descriptor = Assert.Single(
            services,
            service => service.ServiceType == typeof(IDomainEventDispatcher));

        Assert.Equal(typeof(DomainEventDispatcher), descriptor.ImplementationType);
        Assert.Equal(ServiceLifetime.Scoped, descriptor.Lifetime);
    }

    [Fact]
    public void Dispatcher_resolves_from_service_provider()
    {
        var services = new ServiceCollection();
        services.AddSharedInfrastructure();
        using var provider = services.BuildServiceProvider(validateScopes: true);

        using var scope = provider.CreateScope();
        var dispatcher = scope.ServiceProvider.GetRequiredService<IDomainEventDispatcher>();

        Assert.IsType<DomainEventDispatcher>(dispatcher);
    }

    [Fact]
    public void Multiple_handlers_for_one_event_can_resolve()
    {
        var services = new ServiceCollection();
        services.AddSharedInfrastructure();
        services.AddSingleton<IDomainEventHandler<SampleEvent>, FirstHandler>();
        services.AddSingleton<IDomainEventHandler<SampleEvent>, SecondHandler>();
        using var provider = services.BuildServiceProvider();

        var handlers = provider.GetServices<IDomainEventHandler<SampleEvent>>().ToArray();

        Assert.Equal(2, handlers.Length);
        Assert.Contains(handlers, handler => handler is FirstHandler);
        Assert.Contains(handlers, handler => handler is SecondHandler);
    }

    private sealed record SampleEvent : DomainEvent;

    private sealed class FirstHandler : IDomainEventHandler<SampleEvent>
    {
        public Task HandleAsync(SampleEvent domainEvent, CancellationToken cancellationToken = default) =>
            Task.CompletedTask;
    }

    private sealed class SecondHandler : IDomainEventHandler<SampleEvent>
    {
        public Task HandleAsync(SampleEvent domainEvent, CancellationToken cancellationToken = default) =>
            Task.CompletedTask;
    }
}

using System.Collections;
using System.Collections.Concurrent;
using System.Reflection;
using System.Runtime.ExceptionServices;
using HelpDev.SharedApplication.Abstractions.Events;
using HelpDev.SharedKernel.Events;

namespace HelpDev.SharedInfrastructure.Events;

/// <summary>
/// Resolves and invokes <see cref="IDomainEventHandler{TEvent}"/> handlers from the current DI scope.
/// </summary>
public sealed class DomainEventDispatcher : IDomainEventDispatcher
{
    private static readonly ConcurrentDictionary<Type, EventDispatchPlan> Plans = new();

    private readonly IServiceProvider _serviceProvider;

    public DomainEventDispatcher(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
    }

    public async Task DispatchAsync(
        IEnumerable<IDomainEvent> domainEvents,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(domainEvents);

        foreach (var domainEvent in domainEvents)
        {
            ArgumentNullException.ThrowIfNull(domainEvent);
            cancellationToken.ThrowIfCancellationRequested();

            var plan = Plans.GetOrAdd(domainEvent.GetType(), static eventType => EventDispatchPlan.Create(eventType));
            var handlers = _serviceProvider.GetService(plan.HandlerEnumerableType) as IEnumerable
                ?? Array.Empty<object>();

            foreach (var handler in handlers)
            {
                cancellationToken.ThrowIfCancellationRequested();
                await plan.InvokeAsync(handler, domainEvent, cancellationToken).ConfigureAwait(false);
            }
        }
    }

    private sealed class EventDispatchPlan
    {
        private readonly MethodInfo _handleAsyncMethod;

        private EventDispatchPlan(Type handlerEnumerableType, MethodInfo handleAsyncMethod)
        {
            HandlerEnumerableType = handlerEnumerableType;
            _handleAsyncMethod = handleAsyncMethod;
        }

        public Type HandlerEnumerableType { get; }

        public static EventDispatchPlan Create(Type eventType)
        {
            var handlerType = typeof(IDomainEventHandler<>).MakeGenericType(eventType);
            var handlerEnumerableType = typeof(IEnumerable<>).MakeGenericType(handlerType);
            var handleAsyncMethod = handlerType.GetMethod(
                nameof(IDomainEventHandler<IDomainEvent>.HandleAsync),
                BindingFlags.Instance | BindingFlags.Public,
                binder: null,
                types: [eventType, typeof(CancellationToken)],
                modifiers: null)
                ?? throw new InvalidOperationException(
                    $"Could not resolve HandleAsync for domain event type '{eventType.FullName}'.");

            return new EventDispatchPlan(handlerEnumerableType, handleAsyncMethod);
        }

        public Task InvokeAsync(object handler, IDomainEvent domainEvent, CancellationToken cancellationToken)
        {
            try
            {
                var result = _handleAsyncMethod.Invoke(handler, [domainEvent, cancellationToken])
                    ?? throw new InvalidOperationException("Domain event handler returned a null Task.");

                return (Task)result;
            }
            catch (TargetInvocationException exception) when (exception.InnerException is not null)
            {
                // Propagate the handler's own exception rather than a reflection wrapper.
                ExceptionDispatchInfo.Capture(exception.InnerException).Throw();
                throw; // unreachable, satisfies definite assignment
            }
        }
    }
}

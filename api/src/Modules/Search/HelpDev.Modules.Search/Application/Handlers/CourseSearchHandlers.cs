using HelpDev.Modules.Learning.Domain.Courses;
using HelpDev.SharedApplication.Abstractions.Events;

namespace HelpDev.Modules.Search.Application.Handlers;

public sealed class CoursePublishedSearchHandler : IDomainEventHandler<CoursePublishedDomainEvent>
{
    private readonly CourseSearchProjectionApplier _applier;

    public CoursePublishedSearchHandler(CourseSearchProjectionApplier applier)
    {
        _applier = applier;
    }

    public Task HandleAsync(
        CoursePublishedDomainEvent domainEvent,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(domainEvent);

        return _applier.ApplyAsync(
            domainEvent.CourseId,
            domainEvent.EventId,
            domainEvent.OccurredAtUtc,
            cancellationToken);
    }
}

public sealed class CourseUpdatedSearchHandler : IDomainEventHandler<CourseUpdatedDomainEvent>
{
    private readonly CourseSearchProjectionApplier _applier;

    public CourseUpdatedSearchHandler(CourseSearchProjectionApplier applier)
    {
        _applier = applier;
    }

    public Task HandleAsync(
        CourseUpdatedDomainEvent domainEvent,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(domainEvent);

        return _applier.ApplyAsync(
            domainEvent.CourseId,
            domainEvent.EventId,
            domainEvent.OccurredAtUtc,
            cancellationToken);
    }
}

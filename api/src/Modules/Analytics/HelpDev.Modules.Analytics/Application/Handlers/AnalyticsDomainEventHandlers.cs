using HelpDev.Modules.Content.Domain.Events;
using HelpDev.Modules.Learning.Domain.Courses;
using HelpDev.Modules.Learning.Domain.Enrollments;
using HelpDev.SharedApplication.Abstractions.Events;
using HelpDev.SharedContracts.Analytics;

namespace HelpDev.Modules.Analytics.Application.Handlers;

public sealed class ContentPublishedAnalyticsHandler : IDomainEventHandler<ContentPublishedDomainEvent>
{
    private readonly IAnalyticsEventIngestor _ingestor;

    public ContentPublishedAnalyticsHandler(IAnalyticsEventIngestor ingestor)
    {
        _ingestor = ingestor;
    }

    public Task HandleAsync(ContentPublishedDomainEvent domainEvent, CancellationToken cancellationToken = default)
    {
        return _ingestor.IngestAsync(
            new AnalyticsEventEnvelope(
                domainEvent.EventId,
                AnalyticsEventTypes.ContentItemPublished,
                domainEvent.OccurredAtUtc,
                ActorUserId: null,
                SubjectId: domainEvent.ContentId,
                SubjectType: Domain.AnalyticsSubjectTypes.Content,
                Dimensions: null,
                SubjectDisplayName: null,
                SubjectSlug: domainEvent.Slug),
            cancellationToken);
    }
}

public sealed class CoursePublishedAnalyticsHandler : IDomainEventHandler<CoursePublishedDomainEvent>
{
    private readonly IAnalyticsEventIngestor _ingestor;

    public CoursePublishedAnalyticsHandler(IAnalyticsEventIngestor ingestor)
    {
        _ingestor = ingestor;
    }

    public Task HandleAsync(CoursePublishedDomainEvent domainEvent, CancellationToken cancellationToken = default)
    {
        return _ingestor.IngestAsync(
            new AnalyticsEventEnvelope(
                domainEvent.EventId,
                AnalyticsEventTypes.LearningCoursePublished,
                domainEvent.OccurredAtUtc,
                ActorUserId: null,
                SubjectId: domainEvent.CourseId,
                SubjectType: Domain.AnalyticsSubjectTypes.Course,
                Dimensions: null,
                SubjectDisplayName: null,
                SubjectSlug: domainEvent.Slug),
            cancellationToken);
    }
}

public sealed class StudentEnrolledAnalyticsHandler : IDomainEventHandler<StudentEnrolledDomainEvent>
{
    private readonly IAnalyticsEventIngestor _ingestor;

    public StudentEnrolledAnalyticsHandler(IAnalyticsEventIngestor ingestor)
    {
        _ingestor = ingestor;
    }

    public Task HandleAsync(StudentEnrolledDomainEvent domainEvent, CancellationToken cancellationToken = default)
    {
        return _ingestor.IngestAsync(
            new AnalyticsEventEnvelope(
                domainEvent.EventId,
                AnalyticsEventTypes.LearningEnrollmentCreated,
                domainEvent.OccurredAtUtc,
                ActorUserId: domainEvent.UserId,
                SubjectId: domainEvent.CourseId,
                SubjectType: Domain.AnalyticsSubjectTypes.Course,
                Dimensions: null),
            cancellationToken);
    }
}

public sealed class LessonCompletedAnalyticsHandler : IDomainEventHandler<LessonCompletedDomainEvent>
{
    private readonly IAnalyticsEventIngestor _ingestor;

    public LessonCompletedAnalyticsHandler(IAnalyticsEventIngestor ingestor)
    {
        _ingestor = ingestor;
    }

    public Task HandleAsync(LessonCompletedDomainEvent domainEvent, CancellationToken cancellationToken = default)
    {
        return _ingestor.IngestAsync(
            new AnalyticsEventEnvelope(
                domainEvent.EventId,
                AnalyticsEventTypes.LearningLessonCompleted,
                domainEvent.OccurredAtUtc,
                ActorUserId: domainEvent.UserId,
                SubjectId: domainEvent.LessonId,
                SubjectType: null,
                Dimensions: null),
            cancellationToken);
    }
}

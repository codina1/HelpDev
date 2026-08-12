using HelpDev.SharedKernel.Events;

namespace HelpDev.Modules.Learning.Domain.Enrollments;

public sealed record StudentEnrolledDomainEvent(
    Guid EnrollmentId,
    Guid CourseId,
    Guid UserId) : DomainEvent;

public sealed record LessonCompletedDomainEvent(
    Guid EnrollmentId,
    Guid LessonId,
    Guid UserId) : DomainEvent;

public sealed record CourseCompletedDomainEvent(
    Guid EnrollmentId,
    Guid CourseId,
    Guid UserId) : DomainEvent;

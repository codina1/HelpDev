using HelpDev.SharedKernel.Events;

namespace HelpDev.Modules.Learning.Domain.Courses;

/// <summary>
/// Raised when a lesson becomes part of the published knowledge surface
/// (typically when its parent course is published or a published course is updated).
/// </summary>
public sealed record LessonPublishedDomainEvent(
    Guid LessonId,
    Guid CourseId,
    string CourseSlug) : DomainEvent;

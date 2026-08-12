using HelpDev.SharedKernel.Events;

namespace HelpDev.Modules.Learning.Domain.Courses;

public sealed record CourseUpdatedDomainEvent(Guid CourseId) : DomainEvent;

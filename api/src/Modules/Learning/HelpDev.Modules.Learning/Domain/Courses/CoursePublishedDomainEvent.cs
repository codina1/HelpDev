using HelpDev.SharedKernel.Events;

namespace HelpDev.Modules.Learning.Domain.Courses;

public sealed record CoursePublishedDomainEvent(Guid CourseId, string Slug) : DomainEvent;

using HelpDev.SharedKernel.Events;

namespace HelpDev.Modules.Content.Domain.Events;

public sealed record ContentPublishedDomainEvent(Guid ContentId, string Slug) : DomainEvent;
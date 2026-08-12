using HelpDev.SharedKernel.Events;

namespace HelpDev.Modules.Content.Domain.Events;

public sealed record ContentUpdatedDomainEvent(Guid ContentId, string Slug) : DomainEvent;
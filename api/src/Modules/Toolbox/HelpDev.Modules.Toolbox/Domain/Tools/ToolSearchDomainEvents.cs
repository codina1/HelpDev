using HelpDev.SharedKernel.Events;

namespace HelpDev.Modules.Toolbox.Domain.Tools;

public sealed record ToolPublishedDomainEvent(Guid ToolId, string Slug) : DomainEvent;

public sealed record ToolUnpublishedDomainEvent(Guid ToolId, string Slug) : DomainEvent;

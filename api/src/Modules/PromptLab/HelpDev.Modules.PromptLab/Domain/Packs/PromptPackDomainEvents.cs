using HelpDev.SharedKernel.Events;

namespace HelpDev.Modules.PromptLab.Domain.Packs;

public sealed record PromptPackApprovedDomainEvent(Guid PackId, string Slug) : DomainEvent;

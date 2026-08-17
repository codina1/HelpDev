using HelpDev.SharedKernel.Events;

namespace HelpDev.Modules.PromptLab.Domain.Prompts;

public sealed record PromptApprovedDomainEvent(Guid PromptId, string Slug) : DomainEvent;

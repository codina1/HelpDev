using HelpDev.SharedKernel.Events;

namespace HelpDev.Modules.PromptLab.Domain.Prompts;

public sealed record PromptPublishedDomainEvent(
    Guid PromptId,
    string Slug,
    int VersionNumber) : DomainEvent;

public sealed record PromptUnpublishedDomainEvent(Guid PromptId, string Slug) : DomainEvent;

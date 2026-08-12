using HelpDev.Modules.Content.Domain.Entities;

namespace HelpDev.Modules.Content.Application.Persistence;

public interface IContentWorkflowTransitionRepository
{
    Task AddAsync(ContentWorkflowTransition transition, CancellationToken cancellationToken = default);

    Task AddRangeAsync(
        IReadOnlyList<ContentWorkflowTransition> transitions,
        CancellationToken cancellationToken = default);
}

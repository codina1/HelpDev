using HelpDev.Modules.Content.Application.Persistence;
using HelpDev.Modules.Content.Domain.Entities;

namespace HelpDev.Modules.Content.Infrastructure.Persistence;

public sealed class ContentWorkflowTransitionRepository : IContentWorkflowTransitionRepository
{
    private readonly IContentDbContext _dbContext;

    public ContentWorkflowTransitionRepository(IContentDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task AddAsync(ContentWorkflowTransition transition, CancellationToken cancellationToken = default)
    {
        _dbContext.ContentWorkflowTransitions.Add(transition);
        return Task.CompletedTask;
    }

    public Task AddRangeAsync(
        IReadOnlyList<ContentWorkflowTransition> transitions,
        CancellationToken cancellationToken = default)
    {
        _dbContext.ContentWorkflowTransitions.AddRange(transitions);
        return Task.CompletedTask;
    }
}

using HelpDev.Modules.Content.Application.AiWorkflow;
using HelpDev.Modules.Content.Application.Persistence;
using HelpDev.Modules.Content.Domain.AiWorkflow;
using Microsoft.EntityFrameworkCore;

namespace HelpDev.Modules.Content.Infrastructure.Persistence;

public sealed class ContentIdeaRepository : IContentIdeaRepository
{
    private readonly IContentDbContext _dbContext;

    public ContentIdeaRepository(IContentDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<ContentIdea?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        _dbContext.ContentIdeas.FirstOrDefaultAsync(idea => idea.Id == id, cancellationToken);

    public async Task AddAsync(ContentIdea idea, CancellationToken cancellationToken = default) =>
        await _dbContext.ContentIdeas.AddAsync(idea, cancellationToken);
}

public sealed class AiContentWorkflowSessionRepository : IAiContentWorkflowSessionRepository
{
    private readonly IContentDbContext _dbContext;

    public AiContentWorkflowSessionRepository(IContentDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<AiContentWorkflowSession?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        _dbContext.AiContentWorkflowSessions.FirstOrDefaultAsync(session => session.Id == id, cancellationToken);

    public async Task AddAsync(AiContentWorkflowSession session, CancellationToken cancellationToken = default) =>
        await _dbContext.AiContentWorkflowSessions.AddAsync(session, cancellationToken);

    public async Task<IReadOnlyList<AiContentWorkflowSession>> ListByCreatorAsync(
        Guid? createdByUserId,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.AiContentWorkflowSessions.AsNoTracking().AsQueryable();
        if (createdByUserId.HasValue)
        {
            query = query.Where(session => session.CreatedByUserId == createdByUserId.Value);
        }

        return await query
            .OrderByDescending(session => session.UpdatedAtUtc)
            .Take(100)
            .ToListAsync(cancellationToken);
    }
}

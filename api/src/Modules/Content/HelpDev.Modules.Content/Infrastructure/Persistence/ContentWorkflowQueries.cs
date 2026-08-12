using HelpDev.Modules.Content.Application.Contents;
using HelpDev.Modules.Content.Application.Contents.Dtos;
using HelpDev.Modules.Content.Application.Contents.Workflow;
using HelpDev.Modules.Content.Application.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HelpDev.Modules.Content.Infrastructure.Persistence;

public sealed class ContentWorkflowQueries : IContentWorkflowQueries
{
    private readonly IContentDbContext _dbContext;

    public ContentWorkflowQueries(IContentDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<WorkflowHistoryDto> GetHistoryAsync(
        ContentManagementActor actor,
        Guid contentId,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(actor);

        var authorId = await _dbContext.Contents.AsNoTracking()
            .Where(content => content.Id == contentId)
            .Select(content => (Guid?)content.AuthorId)
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);

        if (authorId is null)
        {
            throw new ContentException("محتوا یافت نشد.", ContentErrorCodes.NotFound);
        }

        ContentService.EnsureCanManage(authorId.Value, actor);

        var items = await _dbContext.ContentWorkflowTransitions.AsNoTracking()
            .Where(row => row.ContentId == contentId)
            .OrderByDescending(row => row.CreatedAtUtc)
            .ThenByDescending(row => row.Id)
            .Select(row => new ContentWorkflowTransitionDto(
                row.Id,
                row.FromStatus.ToString(),
                row.ToStatus.ToString(),
                row.ActorUserId,
                row.Comment,
                row.CreatedAtUtc))
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return new WorkflowHistoryDto(items);
    }
}

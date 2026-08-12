using HelpDev.Modules.Content.Application.Contents;
using HelpDev.Modules.Content.Application.Persistence;
using HelpDev.Modules.Content.Application.Tools;
using HelpDev.Modules.Content.Application.Tools.Dtos;
using HelpDev.Modules.Content.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace HelpDev.Modules.Content.Infrastructure.Persistence;

public sealed class ToolQueries : IToolQueries
{
    private readonly IContentDbContext _dbContext;

    public ToolQueries(IContentDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<ToolListItemDto>> ListAsync(
        ContentManagementActor actor,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(actor);

        var query =
            from tool in _dbContext.ToolMetadata.AsNoTracking()
            join content in _dbContext.Contents.AsNoTracking()
                on tool.ContentId equals content.Id
            where content.Type == ContentType.Tool
            select new { tool, content };

        if (!actor.CanManageAllContent)
        {
            query = query.Where(x => x.content.AuthorId == actor.UserId);
        }

        var rows = await query
            .OrderByDescending(x => x.tool.UpdatedAtUtc)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return rows
            .Select(x => new ToolListItemDto(
                x.tool.Id,
                x.tool.ContentId,
                x.tool.ToolName,
                x.tool.ToolCategory,
                x.tool.PricingModel.ToString(),
                x.tool.LicenseType.ToString(),
                x.content.Slug.Value,
                x.content.Status.ToString(),
                x.tool.UpdatedAtUtc))
            .ToArray();
    }
}

using HelpDev.Modules.Content.Application.Common;
using HelpDev.Modules.Content.Application.Contents;
using HelpDev.Modules.Content.Application.Persistence;
using HelpDev.Modules.Content.Application.SeoAnalysis.Dashboard;
using HelpDev.Modules.Content.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace HelpDev.Modules.Content.Infrastructure.Persistence;

public sealed class SeoDashboardQueries : ISeoDashboardQueries
{
    private const int CriticalTake = 20;
    private const int RecentTake = 10;

    private readonly IContentDbContext _dbContext;

    public SeoDashboardQueries(IContentDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<SeoDashboardDto> GetAsync(
        ContentManagementActor actor,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(actor);

        var query = _dbContext.Contents.AsNoTracking();
        if (!actor.CanManageAllContent)
        {
            var authorId = actor.UserId;
            query = query.Where(c => c.AuthorId == authorId);
        }

        var total = await query.CountAsync(cancellationToken);
        var published = await query.CountAsync(c => c.Status == ContentStatus.Published, cancellationToken);

        var missingTitle = await query.CountAsync(
            c => c.SeoMetadata.SeoTitle == null || c.SeoMetadata.SeoTitle == string.Empty,
            cancellationToken);

        var missingDescription = await query.CountAsync(
            c => c.SeoMetadata.SeoDescription == null || c.SeoMetadata.SeoDescription == string.Empty,
            cancellationToken);

        var missingCover = await query.CountAsync(
            c => c.CoverImage == null || c.CoverImage == string.Empty,
            cancellationToken);

        var missingCanonical = await query.CountAsync(
            c => c.SeoMetadata.CanonicalUrl == null || c.SeoMetadata.CanonicalUrl == string.Empty,
            cancellationToken);

        var criticalRows = await query
            .Where(c =>
                c.SeoMetadata.SeoTitle == null
                || c.SeoMetadata.SeoTitle == string.Empty
                || c.CoverImage == null
                || c.CoverImage == string.Empty)
            .OrderByDescending(c => c.UpdatedAt)
            .Take(CriticalTake)
            .Select(c => new
            {
                c.Id,
                c.Title,
                MissingTitle = c.SeoMetadata.SeoTitle == null || c.SeoMetadata.SeoTitle == string.Empty,
                MissingCover = c.CoverImage == null || c.CoverImage == string.Empty,
            })
            .ToListAsync(cancellationToken);

        var criticalFindings = new List<SeoDashboardCriticalFindingDto>();
        foreach (var row in criticalRows)
        {
            if (row.MissingTitle)
            {
                criticalFindings.Add(new SeoDashboardCriticalFindingDto(
                    row.Id,
                    row.Title,
                    "missing_seo_title",
                    "عنوان سئو تنظیم نشده است."));
            }

            if (row.MissingCover)
            {
                criticalFindings.Add(new SeoDashboardCriticalFindingDto(
                    row.Id,
                    row.Title,
                    "missing_cover_image",
                    "تصویر کاور تنظیم نشده است."));
            }
        }

        var recentRows = await query
            .OrderByDescending(c => c.UpdatedAt)
            .Take(RecentTake)
            .Select(c => new
            {
                c.Id,
                c.Title,
                c.Status,
                c.UpdatedAt,
                MissingTitle = c.SeoMetadata.SeoTitle == null || c.SeoMetadata.SeoTitle == string.Empty,
                MissingDescription = c.SeoMetadata.SeoDescription == null || c.SeoMetadata.SeoDescription == string.Empty,
            })
            .ToListAsync(cancellationToken);

        var recent = recentRows
            .Select(r => new SeoDashboardRecentContentDto(
                r.Id,
                r.Title,
                r.Status.ToString(),
                r.UpdatedAt,
                r.MissingTitle,
                r.MissingDescription))
            .ToList();

        return new SeoDashboardDto(
            total,
            published,
            missingTitle,
            missingDescription,
            missingCover,
            missingCanonical,
            LastAnalysisTime: null,
            criticalFindings,
            recent);
    }
}

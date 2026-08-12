using HelpDev.Modules.Analytics.Application.ContentAnalytics;
using HelpDev.Modules.Content.Application.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HelpDev.Infrastructure.Analytics;

/// <summary>
/// Content editorial facts for analytics health. Lives in host Infrastructure so
/// Analytics.Application does not reference Content.Infrastructure.
/// </summary>
public sealed class ContentAnalyticsFactsSource : IContentAnalyticsFactsSource
{
    private readonly IContentDbContext _dbContext;

    public ContentAnalyticsFactsSource(IContentDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ContentAnalyticsFacts?> GetByIdAsync(
        Guid contentId,
        CancellationToken cancellationToken = default)
    {
        var row = await _dbContext.Contents.AsNoTracking()
            .Where(c => c.Id == contentId)
            .Select(c => new FactsRow
            {
                Id = c.Id,
                Title = c.Title,
                Slug = c.Slug,
                Status = c.Status.ToString(),
                UpdatedAt = c.UpdatedAt,
                MissingSeoTitle = c.SeoMetadata.SeoTitle == null || c.SeoMetadata.SeoTitle == string.Empty,
                MissingSeoDescription = c.SeoMetadata.SeoDescription == null || c.SeoMetadata.SeoDescription == string.Empty,
                MissingCover = c.CoverImage == null || c.CoverImage == string.Empty,
                MissingCanonical = c.SeoMetadata.CanonicalUrl == null || c.SeoMetadata.CanonicalUrl == string.Empty,
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (row is null)
        {
            return null;
        }

        var revisionCount = await _dbContext.ContentRevisions.AsNoTracking()
            .CountAsync(r => r.ContentId == contentId, cancellationToken);

        return Map(row, revisionCount);
    }

    public async Task<IReadOnlyList<ContentAnalyticsFacts>> ListRecentAsync(
        int take,
        CancellationToken cancellationToken = default)
    {
        take = Math.Clamp(take, 1, 100);

        var rows = await _dbContext.Contents.AsNoTracking()
            .OrderByDescending(c => c.UpdatedAt)
            .Take(take)
            .Select(c => new FactsRow
            {
                Id = c.Id,
                Title = c.Title,
                Slug = c.Slug,
                Status = c.Status.ToString(),
                UpdatedAt = c.UpdatedAt,
                MissingSeoTitle = c.SeoMetadata.SeoTitle == null || c.SeoMetadata.SeoTitle == string.Empty,
                MissingSeoDescription = c.SeoMetadata.SeoDescription == null || c.SeoMetadata.SeoDescription == string.Empty,
                MissingCover = c.CoverImage == null || c.CoverImage == string.Empty,
                MissingCanonical = c.SeoMetadata.CanonicalUrl == null || c.SeoMetadata.CanonicalUrl == string.Empty,
            })
            .ToListAsync(cancellationToken);

        if (rows.Count == 0)
        {
            return [];
        }

        var ids = rows.Select(r => r.Id).ToList();
        var revisionCounts = await _dbContext.ContentRevisions.AsNoTracking()
            .Where(r => ids.Contains(r.ContentId))
            .GroupBy(r => r.ContentId)
            .Select(g => new { ContentId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.ContentId, x => x.Count, cancellationToken);

        return rows.Select(row => Map(row, revisionCounts.GetValueOrDefault(row.Id))).ToList();
    }

    private static ContentAnalyticsFacts Map(FactsRow row, int revisionCount) =>
        new(
            row.Id,
            row.Title,
            row.Slug.Value,
            row.Status,
            row.UpdatedAt,
            revisionCount,
            row.MissingSeoTitle,
            row.MissingSeoDescription,
            row.MissingCover,
            row.MissingCanonical);

    private sealed class FactsRow
    {
        public Guid Id { get; init; }
        public string Title { get; init; } = string.Empty;
        public HelpDev.Modules.Content.Domain.ValueObjects.Slug Slug { get; init; } = null!;
        public string Status { get; init; } = string.Empty;
        public DateTime UpdatedAt { get; init; }
        public bool MissingSeoTitle { get; init; }
        public bool MissingSeoDescription { get; init; }
        public bool MissingCover { get; init; }
        public bool MissingCanonical { get; init; }
    }
}

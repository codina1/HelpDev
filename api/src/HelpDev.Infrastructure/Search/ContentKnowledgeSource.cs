using HelpDev.Modules.Content.Application.Persistence;
using HelpDev.Modules.Content.Domain.Enums;
using HelpDev.Modules.Search.Application.Contracts;
using HelpDev.Modules.Search.Domain;
using Microsoft.EntityFrameworkCore;

namespace HelpDev.Infrastructure.Search;

/// <summary>Published Content → knowledge documents. No Search domain leakage into Content.</summary>
public sealed class ContentKnowledgeSource : IContentSearchSource
{
    public const int SummaryMaxLength = 280;

    private readonly IContentDbContext _dbContext;

    public ContentKnowledgeSource(IContentDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<SearchSourceDocument?> GetByIdAsync(
        Guid contentId,
        CancellationToken cancellationToken = default)
    {
        var row = await _dbContext.Contents.AsNoTracking()
            .Where(content => content.Id == contentId)
            .Select(content => new
            {
                content.Id,
                content.Title,
                content.Slug,
                content.Body,
                content.Status,
                content.CreatedAt,
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (row is null || row.Status != ContentStatus.Published)
        {
            return null;
        }

        return Map(row.Id, row.Title, row.Slug.Value, row.Body, row.CreatedAt);
    }

    public async Task<IReadOnlyList<SearchSourceDocument>> GetPublishedBatchAsync(
        Guid? afterSourceId,
        int take,
        CancellationToken cancellationToken = default)
    {
        if (take < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(take));
        }

        var query = _dbContext.Contents.AsNoTracking()
            .Where(content => content.Status == ContentStatus.Published);

        if (afterSourceId.HasValue)
        {
            var after = afterSourceId.Value;
            query = query.Where(content => content.Id > after);
        }

        var rows = await query
            .OrderBy(content => content.Id)
            .Take(take)
            .Select(content => new
            {
                content.Id,
                content.Title,
                content.Slug,
                content.Body,
                content.CreatedAt,
            })
            .ToListAsync(cancellationToken);

        return rows
            .Select(row => Map(row.Id, row.Title, row.Slug.Value, row.Body, row.CreatedAt))
            .ToList();
    }

    private static SearchSourceDocument Map(
        Guid id,
        string title,
        string slug,
        string body,
        DateTime createdAt) =>
        new(
            id,
            KnowledgeSourceType.Content,
            title,
            slug,
            Truncate(body),
            $"/content/{slug}",
            IsPublished: true,
            PublishedAtUtc: createdAt,
            UpdatedAtUtc: createdAt,
            Body: body);

    private static string Truncate(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        var trimmed = value.Trim();
        return trimmed.Length <= SummaryMaxLength
            ? trimmed
            : trimmed[..SummaryMaxLength];
    }
}

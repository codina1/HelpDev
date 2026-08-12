using HelpDev.Modules.Search.Application.Contracts;
using HelpDev.Modules.Search.Domain;
using HelpDev.Modules.Toolbox.Application.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HelpDev.Infrastructure.Search;

/// <summary>Published + enabled tools → knowledge documents.</summary>
public sealed class ToolboxKnowledgeSource : IToolSearchSource
{
    public const int SummaryMaxLength = 280;

    private readonly IToolboxDbContext _dbContext;

    public ToolboxKnowledgeSource(IToolboxDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<SearchSourceDocument?> GetByIdAsync(
        Guid toolId,
        CancellationToken cancellationToken = default)
    {
        var row = await _dbContext.ToolDefinitions.AsNoTracking()
            .Where(tool => tool.Id == toolId && tool.IsPublished && tool.IsEnabled)
            .Select(tool => new
            {
                tool.Id,
                tool.Name,
                tool.Slug,
                tool.Summary,
                tool.Description,
                tool.UpdatedAtUtc,
                tool.PublishedAtUtc,
            })
            .FirstOrDefaultAsync(cancellationToken);

        return row is null
            ? null
            : Map(
                row.Id,
                row.Name,
                row.Slug.Value,
                row.Summary,
                row.Description,
                row.UpdatedAtUtc,
                row.PublishedAtUtc);
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

        var query = _dbContext.ToolDefinitions.AsNoTracking()
            .Where(tool => tool.IsPublished && tool.IsEnabled);

        if (afterSourceId.HasValue)
        {
            var after = afterSourceId.Value;
            query = query.Where(tool => tool.Id > after);
        }

        var rows = await query
            .OrderBy(tool => tool.Id)
            .Take(take)
            .Select(tool => new
            {
                tool.Id,
                tool.Name,
                tool.Slug,
                tool.Summary,
                tool.Description,
                tool.UpdatedAtUtc,
                tool.PublishedAtUtc,
            })
            .ToListAsync(cancellationToken);

        return rows
            .Select(row => Map(
                row.Id,
                row.Name,
                row.Slug.Value,
                row.Summary,
                row.Description,
                row.UpdatedAtUtc,
                row.PublishedAtUtc))
            .ToList();
    }

    private static SearchSourceDocument Map(
        Guid id,
        string name,
        string slug,
        string summary,
        string? description,
        DateTime updatedAtUtc,
        DateTime? publishedAtUtc)
    {
        var body = string.IsNullOrWhiteSpace(description)
            ? summary
            : $"{summary}\n\n{description}".Trim();

        return new SearchSourceDocument(
            id,
            KnowledgeSourceType.Tool,
            name,
            slug,
            Truncate(summary),
            $"/tools/{slug}",
            IsPublished: true,
            PublishedAtUtc: publishedAtUtc,
            UpdatedAtUtc: updatedAtUtc,
            Body: body);
    }

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

using HelpDev.Modules.PromptLab.Application.Persistence;
using HelpDev.Modules.Search.Application.Contracts;
using HelpDev.Modules.Search.Domain;
using Microsoft.EntityFrameworkCore;

namespace HelpDev.Infrastructure.Search;

/// <summary>Published + enabled prompts (published version template) → knowledge documents.</summary>
public sealed class PromptLabKnowledgeSource : IPromptSearchSource
{
    public const int SummaryMaxLength = 280;

    private readonly IPromptLabDbContext _dbContext;

    public PromptLabKnowledgeSource(IPromptLabDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<SearchSourceDocument?> GetByIdAsync(
        Guid promptId,
        CancellationToken cancellationToken = default)
    {
        var prompt = await _dbContext.PromptDefinitions.AsNoTracking()
            .Include(p => p.Versions)
            .Where(p => p.Id == promptId && p.IsPublished && p.IsEnabled && p.PublishedVersionNumber != null)
            .FirstOrDefaultAsync(cancellationToken);

        return prompt is null ? null : Map(prompt);
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

        var query = _dbContext.PromptDefinitions.AsNoTracking()
            .Include(p => p.Versions)
            .Where(p => p.IsPublished && p.IsEnabled && p.PublishedVersionNumber != null);

        if (afterSourceId.HasValue)
        {
            var after = afterSourceId.Value;
            query = query.Where(p => p.Id > after);
        }

        var rows = await query
            .OrderBy(p => p.Id)
            .Take(take)
            .ToListAsync(cancellationToken);

        return rows.Select(Map).Where(doc => doc is not null).Cast<SearchSourceDocument>().ToList();
    }

    private static SearchSourceDocument? Map(HelpDev.Modules.PromptLab.Domain.Prompts.PromptDefinition prompt)
    {
        if (prompt.PublishedVersionNumber is null)
        {
            return null;
        }

        var version = prompt.Versions.FirstOrDefault(v => v.VersionNumber == prompt.PublishedVersionNumber.Value);
        var template = version?.Template ?? string.Empty;
        var bodyParts = new List<string> { prompt.Summary };
        if (!string.IsNullOrWhiteSpace(prompt.Description))
        {
            bodyParts.Add(prompt.Description);
        }

        if (!string.IsNullOrWhiteSpace(template))
        {
            bodyParts.Add(template);
        }

        var body = string.Join("\n\n", bodyParts);

        return new SearchSourceDocument(
            prompt.Id,
            KnowledgeSourceType.Prompt,
            prompt.Name,
            prompt.Slug.Value,
            Truncate(prompt.Summary),
            $"/prompts/{prompt.Slug.Value}",
            IsPublished: true,
            PublishedAtUtc: prompt.PublishedAtUtc,
            UpdatedAtUtc: prompt.UpdatedAtUtc,
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

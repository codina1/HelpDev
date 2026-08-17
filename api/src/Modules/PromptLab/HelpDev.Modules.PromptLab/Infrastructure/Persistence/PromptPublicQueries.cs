using HelpDev.Modules.PromptLab.Application;
using HelpDev.Modules.PromptLab.Application.Catalog;
using HelpDev.Modules.PromptLab.Application.Persistence;
using HelpDev.Modules.PromptLab.Domain.AiModels;
using HelpDev.Modules.PromptLab.Domain.Categories;
using HelpDev.Modules.PromptLab.Domain.Prompts;
using HelpDev.Modules.PromptLab.Domain.Specifications;
using Microsoft.EntityFrameworkCore;

namespace HelpDev.Modules.PromptLab.Infrastructure.Persistence;

public sealed class PromptPublicQueries : IPromptPublicQueries
{
    private readonly IPromptLabDbContext _dbContext;

    public PromptPublicQueries(IPromptLabDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PublicPromptPageDto> GetPromptsAsync(
        PublicPromptFilter filter,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(filter);
        EnsureValidPaging(filter.Page, filter.PageSize);

        var query = CreateApprovedQuery();
        query = ApplyFilters(query, filter);

        var total = await query.CountAsync(cancellationToken);

        var ordered = filter.Popular
            ? query
                .OrderByDescending(row => row.Prompt.Views)
                .ThenByDescending(row => row.Prompt.CopyCount)
                .ThenByDescending(row => row.Prompt.PublishedAt)
                .ThenBy(row => row.Prompt.Title)
                .ThenBy(row => row.Prompt.Id)
            : query
                .OrderByDescending(row => row.Prompt.PublishedAt)
                .ThenBy(row => row.Prompt.Title)
                .ThenBy(row => row.Prompt.Id);

        var rows = await ordered
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .Select(row => new
            {
                row.Prompt.Id,
                row.Prompt.Title,
                PromptSlug = row.Prompt.Slug,
                row.Prompt.Description,
                row.Prompt.CoverImage,
                row.Prompt.MediaType,
                row.Prompt.Views,
                row.Prompt.CopyCount,
                row.Prompt.PublishedAt,
                CategoryId = row.Category.Id,
                CategoryName = row.Category.Name,
                CategorySlug = row.Category.Slug,
                AiModelId = row.AiModel.Id,
                AiModelName = row.AiModel.Name,
                AiModelSlug = row.AiModel.Slug,
                AiModelProvider = row.AiModel.Provider,
            })
            .ToListAsync(cancellationToken);

        var items = rows
            .Select(row => new PublicPromptListItemDto(
                row.Id,
                row.Title,
                row.PromptSlug.Value,
                row.Description,
                row.CoverImage,
                row.MediaType.ToString(),
                new PublicPromptCategoryRefDto(row.CategoryId, row.CategoryName, row.CategorySlug.Value),
                new PublicPromptAiModelRefDto(
                    row.AiModelId,
                    row.AiModelName,
                    row.AiModelSlug.Value,
                    row.AiModelProvider),
                row.Views,
                row.CopyCount,
                row.PublishedAt))
            .ToList();

        return new PublicPromptPageDto(filter.Page, filter.PageSize, total, items);
    }

    public async Task<PublicPromptDetailsDto?> GetBySlugAsync(
        string slug,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(slug))
        {
            return null;
        }

        var slugValue = PromptSlug.FromPersisted(slug.Trim().ToLowerInvariant());

        var row = await CreateApprovedQuery()
            .Where(item => item.Prompt.Slug == slugValue)
            .Select(item => new
            {
                item.Prompt.Id,
                item.Prompt.Title,
                PromptSlug = item.Prompt.Slug,
                item.Prompt.Description,
                item.Prompt.Content,
                item.Prompt.CoverImage,
                item.Prompt.MediaType,
                item.Prompt.Views,
                item.Prompt.CopyCount,
                item.Prompt.PublishedAt,
                CategoryId = item.Category.Id,
                CategoryName = item.Category.Name,
                CategorySlug = item.Category.Slug,
                AiModelId = item.AiModel.Id,
                AiModelName = item.AiModel.Name,
                AiModelSlug = item.AiModel.Slug,
                AiModelProvider = item.AiModel.Provider,
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (row is null)
        {
            return null;
        }

        return new PublicPromptDetailsDto(
            row.Id,
            row.Title,
            row.PromptSlug.Value,
            row.Description,
            row.Content,
            row.CoverImage,
            row.MediaType.ToString(),
            new PublicPromptCategoryRefDto(row.CategoryId, row.CategoryName, row.CategorySlug.Value),
            new PublicPromptAiModelRefDto(
                row.AiModelId,
                row.AiModelName,
                row.AiModelSlug.Value,
                row.AiModelProvider),
            row.Views,
            row.CopyCount,
            row.PublishedAt);
    }

    private IQueryable<ApprovedPromptRow> CreateApprovedQuery()
    {
        var approvedOnly = new PublicPromptSpecification().Criteria
            ?? throw new InvalidOperationException("Public prompt specification must filter by approved status.");

        return
            from prompt in _dbContext.Prompts.AsNoTracking().Where(approvedOnly)
            join category in _dbContext.PromptCategories.AsNoTracking()
                on prompt.CategoryId equals category.Id
            join aiModel in _dbContext.AiModels.AsNoTracking()
                on prompt.AiModelId equals aiModel.Id
            where category.IsActive && aiModel.IsActive
            select new ApprovedPromptRow
            {
                Prompt = prompt,
                Category = category,
                AiModel = aiModel,
            };
    }

    private static IQueryable<ApprovedPromptRow> ApplyFilters(
        IQueryable<ApprovedPromptRow> query,
        PublicPromptFilter filter)
    {
        if (!string.IsNullOrWhiteSpace(filter.Category))
        {
            var categorySlug = PromptSlug.FromPersisted(filter.Category.Trim().ToLowerInvariant());
            query = query.Where(row => row.Category.Slug == categorySlug);
        }

        if (!string.IsNullOrWhiteSpace(filter.AiModel))
        {
            var aiModelSlug = PromptSlug.FromPersisted(filter.AiModel.Trim().ToLowerInvariant());
            query = query.Where(row => row.AiModel.Slug == aiModelSlug);
        }

        if (PublicPromptMediaTypes.TryParse(filter.MediaType, out var mediaType))
        {
            query = query.Where(row => row.Prompt.MediaType == mediaType);
        }

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var term = filter.Search.Trim().ToLowerInvariant();
            query = query.Where(row =>
                row.Prompt.Title.ToLower().Contains(term)
                || (row.Prompt.Description != null && row.Prompt.Description.ToLower().Contains(term)));
        }

        return query;
    }

    private static void EnsureValidPaging(int page, int pageSize)
    {
        if (page < 1 || pageSize < 1 || pageSize > PromptLabPaging.MaxPageSize)
        {
            throw new PromptLabException(
                $"Page must be >= 1 and pageSize must be between 1 and {PromptLabPaging.MaxPageSize}.",
                PromptLabApplicationErrorCodes.PaginationInvalid);
        }
    }

    private sealed class ApprovedPromptRow
    {
        public required Prompt Prompt { get; init; }

        public required PromptCategory Category { get; init; }

        public required AiModel AiModel { get; init; }
    }
}

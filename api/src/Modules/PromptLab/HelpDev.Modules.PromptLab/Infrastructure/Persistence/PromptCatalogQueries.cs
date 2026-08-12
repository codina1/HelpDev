using HelpDev.Modules.PromptLab.Application;
using HelpDev.Modules.PromptLab.Application.Catalog;
using HelpDev.Modules.PromptLab.Application.Persistence;
using HelpDev.Modules.PromptLab.Domain.Prompts;
using Microsoft.EntityFrameworkCore;

namespace HelpDev.Modules.PromptLab.Infrastructure.Persistence;

public sealed class PromptCatalogQueries : IPromptCatalogQueries
{
    private readonly IPromptLabDbContext _dbContext;

    public PromptCatalogQueries(IPromptLabDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<PromptCategoryDto>> GetCategoriesAsync(
        CancellationToken cancellationToken = default)
    {
        var rows = await _dbContext.PromptCategories
            .AsNoTracking()
            .Where(category => category.IsActive)
            .OrderBy(category => category.DisplayOrder)
            .ThenBy(category => category.Name)
            .ThenBy(category => category.Id)
            .Select(category => new
            {
                category.Id,
                category.Name,
                category.Slug,
                category.Description,
                category.Icon,
                category.DisplayOrder,
            })
            .ToListAsync(cancellationToken);

        return rows
            .Select(row => new PromptCategoryDto(
                row.Id,
                row.Name,
                row.Slug.Value,
                row.Description,
                row.Icon,
                row.DisplayOrder))
            .ToList();
    }

    public async Task<PromptCatalogPageDto> GetPromptsAsync(
        PromptCatalogFilter filter,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(filter);
        EnsureValidPaging(filter.Page, filter.PageSize);

        var query =
            from prompt in _dbContext.PromptDefinitions.AsNoTracking()
            join category in _dbContext.PromptCategories.AsNoTracking()
                on prompt.CategoryId equals category.Id
            where category.IsActive
                && prompt.IsPublished
                && prompt.IsEnabled
            select new { prompt, category };

        if (!string.IsNullOrWhiteSpace(filter.CategorySlug))
        {
            var categorySlug = PromptSlug.FromPersisted(filter.CategorySlug.Trim().ToLowerInvariant());
            query = query.Where(row => row.category.Slug == categorySlug);
        }

        if (!string.IsNullOrWhiteSpace(filter.Purpose)
            && Enum.TryParse<PromptPurpose>(filter.Purpose.Trim(), ignoreCase: true, out var purpose)
            && Enum.IsDefined(purpose))
        {
            query = query.Where(row => row.prompt.Purpose == purpose);
        }

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var term = filter.Search.Trim().ToLowerInvariant();
            query = query.Where(row =>
                row.prompt.Name.ToLower().Contains(term)
                || row.prompt.Summary.ToLower().Contains(term));
        }

        var total = await query.CountAsync(cancellationToken);

        var rows = await query
            .OrderBy(row => row.category.DisplayOrder)
            .ThenBy(row => row.prompt.DisplayOrder)
            .ThenBy(row => row.prompt.Name)
            .ThenBy(row => row.prompt.Id)
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .Select(row => new
            {
                row.prompt.Id,
                PromptSlug = row.prompt.Slug,
                row.prompt.Name,
                row.prompt.Summary,
                CategoryId = row.category.Id,
                CategoryName = row.category.Name,
                row.prompt.Purpose,
                row.prompt.Visibility,
                row.prompt.RequiresAuthentication,
                row.prompt.DisplayOrder,
                row.prompt.PublishedVersionNumber,
            })
            .ToListAsync(cancellationToken);

        var items = rows
            .Select(row => new PromptCatalogItemDto(
                row.Id,
                row.PromptSlug.Value,
                row.Name,
                row.Summary,
                row.CategoryId,
                row.CategoryName,
                row.Purpose.ToString(),
                row.Visibility.ToString(),
                row.RequiresAuthentication,
                row.DisplayOrder,
                row.PublishedVersionNumber))
            .ToList();

        return new PromptCatalogPageDto(filter.Page, filter.PageSize, total, items);
    }

    public async Task<PromptDetailsDto?> GetBySlugAsync(
        string slug,
        Guid? currentUserId = null,
        CancellationToken cancellationToken = default)
    {
        var slugValue = PromptSlug.FromPersisted(slug.Trim().ToLowerInvariant());

        var prompt = await _dbContext.PromptDefinitions
            .AsNoTracking()
            .Include(definition => definition.Versions)
            .ThenInclude(version => version.Variables)
            .FirstOrDefaultAsync(definition => definition.Slug == slugValue, cancellationToken);

        if (prompt is null || !prompt.IsPublished || !prompt.IsEnabled || prompt.PublishedVersionNumber is null)
        {
            return null;
        }

        var category = await _dbContext.PromptCategories
            .AsNoTracking()
            .FirstOrDefaultAsync(item => item.Id == prompt.CategoryId, cancellationToken);

        if (category is null || !category.IsActive)
        {
            return null;
        }

        if (RequiresAuthentication(prompt) && currentUserId is null)
        {
            throw new PromptLabException(
                "Prompt requires authentication.",
                PromptLabApplicationErrorCodes.RenderRequiresAuthentication);
        }

        var version = prompt.Versions.FirstOrDefault(item => item.VersionNumber == prompt.PublishedVersionNumber);
        if (version is null)
        {
            return null;
        }

        var variables = version.Variables
            .OrderBy(variable => variable.DisplayOrder)
            .ThenBy(variable => variable.Name, StringComparer.OrdinalIgnoreCase)
            .Select(MapVariable)
            .ToList();

        return new PromptDetailsDto(
            prompt.Id,
            prompt.Slug.Value,
            prompt.Name,
            prompt.Summary,
            prompt.Description,
            prompt.Purpose.ToString(),
            prompt.Visibility.ToString(),
            prompt.RequiresAuthentication,
            prompt.AllowHistory,
            prompt.DisplayOrder,
            prompt.PublishedVersionNumber.Value,
            version.Template,
            variables,
            new PromptDetailsCategoryDto(
                category.Id,
                category.Name,
                category.Slug.Value,
                category.Icon));
    }

    private static bool RequiresAuthentication(PromptDefinition prompt) =>
        prompt.Visibility == PromptVisibility.Authenticated || prompt.RequiresAuthentication;

    private static PromptVariableDto MapVariable(PromptVariable variable) =>
        new(
            variable.Name,
            variable.Label,
            variable.Description,
            variable.Type.ToString(),
            variable.IsRequired,
            variable.DefaultValue,
            variable.MinLength,
            variable.MaxLength,
            variable.MinValue,
            variable.MaxValue,
            variable.ValidationPattern,
            variable.AllowedValues.ToList(),
            variable.DisplayOrder);

    private static void EnsureValidPaging(int page, int pageSize)
    {
        if (page < 1 || pageSize < 1 || pageSize > PromptLabPaging.MaxPageSize)
        {
            throw new PromptLabException(
                $"Page must be >= 1 and pageSize must be between 1 and {PromptLabPaging.MaxPageSize}.",
                PromptLabApplicationErrorCodes.PaginationInvalid);
        }
    }
}

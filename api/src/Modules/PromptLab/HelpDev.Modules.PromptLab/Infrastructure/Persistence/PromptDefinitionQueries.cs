using HelpDev.Modules.PromptLab.Application;
using HelpDev.Modules.PromptLab.Application.Catalog;
using HelpDev.Modules.PromptLab.Application.Persistence;
using HelpDev.Modules.PromptLab.Application.Prompts;
using HelpDev.Modules.PromptLab.Domain.Prompts;
using Microsoft.EntityFrameworkCore;

namespace HelpDev.Modules.PromptLab.Infrastructure.Persistence;

public sealed class PromptDefinitionQueries : IPromptDefinitionQueries
{
    private readonly IPromptLabDbContext _dbContext;

    public PromptDefinitionQueries(IPromptLabDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PromptDefinitionAdminDto?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var row = await _dbContext.PromptDefinitions
            .AsNoTracking()
            .Where(prompt => prompt.Id == id)
            .Select(prompt => new PromptDefinitionRow(
                prompt.Id,
                prompt.CategoryId,
                prompt.Name,
                prompt.Slug,
                prompt.Summary,
                prompt.Description,
                prompt.Purpose,
                prompt.Visibility,
                prompt.IsPublished,
                prompt.IsEnabled,
                prompt.RequiresAuthentication,
                prompt.AllowHistory,
                prompt.DisplayOrder,
                prompt.LatestVersionNumber,
                prompt.PublishedVersionNumber,
                prompt.CreatedAtUtc,
                prompt.UpdatedAtUtc,
                prompt.PublishedAtUtc))
            .FirstOrDefaultAsync(cancellationToken);

        return row is null ? null : ToAdminDto(row);
    }

    public async Task<PromptDefinitionPageDto> GetPageAsync(
        PromptDefinitionFilter filter,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(filter);

        if (filter.Page < 1 || filter.PageSize < 1 || filter.PageSize > PromptLabPaging.MaxPageSize)
        {
            throw new PromptLabException(
                $"Page must be >= 1 and pageSize must be between 1 and {PromptLabPaging.MaxPageSize}.",
                PromptLabApplicationErrorCodes.PaginationInvalid);
        }

        var query = _dbContext.PromptDefinitions.AsNoTracking().AsQueryable();

        if (filter.CategoryId.HasValue)
        {
            query = query.Where(prompt => prompt.CategoryId == filter.CategoryId.Value);
        }

        if (!string.IsNullOrWhiteSpace(filter.Purpose)
            && Enum.TryParse<PromptPurpose>(filter.Purpose.Trim(), ignoreCase: true, out var purpose)
            && Enum.IsDefined(purpose))
        {
            query = query.Where(prompt => prompt.Purpose == purpose);
        }

        if (!string.IsNullOrWhiteSpace(filter.Visibility)
            && Enum.TryParse<PromptVisibility>(filter.Visibility.Trim(), ignoreCase: true, out var visibility)
            && Enum.IsDefined(visibility))
        {
            query = query.Where(prompt => prompt.Visibility == visibility);
        }

        if (filter.IsPublished.HasValue)
        {
            query = query.Where(prompt => prompt.IsPublished == filter.IsPublished.Value);
        }

        if (filter.IsEnabled.HasValue)
        {
            query = query.Where(prompt => prompt.IsEnabled == filter.IsEnabled.Value);
        }

        var total = await query.CountAsync(cancellationToken);

        var rows = await query
            .OrderBy(prompt => prompt.DisplayOrder)
            .ThenBy(prompt => prompt.Name)
            .ThenBy(prompt => prompt.Id)
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .Select(prompt => new PromptDefinitionRow(
                prompt.Id,
                prompt.CategoryId,
                prompt.Name,
                prompt.Slug,
                prompt.Summary,
                prompt.Description,
                prompt.Purpose,
                prompt.Visibility,
                prompt.IsPublished,
                prompt.IsEnabled,
                prompt.RequiresAuthentication,
                prompt.AllowHistory,
                prompt.DisplayOrder,
                prompt.LatestVersionNumber,
                prompt.PublishedVersionNumber,
                prompt.CreatedAtUtc,
                prompt.UpdatedAtUtc,
                prompt.PublishedAtUtc))
            .ToListAsync(cancellationToken);

        var items = rows.Select(ToAdminDto).ToList();
        return new PromptDefinitionPageDto(filter.Page, filter.PageSize, total, items);
    }

    public async Task<IReadOnlyList<PromptVersionAdminDto>> GetVersionsAsync(
        Guid promptId,
        CancellationToken cancellationToken = default)
    {
        var prompt = await _dbContext.PromptDefinitions
            .AsNoTracking()
            .Include(definition => definition.Versions)
            .ThenInclude(version => version.Variables)
            .FirstOrDefaultAsync(definition => definition.Id == promptId, cancellationToken);

        if (prompt is null)
        {
            return Array.Empty<PromptVersionAdminDto>();
        }

        return prompt.Versions
            .OrderByDescending(version => version.VersionNumber)
            .Select(ToVersionDto)
            .ToList();
    }

    public async Task<PromptVersionAdminDto?> GetVersionAsync(
        Guid promptId,
        int versionNumber,
        CancellationToken cancellationToken = default)
    {
        var prompt = await _dbContext.PromptDefinitions
            .AsNoTracking()
            .Include(definition => definition.Versions)
            .ThenInclude(version => version.Variables)
            .FirstOrDefaultAsync(definition => definition.Id == promptId, cancellationToken);

        var version = prompt?.Versions.FirstOrDefault(item => item.VersionNumber == versionNumber);
        return version is null ? null : ToVersionDto(version);
    }

    private static PromptDefinitionAdminDto ToAdminDto(PromptDefinitionRow row) =>
        new(
            row.Id,
            row.CategoryId,
            row.Name,
            row.Slug.Value,
            row.Summary,
            row.Description,
            row.Purpose.ToString(),
            row.Visibility.ToString(),
            row.IsPublished,
            row.IsEnabled,
            row.RequiresAuthentication,
            row.AllowHistory,
            row.DisplayOrder,
            row.LatestVersionNumber,
            row.PublishedVersionNumber,
            row.CreatedAtUtc,
            row.UpdatedAtUtc,
            row.PublishedAtUtc);

    private static PromptVersionAdminDto ToVersionDto(PromptVersion version) =>
        new(
            version.Id,
            version.VersionNumber,
            version.Template,
            version.ChangeNotes,
            version.CreatedByUserId,
            version.CreatedAtUtc,
            version.Variables
                .OrderBy(variable => variable.DisplayOrder)
                .ThenBy(variable => variable.Name, StringComparer.OrdinalIgnoreCase)
                .Select(variable => new PromptVariableDto(
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
                    variable.DisplayOrder))
                .ToList());

    private sealed record PromptDefinitionRow(
        Guid Id,
        Guid CategoryId,
        string Name,
        PromptSlug Slug,
        string Summary,
        string? Description,
        PromptPurpose Purpose,
        PromptVisibility Visibility,
        bool IsPublished,
        bool IsEnabled,
        bool RequiresAuthentication,
        bool AllowHistory,
        int DisplayOrder,
        int LatestVersionNumber,
        int? PublishedVersionNumber,
        DateTime CreatedAtUtc,
        DateTime UpdatedAtUtc,
        DateTime? PublishedAtUtc);
}

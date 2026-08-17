using HelpDev.Modules.PromptLab.Application;
using HelpDev.Modules.PromptLab.Application.Catalog;
using HelpDev.Modules.PromptLab.Application.Persistence;
using HelpDev.Modules.PromptLab.Application.Prompts;
using HelpDev.Modules.PromptLab.Domain.Prompts;
using Microsoft.EntityFrameworkCore;

namespace HelpDev.Modules.PromptLab.Infrastructure.Persistence;

public sealed class PromptAdminReviewQueries : IPromptAdminReviewQueries
{
    private readonly IPromptLabDbContext _dbContext;

    public PromptAdminReviewQueries(IPromptLabDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<AdminPromptReviewPageDto> GetPromptsAsync(
        AdminPromptReviewFilter filter,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(filter);
        EnsureValidPaging(filter.Page, filter.PageSize);
        if (!AdminPromptReviewStatuses.TryParse(filter.Status, out var status))
        {
            throw new PromptLabException(
                "Review status is invalid.",
                PromptLabApplicationErrorCodes.PromptStatusInvalid);
        }

        var query =
            from prompt in _dbContext.Prompts.AsNoTracking()
            join category in _dbContext.PromptCategories.AsNoTracking()
                on prompt.CategoryId equals category.Id
            where prompt.Status == status
            select new { prompt, category };

        var total = await query.CountAsync(cancellationToken);

        var rows = await query
            .OrderByDescending(row => row.prompt.UpdatedAt)
            .ThenBy(row => row.prompt.Title)
            .ThenBy(row => row.prompt.Id)
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .Select(row => new
            {
                row.prompt.Id,
                row.prompt.Title,
                PromptSlug = row.prompt.Slug,
                row.prompt.AuthorId,
                row.prompt.CategoryId,
                CategoryName = row.category.Name,
                row.prompt.Content,
                row.prompt.Status,
                row.prompt.RejectionReason,
                row.prompt.CreatedAt,
                row.prompt.UpdatedAt,
                row.prompt.PublishedAt,
            })
            .ToListAsync(cancellationToken);

        var items = rows
            .Select(row => new AdminPromptReviewListItemDto(
                row.Id,
                row.Title,
                row.PromptSlug.Value,
                row.AuthorId,
                row.CategoryId,
                row.CategoryName,
                AdminPromptReviewStatuses.Preview(row.Content),
                row.Status.ToString(),
                row.RejectionReason,
                row.CreatedAt,
                row.UpdatedAt,
                row.PublishedAt))
            .ToList();

        return new AdminPromptReviewPageDto(filter.Page, filter.PageSize, total, items);
    }

    public async Task<AdminPromptReviewDetailsDto?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var row = await (
            from prompt in _dbContext.Prompts.AsNoTracking()
            join category in _dbContext.PromptCategories.AsNoTracking()
                on prompt.CategoryId equals category.Id
            where prompt.Id == id
            select new
            {
                prompt.Id,
                prompt.Title,
                PromptSlug = prompt.Slug,
                prompt.Description,
                prompt.Content,
                prompt.CoverImage,
                prompt.MediaType,
                prompt.AuthorId,
                prompt.CategoryId,
                CategoryName = category.Name,
                prompt.AiModelId,
                prompt.Status,
                prompt.RejectionReason,
                prompt.Views,
                prompt.CopyCount,
                prompt.CreatedAt,
                prompt.UpdatedAt,
                prompt.PublishedAt,
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (row is null)
        {
            return null;
        }

        return new AdminPromptReviewDetailsDto(
            row.Id,
            row.Title,
            row.PromptSlug.Value,
            row.Description,
            row.Content,
            row.CoverImage,
            row.MediaType.ToString(),
            row.AuthorId,
            row.CategoryId,
            row.CategoryName,
            row.AiModelId,
            row.Status.ToString(),
            row.RejectionReason,
            row.Views,
            row.CopyCount,
            row.CreatedAt,
            row.UpdatedAt,
            row.PublishedAt);
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
}

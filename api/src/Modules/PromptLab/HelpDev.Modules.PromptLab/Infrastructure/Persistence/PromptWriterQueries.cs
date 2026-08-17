using HelpDev.Modules.PromptLab.Application;
using HelpDev.Modules.PromptLab.Application.Catalog;
using HelpDev.Modules.PromptLab.Application.Persistence;
using HelpDev.Modules.PromptLab.Application.Prompts;
using HelpDev.Modules.PromptLab.Domain.Prompts;
using Microsoft.EntityFrameworkCore;

namespace HelpDev.Modules.PromptLab.Infrastructure.Persistence;

public sealed class PromptWriterQueries : IPromptWriterQueries
{
    private readonly IPromptLabDbContext _dbContext;

    public PromptWriterQueries(IPromptLabDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<WriterPromptPageDto> GetMyPromptsAsync(
        Guid authorId,
        WriterPromptFilter filter,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(filter);
        EnsureAuthor(authorId);
        EnsureValidPaging(filter.Page, filter.PageSize);

        var query = _dbContext.Prompts
            .AsNoTracking()
            .Where(prompt => prompt.AuthorId == authorId);

        if (TryParseStatus(filter.Status, out var status))
        {
            query = query.Where(prompt => prompt.Status == status);
        }

        var total = await query.CountAsync(cancellationToken);

        var rows = await query
            .OrderByDescending(prompt => prompt.UpdatedAt)
            .ThenBy(prompt => prompt.Title)
            .ThenBy(prompt => prompt.Id)
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .Select(prompt => new
            {
                prompt.Id,
                prompt.Title,
                PromptSlug = prompt.Slug,
                prompt.Description,
                prompt.CoverImage,
                prompt.MediaType,
                prompt.CategoryId,
                prompt.AiModelId,
                prompt.Status,
                prompt.Views,
                prompt.CopyCount,
                prompt.CreatedAt,
                prompt.UpdatedAt,
                prompt.PublishedAt,
            })
            .ToListAsync(cancellationToken);

        var items = rows
            .Select(row => new WriterPromptListItemDto(
                row.Id,
                row.Title,
                row.PromptSlug.Value,
                row.Description,
                row.CoverImage,
                row.MediaType.ToString(),
                row.CategoryId,
                row.AiModelId,
                row.Status.ToString(),
                row.Views,
                row.CopyCount,
                row.CreatedAt,
                row.UpdatedAt,
                row.PublishedAt))
            .ToList();

        return new WriterPromptPageDto(filter.Page, filter.PageSize, total, items);
    }

    public async Task<WriterPromptDetailsDto?> GetMyByIdAsync(
        Guid authorId,
        Guid id,
        CancellationToken cancellationToken = default)
    {
        EnsureAuthor(authorId);

        var row = await _dbContext.Prompts
            .AsNoTracking()
            .Where(prompt => prompt.Id == id && prompt.AuthorId == authorId)
            .Select(prompt => new
            {
                prompt.Id,
                prompt.Title,
                PromptSlug = prompt.Slug,
                prompt.Description,
                prompt.Content,
                prompt.CoverImage,
                prompt.MediaType,
                prompt.CategoryId,
                prompt.AiModelId,
                prompt.Status,
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

        return new WriterPromptDetailsDto(
            row.Id,
            row.Title,
            row.PromptSlug.Value,
            row.Description,
            row.Content,
            row.CoverImage,
            row.MediaType.ToString(),
            row.CategoryId,
            row.AiModelId,
            row.Status.ToString(),
            row.Views,
            row.CopyCount,
            row.CreatedAt,
            row.UpdatedAt,
            row.PublishedAt);
    }

    private static bool TryParseStatus(string? value, out PromptStatus status)
    {
        status = default;
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        return Enum.TryParse(value.Trim(), ignoreCase: true, out status) && Enum.IsDefined(status);
    }

    private static void EnsureAuthor(Guid authorId)
    {
        if (authorId == Guid.Empty)
        {
            throw new PromptLabException(
                "Author id is required.",
                PromptLabApplicationErrorCodes.PromptAuthorInvalid);
        }
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

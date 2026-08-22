using HelpDev.Modules.Content.Application.Common;
using HelpDev.Modules.Content.Application.Contents;
using HelpDev.Modules.Content.Application.Contents.Dtos;
using HelpDev.Modules.Content.Application.Persistence;
using HelpDev.Modules.Content.Domain.Enums;
using HelpDev.Modules.Content.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using ContentEntity = HelpDev.Modules.Content.Domain.Entities.Content;

namespace HelpDev.Modules.Content.Infrastructure.Persistence;

/// <summary>
/// Read-only admin content listing. Filtering, ordering and pagination run in SQL
/// (AsNoTracking + projection); the aggregate never escapes as IQueryable.
/// </summary>
public sealed class AdminContentQueries : IAdminContentQueries
{
    private readonly IContentDbContext _dbContext;

    public AdminContentQueries(IContentDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PagedResult<AdminContentListItemDto>> ListAsync(
        ContentSearchFilter filter,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(filter);

        var query = _dbContext.Contents.AsNoTracking();

        if (filter.AuthorId.HasValue)
        {
            var authorId = filter.AuthorId.Value;
            query = query.Where(content => content.AuthorId == authorId);
        }

        if (filter.Status is not null)
        {
            var status = ParseStatusOrThrow(filter.Status);
            query = query.Where(content => content.Status == status);
        }

        if (filter.Type is not null)
        {
            var type = ParseTypeOrThrow(filter.Type);
            query = query.Where(content => content.Type == type);
        }

        if (filter.Search is not null)
        {
            // Provider-agnostic case-insensitive contains: lower(title) LIKE %term%.
            var pattern = $"%{EscapeLike(filter.Search.ToLowerInvariant())}%";

            // Slug is a value-converted column, so equality is used for slug matching while
            // the free-text title search relies on a lowercased LIKE.
            if (Slug.TryCreate(filter.Search, out var searchSlug) && searchSlug is not null)
            {
                query = query.Where(content =>
                    EF.Functions.Like(content.Title.ToLower(), pattern) || content.Slug == searchSlug);
            }
            else
            {
                query = query.Where(content => EF.Functions.Like(content.Title.ToLower(), pattern));
            }
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var rows = await query
            .OrderByDescending(content => content.UpdatedAt)
            .ThenBy(content => content.Id)
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .Select(content => new Row
            {
                Id = content.Id,
                Title = content.Title,
                Slug = content.Slug,
                Type = content.Type,
                Status = content.Status,
                AuthorId = content.AuthorId,
                CreatedAt = content.CreatedAt,
                UpdatedAt = content.UpdatedAt,
                PublishedAtUtc = content.PublishedAtUtc,
            })
            .ToListAsync(cancellationToken);

        var items = rows
            .Select(row => new AdminContentListItemDto(
                row.Id,
                row.Title,
                row.Slug.Value,
                row.Type.ToString(),
                row.Status.ToString(),
                row.AuthorId,
                row.CreatedAt,
                row.UpdatedAt,
                row.PublishedAtUtc))
            .ToList();

        return new PagedResult<AdminContentListItemDto>(items, filter.Page, filter.PageSize, totalCount);
    }

    public async Task<AdminContentDetailDto?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var row = await ProjectDetail(_dbContext.Contents.AsNoTracking().Where(content => content.Id == id))
            .FirstOrDefaultAsync(cancellationToken);

        return row is null ? null : MapDetail(row);
    }

    public async Task<AdminContentDetailDto?> GetBySlugAsync(
        string slug,
        CancellationToken cancellationToken = default)
    {
        if (!Slug.TryCreate(slug, out var normalizedSlug) || normalizedSlug is null)
        {
            return null;
        }

        var row = await ProjectDetail(
                _dbContext.Contents.AsNoTracking().Where(content => content.Slug == normalizedSlug))
            .FirstOrDefaultAsync(cancellationToken);

        return row is null ? null : MapDetail(row);
    }

    // Projection runs in SQL (value objects/enums are materialized then mapped in memory,
    // mirroring the list query). No aggregate is tracked and no IQueryable escapes.
    private static IQueryable<DetailRow> ProjectDetail(IQueryable<ContentEntity> source) =>
        source.Select(content => new DetailRow
        {
            Id = content.Id,
            Title = content.Title,
            Slug = content.Slug,
            Body = content.Body,
            Excerpt = content.Excerpt,
            CoverImage = content.CoverImage,
            Type = content.Type,
            Status = content.Status,
            AuthorId = content.AuthorId,
            Views = content.Views,
            Saves = content.Saves,
            CreatedAt = content.CreatedAt,
            UpdatedAt = content.UpdatedAt,
            PublishedAtUtc = content.PublishedAtUtc,
            SeoTitle = content.SeoMetadata.SeoTitle,
            SeoDescription = content.SeoMetadata.SeoDescription,
            CanonicalUrl = content.SeoMetadata.CanonicalUrl,
            OgImage = content.SeoMetadata.OgImage,
            FocusKeyword = content.SeoMetadata.FocusKeyword,
            ContentJson = content.ContentJson,
            ContentHtml = content.ContentHtml,
            ContentFormat = content.ContentFormat,
            EditorVersion = content.EditorVersion,
            WordCount = content.WordCount,
            ReadingTimeMinutes = content.ReadingTimeMinutes,
            LastAutosavedAtUtc = content.LastAutosavedAtUtc,
        });

    private static AdminContentDetailDto MapDetail(DetailRow row) =>
        new(
            row.Id,
            row.Title,
            row.Slug.Value,
            row.Body,
            row.Excerpt,
            row.CoverImage,
            row.Type.ToString(),
            row.Status.ToString(),
            row.AuthorId,
            row.Views,
            row.Saves,
            row.CreatedAt,
            row.UpdatedAt,
            row.PublishedAtUtc,
            new SeoMetadataDto(
                row.SeoTitle,
                row.SeoDescription,
                row.CanonicalUrl,
                row.OgImage,
                row.FocusKeyword),
            row.ContentJson,
            row.ContentHtml,
            row.ContentFormat,
            row.EditorVersion,
            row.WordCount,
            row.ReadingTimeMinutes,
            row.LastAutosavedAtUtc);

    private static ContentStatus ParseStatusOrThrow(string status)
    {
        if (!Enum.TryParse<ContentStatus>(status, ignoreCase: true, out var parsed))
        {
            throw new ContentException("وضعیت فیلتر معتبر نیست.", ContentErrorCodes.Validation);
        }

        return parsed;
    }

    private static ContentType ParseTypeOrThrow(string type)
    {
        if (!Enum.TryParse<ContentType>(type, ignoreCase: true, out var parsed))
        {
            throw new ContentException("نوع فیلتر معتبر نیست.", ContentErrorCodes.Validation);
        }

        return parsed;
    }

    private static string EscapeLike(string value) =>
        value
            .Replace("\\", "\\\\", StringComparison.Ordinal)
            .Replace("%", "\\%", StringComparison.Ordinal)
            .Replace("_", "\\_", StringComparison.Ordinal);

    private sealed class Row
    {
        public Guid Id { get; init; }

        public string Title { get; init; } = string.Empty;

        public Slug Slug { get; init; } = null!;

        public ContentType Type { get; init; }

        public ContentStatus Status { get; init; }

        public Guid AuthorId { get; init; }

        public DateTime CreatedAt { get; init; }

        public DateTime UpdatedAt { get; init; }

        public DateTime? PublishedAtUtc { get; init; }
    }

    private sealed class DetailRow
    {
        public Guid Id { get; init; }

        public string Title { get; init; } = string.Empty;

        public Slug Slug { get; init; } = null!;

        public string Body { get; init; } = string.Empty;

        public string Excerpt { get; init; } = string.Empty;

        public string? CoverImage { get; init; }

        public ContentType Type { get; init; }

        public ContentStatus Status { get; init; }

        public Guid AuthorId { get; init; }

        public int Views { get; init; }

        public int Saves { get; init; }

        public DateTime CreatedAt { get; init; }

        public DateTime UpdatedAt { get; init; }

        public DateTime? PublishedAtUtc { get; init; }

        public string? SeoTitle { get; init; }

        public string? SeoDescription { get; init; }

        public string? CanonicalUrl { get; init; }

        public string? OgImage { get; init; }

        public string? FocusKeyword { get; init; }

        public string? ContentJson { get; init; }

        public string? ContentHtml { get; init; }

        public string? ContentFormat { get; init; }

        public string? EditorVersion { get; init; }

        public int? WordCount { get; init; }

        public int? ReadingTimeMinutes { get; init; }

        public DateTime? LastAutosavedAtUtc { get; init; }
    }
}

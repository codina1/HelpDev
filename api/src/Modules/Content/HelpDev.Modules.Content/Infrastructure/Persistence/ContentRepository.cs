using HelpDev.Modules.Content.Application.Persistence;
using HelpDev.Modules.Content.Domain.Enums;
using HelpDev.Modules.Content.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using ContentEntity = HelpDev.Modules.Content.Domain.Entities.Content;

namespace HelpDev.Modules.Content.Infrastructure.Persistence;

public sealed class ContentRepository : IContentRepository
{
    private readonly IContentDbContext _dbContext;

    public ContentRepository(IContentDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<ContentEntity>> GetPublishedAsync(CancellationToken cancellationToken = default) =>
        await _dbContext.Contents
            .AsNoTracking()
            .Where(content => content.Status == ContentStatus.Published)
            .OrderByDescending(content => content.CreatedAt)
            .ToListAsync(cancellationToken);

    public Task<ContentEntity?> GetPublishedBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        var slugValue = Slug.FromPersisted(slug);
        return _dbContext.Contents
            .AsNoTracking()
            .FirstOrDefaultAsync(
                content => content.Slug == slugValue && content.Status == ContentStatus.Published,
                cancellationToken);
    }

    public Task<ContentEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        _dbContext.Contents.FirstOrDefaultAsync(content => content.Id == id, cancellationToken);

    public Task<bool> SlugExistsAsync(string slug, CancellationToken cancellationToken = default)
    {
        var slugValue = Slug.FromPersisted(slug);
        return _dbContext.Contents.AnyAsync(content => content.Slug == slugValue, cancellationToken);
    }

    public Task<bool> SlugExistsForOtherAsync(string slug, Guid excludingContentId, CancellationToken cancellationToken = default)
    {
        var slugValue = Slug.FromPersisted(slug);
        return _dbContext.Contents.AnyAsync(
            content => content.Slug == slugValue && content.Id != excludingContentId,
            cancellationToken);
    }

    public async Task<ContentEntity> AddAsync(ContentEntity content, CancellationToken cancellationToken = default)
    {
        _dbContext.Contents.Add(content);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return content;
    }
}
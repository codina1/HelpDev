using HelpDev.Modules.Administration.Application;
using HelpDev.Modules.Administration.Application.Announcements;
using HelpDev.Modules.Administration.Application.Persistence;
using HelpDev.Modules.Administration.Domain.Announcements;
using Microsoft.EntityFrameworkCore;

namespace HelpDev.Modules.Administration.Infrastructure.Persistence;

public sealed class AnnouncementQueries : IAnnouncementQueries
{
    private readonly IAdministrationDbContext _dbContext;

    public AnnouncementQueries(IAdministrationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<AnnouncementDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Announcements
            .AsNoTracking()
            .Where(item => item.Id == id)
            .Select(item => new AnnouncementDto(
                item.Id,
                item.Title,
                item.Body,
                item.Type.ToString(),
                item.Status.ToString(),
                item.StartsAtUtc,
                item.EndsAtUtc,
                item.CreatedAtUtc,
                item.UpdatedAtUtc,
                item.PublishedAtUtc))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<AnnouncementPageDto> GetPageAsync(
        AnnouncementFilter filter,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(filter);

        if (filter.Page < 1 || filter.PageSize < 1 || filter.PageSize > AnnouncementPaging.MaxPageSize)
        {
            throw new AdministrationException(
                $"Page must be >= 1 and pageSize must be between 1 and {AnnouncementPaging.MaxPageSize}.",
                AdministrationApplicationErrorCodes.PaginationInvalid);
        }

        var status = AnnouncementEnumParser.ParseStatusOrNull(filter.Status);
        var type = AnnouncementEnumParser.ParseTypeOrNull(filter.Type);

        var query = _dbContext.Announcements.AsNoTracking().AsQueryable();
        if (status is not null)
        {
            query = query.Where(item => item.Status == status);
        }

        if (type is not null)
        {
            query = query.Where(item => item.Type == type);
        }

        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(item => item.UpdatedAtUtc)
            .ThenByDescending(item => item.Id)
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .Select(item => new AnnouncementListItemDto(
                item.Id,
                item.Title,
                item.Type.ToString(),
                item.Status.ToString(),
                item.StartsAtUtc,
                item.EndsAtUtc,
                item.UpdatedAtUtc,
                item.PublishedAtUtc))
            .ToListAsync(cancellationToken);

        return new AnnouncementPageDto(filter.Page, filter.PageSize, total, items);
    }

    public async Task<IReadOnlyList<ActiveAnnouncementDto>> GetActiveAsync(
        DateTime utcNow,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Announcements
            .AsNoTracking()
            .Where(item => item.Status == AnnouncementStatus.Published)
            .Where(item => item.StartsAtUtc == null || item.StartsAtUtc <= utcNow)
            .Where(item => item.EndsAtUtc == null || item.EndsAtUtc > utcNow)
            .OrderByDescending(item => item.UpdatedAtUtc)
            .ThenByDescending(item => item.Id)
            .Select(item => new ActiveAnnouncementDto(
                item.Id,
                item.Title,
                item.Body,
                item.Type.ToString(),
                item.StartsAtUtc,
                item.EndsAtUtc))
            .ToListAsync(cancellationToken);
    }
}

using HelpDev.Modules.Toolbox.Application.Favorites;
using HelpDev.Modules.Toolbox.Application.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HelpDev.Modules.Toolbox.Infrastructure.Persistence;

public sealed class ToolFavoriteQueries : IToolFavoriteQueries
{
    private readonly IToolboxDbContext _dbContext;

    public ToolFavoriteQueries(IToolboxDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<ToolFavoriteDto>> GetUserFavoritesAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var rows = await (
            from favorite in _dbContext.ToolFavorites.AsNoTracking()
            join tool in _dbContext.ToolDefinitions.AsNoTracking()
                on favorite.ToolId equals tool.Id
            join category in _dbContext.ToolCategories.AsNoTracking()
                on tool.CategoryId equals category.Id
            where favorite.UserId == userId
            orderby favorite.CreatedAtUtc descending, favorite.Id descending
            select new
            {
                favorite.ToolId,
                ToolSlug = tool.Slug,
                tool.Name,
                tool.Summary,
                CategoryName = category.Name,
                FavoritedAtUtc = favorite.CreatedAtUtc,
            })
            .ToListAsync(cancellationToken);

        return rows
            .Select(row => new ToolFavoriteDto(
                row.ToolId,
                row.ToolSlug.Value,
                row.Name,
                row.Summary,
                row.CategoryName,
                row.FavoritedAtUtc))
            .ToList();
    }
}

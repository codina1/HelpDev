using HelpDev.Modules.PromptLab.Application.Favorites;
using HelpDev.Modules.PromptLab.Application.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HelpDev.Modules.PromptLab.Infrastructure.Persistence;

public sealed class PromptFavoriteQueries : IPromptFavoriteQueries
{
    private readonly IPromptLabDbContext _dbContext;

    public PromptFavoriteQueries(IPromptLabDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<PromptFavoriteDto>> GetUserFavoritesAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var rows = await (
            from favorite in _dbContext.PromptFavorites.AsNoTracking()
            join prompt in _dbContext.PromptDefinitions.AsNoTracking()
                on favorite.PromptDefinitionId equals prompt.Id
            join category in _dbContext.PromptCategories.AsNoTracking()
                on prompt.CategoryId equals category.Id
            where favorite.UserId == userId
            orderby favorite.CreatedAtUtc descending, favorite.Id descending
            select new
            {
                PromptId = favorite.PromptDefinitionId,
                PromptSlug = prompt.Slug,
                prompt.Name,
                prompt.Summary,
                CategoryName = category.Name,
                FavoritedAtUtc = favorite.CreatedAtUtc,
            })
            .ToListAsync(cancellationToken);

        return rows
            .Select(row => new PromptFavoriteDto(
                row.PromptId,
                row.PromptSlug.Value,
                row.Name,
                row.Summary,
                row.CategoryName,
                row.FavoritedAtUtc))
            .ToList();
    }
}

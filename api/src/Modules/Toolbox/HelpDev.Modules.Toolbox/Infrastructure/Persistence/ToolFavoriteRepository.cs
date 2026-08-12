using HelpDev.Modules.Toolbox.Application.Persistence;
using HelpDev.Modules.Toolbox.Domain.Favorites;
using Microsoft.EntityFrameworkCore;

namespace HelpDev.Modules.Toolbox.Infrastructure.Persistence;

public sealed class ToolFavoriteRepository : IToolFavoriteRepository
{
    private readonly IToolboxDbContext _dbContext;

    public ToolFavoriteRepository(IToolboxDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<ToolFavorite?> GetAsync(
        Guid userId,
        Guid toolId,
        CancellationToken cancellationToken = default) =>
        _dbContext.ToolFavorites.FirstOrDefaultAsync(
            favorite => favorite.UserId == userId && favorite.ToolId == toolId,
            cancellationToken);

    public async Task AddAsync(ToolFavorite favorite, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(favorite);
        await _dbContext.ToolFavorites.AddAsync(favorite, cancellationToken);
    }

    public void Remove(ToolFavorite favorite)
    {
        ArgumentNullException.ThrowIfNull(favorite);
        _dbContext.ToolFavorites.Remove(favorite);
    }
}

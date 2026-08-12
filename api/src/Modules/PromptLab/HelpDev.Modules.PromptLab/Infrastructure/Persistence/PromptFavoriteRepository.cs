using HelpDev.Modules.PromptLab.Application.Persistence;
using HelpDev.Modules.PromptLab.Domain.Favorites;
using Microsoft.EntityFrameworkCore;

namespace HelpDev.Modules.PromptLab.Infrastructure.Persistence;

public sealed class PromptFavoriteRepository : IPromptFavoriteRepository
{
    private readonly IPromptLabDbContext _dbContext;

    public PromptFavoriteRepository(IPromptLabDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<PromptFavorite?> GetAsync(
        Guid userId,
        Guid promptDefinitionId,
        CancellationToken cancellationToken = default) =>
        _dbContext.PromptFavorites.FirstOrDefaultAsync(
            favorite => favorite.UserId == userId && favorite.PromptDefinitionId == promptDefinitionId,
            cancellationToken);

    public async Task AddAsync(PromptFavorite favorite, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(favorite);
        await _dbContext.PromptFavorites.AddAsync(favorite, cancellationToken);
    }

    public void Remove(PromptFavorite favorite)
    {
        ArgumentNullException.ThrowIfNull(favorite);
        _dbContext.PromptFavorites.Remove(favorite);
    }
}

using HelpDev.Modules.PromptLab.Domain.Favorites;

namespace HelpDev.Modules.PromptLab.Application.Persistence;

public interface IPromptFavoriteRepository
{
    Task<PromptFavorite?> GetAsync(
        Guid userId,
        Guid promptDefinitionId,
        CancellationToken cancellationToken = default);

    Task AddAsync(PromptFavorite favorite, CancellationToken cancellationToken = default);

    void Remove(PromptFavorite favorite);
}

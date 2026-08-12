using HelpDev.Modules.Toolbox.Domain.Favorites;

namespace HelpDev.Modules.Toolbox.Application.Persistence;

public interface IToolFavoriteRepository
{
    Task<ToolFavorite?> GetAsync(Guid userId, Guid toolId, CancellationToken cancellationToken = default);

    Task AddAsync(ToolFavorite favorite, CancellationToken cancellationToken = default);

    void Remove(ToolFavorite favorite);
}

namespace HelpDev.Modules.Toolbox.Application.Favorites;

public sealed record ToolFavoriteDto(
    Guid ToolId,
    string ToolSlug,
    string Name,
    string Summary,
    string CategoryName,
    DateTime FavoritedAtUtc);

public interface IToolFavoriteQueries
{
    Task<IReadOnlyList<ToolFavoriteDto>> GetUserFavoritesAsync(
        Guid userId,
        CancellationToken cancellationToken = default);
}

public interface IToolFavoriteService
{
    Task AddAsync(Guid userId, Guid toolId, CancellationToken cancellationToken = default);

    Task RemoveAsync(Guid userId, Guid toolId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ToolFavoriteDto>> GetUserFavoritesAsync(
        Guid userId,
        CancellationToken cancellationToken = default);
}

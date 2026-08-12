namespace HelpDev.Modules.PromptLab.Application.Favorites;

public sealed record PromptFavoriteDto(
    Guid PromptId,
    string PromptSlug,
    string Name,
    string Summary,
    string CategoryName,
    DateTime FavoritedAtUtc);

public interface IPromptFavoriteQueries
{
    Task<IReadOnlyList<PromptFavoriteDto>> GetUserFavoritesAsync(
        Guid userId,
        CancellationToken cancellationToken = default);
}

public interface IPromptFavoriteService
{
    Task AddAsync(Guid userId, Guid promptId, CancellationToken cancellationToken = default);

    Task RemoveAsync(Guid userId, Guid promptId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<PromptFavoriteDto>> GetUserFavoritesAsync(
        Guid userId,
        CancellationToken cancellationToken = default);
}

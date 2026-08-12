using ContentEntity = HelpDev.Modules.Content.Domain.Entities.Content;

namespace HelpDev.Modules.Content.Application.Persistence;

public interface IContentRepository
{
    Task<IReadOnlyList<ContentEntity>> GetPublishedAsync(CancellationToken cancellationToken = default);

    Task<ContentEntity?> GetPublishedBySlugAsync(string slug, CancellationToken cancellationToken = default);

    /// <summary>Loads a tracked aggregate by id for mutation (update/publish). Returns null when absent.</summary>
    Task<ContentEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<bool> SlugExistsAsync(string slug, CancellationToken cancellationToken = default);

    /// <summary>Checks slug uniqueness excluding a specific content id (used on update).</summary>
    Task<bool> SlugExistsForOtherAsync(string slug, Guid excludingContentId, CancellationToken cancellationToken = default);

    Task<ContentEntity> AddAsync(ContentEntity content, CancellationToken cancellationToken = default);
}

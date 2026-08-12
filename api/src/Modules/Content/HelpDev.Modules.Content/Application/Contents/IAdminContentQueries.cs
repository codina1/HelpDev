using HelpDev.Modules.Content.Application.Common;
using HelpDev.Modules.Content.Application.Contents.Dtos;

namespace HelpDev.Modules.Content.Application.Contents;

/// <summary>
/// Read-side port for the admin content management model. Implemented in Infrastructure
/// with server-side pagination/filtering (list) and projection-only detail reads
/// (AsNoTracking, single SQL round-trip, no aggregate tracking). Ownership is enforced
/// by callers in the Application layer, not here.
/// </summary>
public interface IAdminContentQueries
{
    Task<PagedResult<AdminContentListItemDto>> ListAsync(
        ContentSearchFilter filter,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Full admin detail projection by id, or <c>null</c> when the item does not exist.
    /// Includes body, excerpt, cover image, timestamps and SEO metadata.
    /// </summary>
    Task<AdminContentDetailDto?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Full admin detail projection by slug, or <c>null</c> when the slug is invalid
    /// or the item does not exist.
    /// </summary>
    Task<AdminContentDetailDto?> GetBySlugAsync(
        string slug,
        CancellationToken cancellationToken = default);
}

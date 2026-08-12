using HelpDev.Modules.Content.Application.Contents.Dtos;
using HelpDev.Modules.Content.Application.SeoAnalysis;

namespace HelpDev.Modules.Content.Application.Contents;

public interface IContentService
{
    Task<IReadOnlyList<ContentListItemDto>> ListPublishedAsync(CancellationToken cancellationToken = default);

    Task<ContentDetailDto> GetPublishedBySlugAsync(
        string slug,
        Guid? viewerUserId = null,
        CancellationToken cancellationToken = default);

    Task<ContentDetailDto> CreateAsync(Guid authorId, CreateContentRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Edits an existing content item. Enforces ownership (writers may only edit their own content).
    /// </summary>
    Task<AdminContentDetailDto> UpdateAsync(
        ContentManagementActor actor,
        Guid id,
        UpdateContentRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Publishes an existing draft (Draft → Published). Publishing an already-published item is a no-op.
    /// </summary>
    Task<AdminContentDetailDto> PublishAsync(
        ContentManagementActor actor,
        Guid id,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates SEO metadata for an existing content item. Enforces ownership. No-op updates do not
    /// bump timestamps; published-content updates raise <c>content.updated.v1</c> via the outbox.
    /// </summary>
    Task<AdminContentDetailDto> UpdateSeoMetadataAsync(
        ContentManagementActor actor,
        Guid id,
        UpdateSeoMetadataRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Admin CMS detail read by id. Enforces ownership; writers requesting another user's content
    /// receive the same <c>content_not_found</c> response as a missing item (no existence leak).
    /// </summary>
    Task<AdminContentDetailDto> GetManagedByIdAsync(
        ContentManagementActor actor,
        Guid id,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Runs the deterministic SEO analyzer on saved server content. Side-effect free:
    /// no SaveChanges, no domain events, no Outbox. Enforces the same ownership masking as reads.
    /// </summary>
    Task<SeoAuditReportDto> AnalyzeSeoAsync(
        ContentManagementActor actor,
        Guid id,
        CancellationToken cancellationToken = default);
}

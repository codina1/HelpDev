using HelpDev.Modules.Content.Application.Common;
using HelpDev.Modules.Content.Application.Contents;
using HelpDev.Modules.Content.Application.Contents.Dtos;

namespace HelpDev.Modules.Content.Application.Contents.Revisions;

public interface IContentRevisionQueries
{
    Task<PagedResult<ContentRevisionListItemDto>> GetPagedAsync(
        ContentManagementActor actor,
        Guid contentId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<ContentRevisionDetailDto?> GetByVersionAsync(
        ContentManagementActor actor,
        Guid contentId,
        int versionNumber,
        CancellationToken cancellationToken = default);
}

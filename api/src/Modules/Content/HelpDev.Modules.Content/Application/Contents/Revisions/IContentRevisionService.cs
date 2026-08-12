using HelpDev.Modules.Content.Application.Contents.Dtos;
using ContentEntity = HelpDev.Modules.Content.Domain.Entities.Content;

namespace HelpDev.Modules.Content.Application.Contents.Revisions;

public interface IContentRevisionService
{
    Task<AdminContentDetailDto> RestoreAsync(
        ContentManagementActor actor,
        Guid contentId,
        int versionNumber,
        RestoreContentRevisionRequest? request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Appends an immutable revision for the current aggregate state. Caller must commit via UnitOfWork.
    /// </summary>
    Task AppendRevisionAsync(
        ContentEntity content,
        Guid createdByUserId,
        string? changeReason,
        CancellationToken cancellationToken = default);
}

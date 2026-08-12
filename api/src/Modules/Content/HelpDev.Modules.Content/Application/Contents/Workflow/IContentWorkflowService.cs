using HelpDev.Modules.Content.Application.Contents.Dtos;

namespace HelpDev.Modules.Content.Application.Contents.Workflow;

public interface IContentWorkflowService
{
    Task<AdminContentDetailDto> SubmitForReviewAsync(
        ContentManagementActor actor,
        Guid contentId,
        CancellationToken cancellationToken = default);

    Task<AdminContentDetailDto> ApproveAsync(
        ContentManagementActor actor,
        Guid contentId,
        CancellationToken cancellationToken = default);

    Task<AdminContentDetailDto> RejectAsync(
        ContentManagementActor actor,
        Guid contentId,
        RejectContentRequest request,
        CancellationToken cancellationToken = default);

    Task<AdminContentDetailDto> PublishAsync(
        ContentManagementActor actor,
        Guid contentId,
        CancellationToken cancellationToken = default);

    Task<AdminContentDetailDto> ArchiveAsync(
        ContentManagementActor actor,
        Guid contentId,
        CancellationToken cancellationToken = default);

    Task<WorkflowHistoryDto> GetWorkflowHistoryAsync(
        ContentManagementActor actor,
        Guid contentId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Legacy create-as-published: records full workflow chain in one transaction.
    /// </summary>
    Task BootstrapPublishAfterCreateAsync(
        Guid contentId,
        Guid authorId,
        CancellationToken cancellationToken = default);
}

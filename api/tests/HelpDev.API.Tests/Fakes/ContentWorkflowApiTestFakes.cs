using HelpDev.Modules.Content.Application.Contents;
using HelpDev.Modules.Content.Application.Contents.Dtos;
using HelpDev.Modules.Content.Application.Contents.Workflow;

namespace HelpDev.API.Tests.Fakes;

internal sealed class FakeContentWorkflowService : IContentWorkflowService
{
    public ContentManagementActor? LastActor { get; private set; }

    public Guid? LastContentId { get; private set; }

    public string? LastOperation { get; private set; }

    public AdminContentDetailDto DetailToReturn { get; set; } = FakeContentService.CreateSampleDetail();

    public WorkflowHistoryDto HistoryToReturn { get; set; } = new([]);

    public Task<AdminContentDetailDto> SubmitForReviewAsync(
        ContentManagementActor actor,
        Guid contentId,
        CancellationToken cancellationToken = default)
    {
        Record(actor, contentId, nameof(SubmitForReviewAsync));
        return Task.FromResult(DetailToReturn);
    }

    public Task<AdminContentDetailDto> ApproveAsync(
        ContentManagementActor actor,
        Guid contentId,
        CancellationToken cancellationToken = default)
    {
        Record(actor, contentId, nameof(ApproveAsync));
        return Task.FromResult(DetailToReturn);
    }

    public Task<AdminContentDetailDto> RejectAsync(
        ContentManagementActor actor,
        Guid contentId,
        RejectContentRequest request,
        CancellationToken cancellationToken = default)
    {
        Record(actor, contentId, nameof(RejectAsync));
        return Task.FromResult(DetailToReturn);
    }

    public Task<AdminContentDetailDto> PublishAsync(
        ContentManagementActor actor,
        Guid contentId,
        CancellationToken cancellationToken = default)
    {
        Record(actor, contentId, nameof(PublishAsync));
        return Task.FromResult(DetailToReturn);
    }

    public Task<AdminContentDetailDto> ArchiveAsync(
        ContentManagementActor actor,
        Guid contentId,
        CancellationToken cancellationToken = default)
    {
        Record(actor, contentId, nameof(ArchiveAsync));
        return Task.FromResult(DetailToReturn);
    }

    public Task<WorkflowHistoryDto> GetWorkflowHistoryAsync(
        ContentManagementActor actor,
        Guid contentId,
        CancellationToken cancellationToken = default)
    {
        Record(actor, contentId, nameof(GetWorkflowHistoryAsync));
        return Task.FromResult(HistoryToReturn);
    }

    public Task BootstrapPublishAfterCreateAsync(
        Guid contentId,
        Guid authorId,
        CancellationToken cancellationToken = default) =>
        Task.CompletedTask;

    private void Record(ContentManagementActor actor, Guid contentId, string operation)
    {
        LastActor = actor;
        LastContentId = contentId;
        LastOperation = operation;
    }
}

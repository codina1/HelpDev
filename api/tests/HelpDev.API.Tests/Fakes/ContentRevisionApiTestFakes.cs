using HelpDev.Modules.Content.Application.Common;
using HelpDev.Modules.Content.Application.Contents;
using HelpDev.Modules.Content.Application.Contents.Dtos;
using HelpDev.Modules.Content.Application.Contents.Revisions;

namespace HelpDev.API.Tests.Fakes;

internal sealed class FakeContentRevisionQueries : IContentRevisionQueries
{
    public ContentManagementActor? LastActor { get; private set; }

    public Guid? LastContentId { get; private set; }

    public PagedResult<ContentRevisionListItemDto> PagedToReturn { get; set; } =
        new([], 1, 20, 0);

    public ContentRevisionDetailDto? DetailToReturn { get; set; }

    public Task<PagedResult<ContentRevisionListItemDto>> GetPagedAsync(
        ContentManagementActor actor,
        Guid contentId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        LastActor = actor;
        LastContentId = contentId;
        return Task.FromResult(PagedToReturn);
    }

    public Task<ContentRevisionDetailDto?> GetByVersionAsync(
        ContentManagementActor actor,
        Guid contentId,
        int versionNumber,
        CancellationToken cancellationToken = default)
    {
        LastActor = actor;
        LastContentId = contentId;
        return Task.FromResult(DetailToReturn);
    }
}

internal sealed class FakeContentRevisionService : IContentRevisionService
{
    public ContentManagementActor? LastActor { get; private set; }

    public Guid? LastContentId { get; private set; }

    public int? LastVersion { get; private set; }

    public AdminContentDetailDto DetailToReturn { get; set; } = FakeContentService.CreateSampleDetail();

    public Task<AdminContentDetailDto> RestoreAsync(
        ContentManagementActor actor,
        Guid contentId,
        int versionNumber,
        RestoreContentRevisionRequest? request,
        CancellationToken cancellationToken = default)
    {
        LastActor = actor;
        LastContentId = contentId;
        LastVersion = versionNumber;
        return Task.FromResult(DetailToReturn);
    }

    public Task AppendRevisionAsync(
        HelpDev.Modules.Content.Domain.Entities.Content content,
        Guid createdByUserId,
        string? changeReason,
        CancellationToken cancellationToken = default) =>
        Task.CompletedTask;
}

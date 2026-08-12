using HelpDev.Modules.Content.Application.Common;
using HelpDev.Modules.Content.Application.Contents;
using HelpDev.Modules.Content.Application.Contents.Dtos;
using HelpDev.Modules.Content.Application.SeoAnalysis;

namespace HelpDev.API.Tests.Fakes;

internal sealed class FakeContentService : IContentService
{
    public ContentManagementActor? LastActor { get; private set; }

    public Guid? LastContentId { get; private set; }

    public UpdateContentRequest? LastUpdateRequest { get; private set; }

    public UpdateSeoMetadataRequest? LastSeoRequest { get; private set; }

    public string? LastOperation { get; private set; }

    public Exception? ExceptionToThrow { get; set; }

    public AdminContentDetailDto DetailToReturn { get; set; } = CreateSampleDetail();

    public Task<IReadOnlyList<ContentListItemDto>> ListPublishedAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<ContentListItemDto>>([]);

    public Task<ContentDetailDto> GetPublishedBySlugAsync(
        string slug,
        Guid? viewerUserId = null,
        CancellationToken cancellationToken = default) =>
        throw new NotSupportedException();

    public Task<ContentDetailDto> CreateAsync(
        Guid authorId,
        CreateContentRequest request,
        CancellationToken cancellationToken = default) =>
        throw new NotSupportedException();

    public Task<AdminContentDetailDto> UpdateAsync(
        ContentManagementActor actor,
        Guid id,
        UpdateContentRequest request,
        CancellationToken cancellationToken = default)
    {
        ThrowIfNeeded();
        LastActor = actor;
        LastContentId = id;
        LastUpdateRequest = request;
        LastOperation = nameof(UpdateAsync);
        return Task.FromResult(DetailToReturn);
    }

    public Task<AdminContentDetailDto> PublishAsync(
        ContentManagementActor actor,
        Guid id,
        CancellationToken cancellationToken = default)
    {
        ThrowIfNeeded();
        LastActor = actor;
        LastContentId = id;
        LastOperation = nameof(PublishAsync);
        return Task.FromResult(DetailToReturn);
    }

    public Task<AdminContentDetailDto> UpdateSeoMetadataAsync(
        ContentManagementActor actor,
        Guid id,
        UpdateSeoMetadataRequest request,
        CancellationToken cancellationToken = default)
    {
        ThrowIfNeeded();
        LastActor = actor;
        LastContentId = id;
        LastSeoRequest = request;
        LastOperation = nameof(UpdateSeoMetadataAsync);
        return Task.FromResult(DetailToReturn);
    }

    public Task<AdminContentDetailDto> GetManagedByIdAsync(
        ContentManagementActor actor,
        Guid id,
        CancellationToken cancellationToken = default)
    {
        ThrowIfNeeded();
        LastActor = actor;
        LastContentId = id;
        LastOperation = nameof(GetManagedByIdAsync);
        return Task.FromResult(DetailToReturn);
    }

    public SeoAuditReportDto ReportToReturn { get; set; } = CreateSampleAuditReport();

    public Task<SeoAuditReportDto> AnalyzeSeoAsync(
        ContentManagementActor actor,
        Guid id,
        CancellationToken cancellationToken = default)
    {
        ThrowIfNeeded();
        LastActor = actor;
        LastContentId = id;
        LastOperation = nameof(AnalyzeSeoAsync);
        return Task.FromResult(ReportToReturn);
    }

    private void ThrowIfNeeded()
    {
        if (ExceptionToThrow is not null)
        {
            throw ExceptionToThrow;
        }
    }

    internal static AdminContentDetailDto CreateSampleDetail() =>
        new(
            Guid.Parse("33333333-3333-3333-3333-333333333333"),
            "Sample",
            "sample-content",
            "Body",
            "Excerpt",
            null,
            nameof(HelpDev.Modules.Content.Domain.Enums.ContentType.Article),
            nameof(HelpDev.Modules.Content.Domain.Enums.ContentStatus.Draft),
            Guid.Parse("44444444-4444-4444-4444-444444444444"),
            0,
            0,
            DateTime.UtcNow,
            DateTime.UtcNow,
            null,
            new SeoMetadataDto(null, null, null, null, null));

    internal static SeoAuditReportDto CreateSampleAuditReport() =>
        new(
            Guid.Parse("33333333-3333-3333-3333-333333333333"),
            DateTime.UtcNow,
            new SeoAuditSummaryDto(0, 0, 1),
            [
                new SeoAuditFindingDto(
                    "seo.title.missing",
                    SeoPlatformCategory.Metadata,
                    SeoFindingSeverity.Info,
                    "عنوان مؤثر: ok",
                    null,
                    "seoTitle"),
            ]);
}

internal sealed class FakeAdminContentQueries : IAdminContentQueries
{
    public ContentSearchFilter? LastFilter { get; private set; }

    public Guid? LastGetById { get; private set; }

    public string? LastGetBySlug { get; private set; }

    public PagedResult<AdminContentListItemDto> ResultToReturn { get; set; } =
        new([], 1, 20, 0);

    public AdminContentDetailDto? DetailToReturn { get; set; } = FakeContentService.CreateSampleDetail();

    public Task<PagedResult<AdminContentListItemDto>> ListAsync(
        ContentSearchFilter filter,
        CancellationToken cancellationToken = default)
    {
        LastFilter = filter;
        return Task.FromResult(ResultToReturn);
    }

    public Task<AdminContentDetailDto?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        LastGetById = id;
        return Task.FromResult(DetailToReturn);
    }

    public Task<AdminContentDetailDto?> GetBySlugAsync(
        string slug,
        CancellationToken cancellationToken = default)
    {
        LastGetBySlug = slug;
        return Task.FromResult(DetailToReturn);
    }
}

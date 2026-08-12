using HelpDev.Modules.Content.Application.Common;
using HelpDev.Modules.Content.Application.Contents.Dtos;
using HelpDev.Modules.Content.Application.Contents.Revisions;
using HelpDev.Modules.Content.Application.Contents.Workflow;
using HelpDev.Modules.Content.Application.Persistence;
using HelpDev.Modules.Content.Application.SeoAnalysis;
using HelpDev.Modules.Content.Domain.Enums;
using HelpDev.Modules.Content.Domain.ValueObjects;
using HelpDev.SharedApplication.Abstractions.Persistence;
using HelpDev.SharedContracts.Analytics;
using HelpDev.SharedKernel.Exceptions;
using HelpDev.SharedKernel.Time;
using Microsoft.Extensions.Logging;
using ContentEntity = HelpDev.Modules.Content.Domain.Entities.Content;

namespace HelpDev.Modules.Content.Application.Contents;

public sealed class ContentService : IContentService
{
    private readonly IContentRepository _contentRepository;
    private readonly IAdminContentQueries _adminContentQueries;
    private readonly IContentSeoAnalyzer _seoAnalyzer;
    private readonly IContentRevisionService _revisionService;
    private readonly IContentWorkflowService _workflowService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDateTimeProvider _clock;
    private readonly IAnalyticsEventIngestor _analyticsIngestor;
    private readonly ILogger<ContentService> _logger;

    public ContentService(
        IContentRepository contentRepository,
        IAdminContentQueries adminContentQueries,
        IContentSeoAnalyzer seoAnalyzer,
        IContentRevisionService revisionService,
        IContentWorkflowService workflowService,
        IUnitOfWork unitOfWork,
        IDateTimeProvider clock,
        IAnalyticsEventIngestor analyticsIngestor,
        ILogger<ContentService> logger)
    {
        _contentRepository = contentRepository;
        _adminContentQueries = adminContentQueries;
        _seoAnalyzer = seoAnalyzer;
        _revisionService = revisionService;
        _workflowService = workflowService;
        _unitOfWork = unitOfWork;
        _clock = clock;
        _analyticsIngestor = analyticsIngestor;
        _logger = logger;
    }

    public async Task<IReadOnlyList<ContentListItemDto>> ListPublishedAsync(
        CancellationToken cancellationToken = default)
    {
        var items = await _contentRepository.GetPublishedAsync(cancellationToken);
        return items.Select(MapToListItem).ToList();
    }

    public async Task<ContentDetailDto> GetPublishedBySlugAsync(
        string slug,
        Guid? viewerUserId = null,
        CancellationToken cancellationToken = default)
    {
        if (!SlugNormalizer.TryNormalize(slug, out var normalizedSlug))
        {
            throw new ContentException("اسلاگ معتبر نیست.");
        }

        var content = await _contentRepository.GetPublishedBySlugAsync(normalizedSlug, cancellationToken);
        if (content is null)
        {
            throw new ContentException("محتوا یافت نشد.");
        }

        await TryIngestViewAsync(content, viewerUserId, cancellationToken);
        return MapToDetail(content);
    }

    public async Task<ContentDetailDto> CreateAsync(
        Guid authorId,
        CreateContentRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!Slug.TryCreate(request.Slug, out var slug) || slug is null)
        {
            throw new ContentException("اسلاگ معتبر نیست.");
        }

        if (!Enum.TryParse<ContentType>(request.Type, ignoreCase: true, out var type))
        {
            throw new ContentException("نوع محتوا معتبر نیست.");
        }

        if (!Enum.TryParse<ContentStatus>(request.Status, ignoreCase: true, out var status))
        {
            throw new ContentException("وضعیت محتوا معتبر نیست.");
        }

        if (await _contentRepository.SlugExistsAsync(slug.Value, cancellationToken))
        {
            throw new ContentException("این اسلاگ قبلاً استفاده شده است.", ContentErrorCodes.SlugDuplicate);
        }

        try
        {
            var wantPublished = status == ContentStatus.Published;
            var content = ContentEntity.Create(
                Guid.NewGuid(),
                request.Title,
                slug,
                request.Body,
                type,
                authorId,
                ContentStatus.Draft,
                _clock.UtcNow);

            await _contentRepository.AddAsync(content, cancellationToken);

            if (wantPublished)
            {
                await _workflowService.BootstrapPublishAfterCreateAsync(content.Id, authorId, cancellationToken);
            }

            await TryIngestCreatedAsync(content, cancellationToken);
            return MapToDetail(content);
        }
        catch (DomainException ex)
        {
            throw new ContentException(ex.Message);
        }
    }

    public async Task<AdminContentDetailDto> UpdateAsync(
        ContentManagementActor actor,
        Guid id,
        UpdateContentRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var content = await GetManagedContentAsync(actor, id, cancellationToken);

        if (!Slug.TryCreate(request.Slug, out var slug) || slug is null)
        {
            throw new ContentException("اسلاگ معتبر نیست.");
        }

        if (!Enum.TryParse<ContentType>(request.Type, ignoreCase: true, out var type))
        {
            throw new ContentException("نوع محتوا معتبر نیست.");
        }

        if (slug != content.Slug
            && await _contentRepository.SlugExistsForOtherAsync(slug.Value, content.Id, cancellationToken))
        {
            throw new ContentException("این اسلاگ قبلاً استفاده شده است.", ContentErrorCodes.SlugDuplicate);
        }

        try
        {
            var changed = content.UpdateDetails(
                request.Title,
                slug,
                type,
                request.Body,
                request.Excerpt,
                request.CoverImage,
                _clock.UtcNow);

            if (changed)
            {
                await _revisionService.AppendRevisionAsync(content, actor.UserId, changeReason: null, cancellationToken)
                    .ConfigureAwait(false);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return MapToAdminDetail(content);
        }
        catch (DomainException ex)
        {
            throw new ContentException(ex.Message, ContentErrorCodes.OperationInvalid, ex);
        }
    }

    public Task<AdminContentDetailDto> PublishAsync(
        ContentManagementActor actor,
        Guid id,
        CancellationToken cancellationToken = default) =>
        _workflowService.PublishAsync(actor, id, cancellationToken);

    public async Task<AdminContentDetailDto> UpdateSeoMetadataAsync(
        ContentManagementActor actor,
        Guid id,
        UpdateSeoMetadataRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var content = await GetManagedContentAsync(actor, id, cancellationToken);

        try
        {
            var seoMetadata = SeoMetadata.Create(
                request.SeoTitle,
                request.SeoDescription,
                request.CanonicalUrl,
                request.OgImage,
                request.FocusKeyword);

            var changed = content.UpdateSeoMetadata(seoMetadata, _clock.UtcNow);

            if (changed)
            {
                await _revisionService.AppendRevisionAsync(content, actor.UserId, changeReason: null, cancellationToken)
                    .ConfigureAwait(false);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return MapToAdminDetail(content);
        }
        catch (DomainException ex)
        {
            throw new ContentException(ex.Message, ContentErrorCodes.OperationInvalid, ex);
        }
    }

    public async Task<AdminContentDetailDto> GetManagedByIdAsync(
        ContentManagementActor actor,
        Guid id,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(actor);

        var detail = await _adminContentQueries.GetByIdAsync(id, cancellationToken);
        if (detail is null)
        {
            throw new ContentException("محتوا یافت نشد.", ContentErrorCodes.NotFound);
        }

        EnsureCanManage(detail.AuthorId, actor);
        return detail;
    }

    public async Task<SeoAuditReportDto> AnalyzeSeoAsync(
        ContentManagementActor actor,
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var detail = await GetManagedByIdAsync(actor, id, cancellationToken);

        var input = new SeoAnalysisInput(
            detail.Title,
            detail.Slug,
            detail.Body,
            detail.Excerpt,
            detail.CoverImage,
            detail.ContentType,
            detail.Seo.SeoTitle,
            detail.Seo.SeoDescription,
            detail.Seo.CanonicalUrl,
            detail.Seo.OgImage,
            detail.Seo.FocusKeyword);

        // Pure analysis — no SaveChanges, no domain events, no Outbox.
        var report = _seoAnalyzer.Analyze(input, _clock.UtcNow);
        return SeoAuditMapper.ToDto(detail.Id, report);
    }

    private async Task<ContentEntity> GetManagedContentAsync(
        ContentManagementActor actor,
        Guid id,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(actor);

        var content = await _contentRepository.GetByIdAsync(id, cancellationToken);
        if (content is null)
        {
            throw new ContentException("محتوا یافت نشد.", ContentErrorCodes.NotFound);
        }

        EnsureCanManage(content.AuthorId, actor);
        return content;
    }

    /// <summary>
    /// Cross-owner access is indistinguishable from a missing item (content_not_found).
    /// </summary>
    public static void EnsureCanManage(ContentEntity content, ContentManagementActor actor)
    {
        ArgumentNullException.ThrowIfNull(content);
        EnsureCanManage(content.AuthorId, actor);
    }

    /// <summary>
    /// Cross-owner access is indistinguishable from a missing item (content_not_found).
    /// </summary>
    public static void EnsureCanManage(Guid authorId, ContentManagementActor actor)
    {
        ArgumentNullException.ThrowIfNull(actor);

        if (actor.CanManageAllContent || authorId == actor.UserId)
        {
            return;
        }

        throw new ContentException("محتوا یافت نشد.", ContentErrorCodes.NotFound);
    }

    private static AdminContentDetailDto MapToAdminDetail(ContentEntity content) =>
        new(
            content.Id,
            content.Title,
            content.Slug.Value,
            content.Body,
            content.Excerpt,
            content.CoverImage,
            content.Type.ToString(),
            content.Status.ToString(),
            content.AuthorId,
            content.Views,
            content.Saves,
            content.CreatedAt,
            content.UpdatedAt,
            content.PublishedAtUtc,
            new SeoMetadataDto(
                content.SeoMetadata.SeoTitle,
                content.SeoMetadata.SeoDescription,
                content.SeoMetadata.CanonicalUrl,
                content.SeoMetadata.OgImage,
                content.SeoMetadata.FocusKeyword));

    private static ContentListItemDto MapToListItem(ContentEntity content) =>
        new(
            content.Id,
            content.Title,
            content.Slug.Value,
            content.Type.ToString(),
            content.AuthorId,
            content.Views,
            content.Saves,
            content.CreatedAt);

    private static ContentDetailDto MapToDetail(ContentEntity content) =>
        new(
            content.Id,
            content.Title,
            content.Slug.Value,
            content.Body,
            content.Type.ToString(),
            content.AuthorId,
            content.Status.ToString(),
            content.Views,
            content.Saves,
            content.CreatedAt);

    private async Task TryIngestViewAsync(
        ContentEntity content,
        Guid? viewerUserId,
        CancellationToken cancellationToken)
    {
        try
        {
            await _analyticsIngestor.IngestAsync(
                new AnalyticsEventEnvelope(
                    Guid.NewGuid(),
                    AnalyticsEventTypes.ContentItemViewed,
                    DateTime.UtcNow,
                    viewerUserId,
                    content.Id,
                    "Content",
                    new Dictionary<string, string>
                    {
                        [AnalyticsDimensionKeys.ContentType] = content.Type.ToString(),
                        [AnalyticsDimensionKeys.IsAuthenticated] = viewerUserId.HasValue ? "true" : "false",
                    },
                    SubjectDisplayName: content.Title,
                    SubjectSlug: content.Slug.Value),
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Analytics content view ingestion skipped.");
        }
    }

    private async Task TryIngestCreatedAsync(ContentEntity content, CancellationToken cancellationToken)
    {
        try
        {
            await _analyticsIngestor.IngestAsync(
                new AnalyticsEventEnvelope(
                    Guid.NewGuid(),
                    AnalyticsEventTypes.ContentItemCreated,
                    DateTime.UtcNow,
                    content.AuthorId,
                    content.Id,
                    "Content",
                    new Dictionary<string, string>
                    {
                        [AnalyticsDimensionKeys.ContentType] = content.Type.ToString(),
                    },
                    SubjectDisplayName: content.Title,
                    SubjectSlug: content.Slug.Value),
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Analytics content created ingestion skipped.");
        }
    }
}
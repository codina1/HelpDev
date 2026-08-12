using HelpDev.Modules.Content.Application.Contents;
using HelpDev.Modules.Content.Application.Contents.Dtos;
using HelpDev.Modules.Content.Application.Persistence;
using HelpDev.Modules.Content.Domain.Entities;
using HelpDev.Modules.Content.Domain.ValueObjects;
using HelpDev.SharedApplication.Abstractions.Persistence;
using HelpDev.SharedKernel.Exceptions;
using HelpDev.SharedKernel.Time;
using ContentEntity = HelpDev.Modules.Content.Domain.Entities.Content;

namespace HelpDev.Modules.Content.Application.Contents.Revisions;

public sealed class ContentRevisionService : IContentRevisionService
{
    private readonly IContentRepository _contentRepository;
    private readonly IContentRevisionRepository _revisionRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDateTimeProvider _clock;

    public ContentRevisionService(
        IContentRepository contentRepository,
        IContentRevisionRepository revisionRepository,
        IUnitOfWork unitOfWork,
        IDateTimeProvider clock)
    {
        _contentRepository = contentRepository;
        _revisionRepository = revisionRepository;
        _unitOfWork = unitOfWork;
        _clock = clock;
    }

    public async Task AppendRevisionAsync(
        ContentEntity content,
        Guid createdByUserId,
        string? changeReason,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(content);

        var maxVersion = await _revisionRepository.GetMaxVersionNumberAsync(content.Id, cancellationToken)
            .ConfigureAwait(false);
        var nextVersion = maxVersion + 1;
        var snapshot = ContentRevisionSnapshot.FromContent(content);

        var revision = ContentRevision.Create(
            Guid.NewGuid(),
            content.Id,
            nextVersion,
            snapshot,
            changeReason,
            createdByUserId,
            _clock.UtcNow);

        await _revisionRepository.AddAsync(revision, cancellationToken).ConfigureAwait(false);
    }

    public async Task<AdminContentDetailDto> RestoreAsync(
        ContentManagementActor actor,
        Guid contentId,
        int versionNumber,
        RestoreContentRevisionRequest? request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(actor);

        if (versionNumber <= 0)
        {
            throw new ContentException("شماره نسخه معتبر نیست.");
        }

        var content = await _contentRepository.GetByIdAsync(contentId, cancellationToken).ConfigureAwait(false);
        if (content is null)
        {
            throw new ContentException("محتوا یافت نشد.", ContentErrorCodes.NotFound);
        }

        ContentService.EnsureCanManage(content, actor);

        var revision = await _revisionRepository
            .GetByContentIdAndVersionAsync(contentId, versionNumber, cancellationToken)
            .ConfigureAwait(false);
        if (revision is null)
        {
            throw new ContentException("نسخه یافت نشد.", ContentErrorCodes.NotFound);
        }

        try
        {
            content.RestoreFromSnapshot(revision.Snapshot, _clock.UtcNow);
            await AppendRevisionAsync(
                    content,
                    actor.UserId,
                    request?.ChangeReason,
                    cancellationToken)
                .ConfigureAwait(false);
            await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            return ContentServiceMap.ToAdminDetail(content);
        }
        catch (ArgumentException ex)
        {
            throw new ContentException(ex.Message, ContentErrorCodes.Validation, ex);
        }
        catch (DomainException ex)
        {
            throw new ContentException(ex.Message, ContentErrorCodes.OperationInvalid, ex);
        }
    }
}

/// <summary>Maps aggregate to admin DTO without duplicating ContentService private mapper.</summary>
internal static class ContentServiceMap
{
    public static AdminContentDetailDto ToAdminDetail(ContentEntity content) =>
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
}

using HelpDev.Modules.Content.Application.Contents;
using HelpDev.Modules.Content.Application.News.Dtos;
using HelpDev.Modules.Content.Application.Persistence;
using HelpDev.Modules.Content.Domain.Enums;
using HelpDev.Modules.Content.Domain.News;
using HelpDev.SharedApplication.Abstractions.Persistence;
using HelpDev.SharedKernel.Exceptions;
using HelpDev.SharedKernel.Time;

namespace HelpDev.Modules.Content.Application.News;

public sealed class NewsMetadataService : INewsMetadataService
{
    private readonly IContentRepository _contentRepository;
    private readonly INewsMetadataRepository _metadataRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDateTimeProvider _clock;

    public NewsMetadataService(
        IContentRepository contentRepository,
        INewsMetadataRepository metadataRepository,
        IUnitOfWork unitOfWork,
        IDateTimeProvider clock)
    {
        _contentRepository = contentRepository;
        _metadataRepository = metadataRepository;
        _unitOfWork = unitOfWork;
        _clock = clock;
    }

    public async Task<NewsMetadataDto?> GetByContentIdAsync(
        ContentManagementActor actor,
        Guid contentId,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(actor);

        var content = await LoadNewsContentAsync(actor, contentId, cancellationToken).ConfigureAwait(false);
        var metadata = await _metadataRepository.GetByContentIdAsync(content.Id, cancellationToken)
            .ConfigureAwait(false);
        return metadata is null ? null : Map(metadata);
    }

    public async Task<NewsMetadataDto> CreateAsync(
        ContentManagementActor actor,
        Guid contentId,
        UpdateNewsMetadataRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(actor);
        ArgumentNullException.ThrowIfNull(request);

        var content = await LoadNewsContentAsync(actor, contentId, cancellationToken).ConfigureAwait(false);
        var existing = await _metadataRepository.GetByContentIdAsync(content.Id, cancellationToken)
            .ConfigureAwait(false);
        if (existing is not null)
        {
            throw new ContentException("متادیتای خبر قبلاً ایجاد شده است.", ContentErrorCodes.OperationInvalid);
        }

        try
        {
            var priority = ParsePriority(request.Priority);
            var newsDate = request.NewsDateUtc == default ? _clock.UtcNow : request.NewsDateUtc;
            var metadata = NewsMetadata.Create(
                Guid.NewGuid(),
                content.Id,
                request.SourceName,
                request.SourceUrl,
                newsDate,
                priority,
                request.ExternalReference,
                _clock.UtcNow);

            await _metadataRepository.AddAsync(metadata, cancellationToken).ConfigureAwait(false);
            await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            return Map(metadata);
        }
        catch (DomainException ex)
        {
            throw new ContentException(ex.Message, ContentErrorCodes.Validation, ex);
        }
    }

    public async Task<NewsMetadataDto> UpdateAsync(
        ContentManagementActor actor,
        Guid contentId,
        UpdateNewsMetadataRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(actor);
        ArgumentNullException.ThrowIfNull(request);

        var content = await LoadNewsContentAsync(actor, contentId, cancellationToken).ConfigureAwait(false);
        var metadata = await _metadataRepository.GetByContentIdAsync(content.Id, cancellationToken)
            .ConfigureAwait(false);
        if (metadata is null)
        {
            throw new ContentException("متادیتای خبر یافت نشد.", ContentErrorCodes.NotFound);
        }

        try
        {
            var priority = ParsePriority(request.Priority);
            var newsDate = request.NewsDateUtc == default ? _clock.UtcNow : request.NewsDateUtc;
            metadata.Update(
                request.SourceName,
                request.SourceUrl,
                newsDate,
                priority,
                request.ExternalReference,
                _clock.UtcNow);

            await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            return Map(metadata);
        }
        catch (DomainException ex)
        {
            throw new ContentException(ex.Message, ContentErrorCodes.Validation, ex);
        }
    }

    private async Task<Domain.Entities.Content> LoadNewsContentAsync(
        ContentManagementActor actor,
        Guid contentId,
        CancellationToken cancellationToken)
    {
        var content = await _contentRepository.GetByIdAsync(contentId, cancellationToken).ConfigureAwait(false);
        if (content is null)
        {
            throw new ContentException("محتوا یافت نشد.", ContentErrorCodes.NotFound);
        }

        ContentService.EnsureCanManage(content, actor);

        if (content.Type != ContentType.News)
        {
            throw new ContentException("این محتوا از نوع خبر نیست.", ContentErrorCodes.Validation);
        }

        return content;
    }

    private static NewsPriority ParsePriority(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)
            || !Enum.TryParse<NewsPriority>(value.Trim(), ignoreCase: true, out var priority)
            || !Enum.IsDefined(priority))
        {
            throw new DomainException("اولویت خبر معتبر نیست.");
        }

        return priority;
    }

    private static NewsMetadataDto Map(NewsMetadata metadata) =>
        new(
            metadata.Id,
            metadata.ContentId,
            metadata.SourceName,
            metadata.SourceUrl,
            metadata.NewsDateUtc,
            metadata.Priority.ToString(),
            metadata.ExternalReference,
            metadata.CreatedAtUtc,
            metadata.UpdatedAtUtc);
}

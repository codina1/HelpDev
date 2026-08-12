using HelpDev.Modules.Content.Application.Articles.Dtos;
using HelpDev.Modules.Content.Application.Contents;
using HelpDev.Modules.Content.Application.Persistence;
using HelpDev.Modules.Content.Domain.Articles;
using HelpDev.Modules.Content.Domain.Enums;
using HelpDev.SharedApplication.Abstractions.Persistence;
using HelpDev.SharedKernel.Exceptions;
using HelpDev.SharedKernel.Time;

namespace HelpDev.Modules.Content.Application.Articles;

public sealed class ArticleMetadataService : IArticleMetadataService
{
    private readonly IContentRepository _contentRepository;
    private readonly IArticleMetadataRepository _metadataRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDateTimeProvider _clock;

    public ArticleMetadataService(
        IContentRepository contentRepository,
        IArticleMetadataRepository metadataRepository,
        IUnitOfWork unitOfWork,
        IDateTimeProvider clock)
    {
        _contentRepository = contentRepository;
        _metadataRepository = metadataRepository;
        _unitOfWork = unitOfWork;
        _clock = clock;
    }

    public async Task<ArticleMetadataDto?> GetByContentIdAsync(
        ContentManagementActor actor,
        Guid contentId,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(actor);

        var content = await LoadArticleContentAsync(actor, contentId, cancellationToken).ConfigureAwait(false);
        var metadata = await _metadataRepository.GetByContentIdAsync(content.Id, cancellationToken)
            .ConfigureAwait(false);
        return metadata is null ? null : Map(metadata);
    }

    public async Task<ArticleMetadataDto> CreateAsync(
        ContentManagementActor actor,
        Guid contentId,
        UpdateArticleMetadataRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(actor);
        ArgumentNullException.ThrowIfNull(request);

        var content = await LoadArticleContentAsync(actor, contentId, cancellationToken).ConfigureAwait(false);
        var existing = await _metadataRepository.GetByContentIdAsync(content.Id, cancellationToken)
            .ConfigureAwait(false);
        if (existing is not null)
        {
            throw new ContentException("متادیتای مقاله قبلاً ایجاد شده است.", ContentErrorCodes.OperationInvalid);
        }

        try
        {
            var difficulty = ParseDifficulty(request.DifficultyLevel);
            var metadata = ArticleMetadata.Create(
                Guid.NewGuid(),
                content.Id,
                request.CategoryId,
                difficulty,
                request.ReadingTimeMinutes,
                request.IsFeatured,
                request.AllowComments,
                request.TableOfContentsEnabled,
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

    public async Task<ArticleMetadataDto> UpdateAsync(
        ContentManagementActor actor,
        Guid contentId,
        UpdateArticleMetadataRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(actor);
        ArgumentNullException.ThrowIfNull(request);

        var content = await LoadArticleContentAsync(actor, contentId, cancellationToken).ConfigureAwait(false);
        var metadata = await _metadataRepository.GetByContentIdAsync(content.Id, cancellationToken)
            .ConfigureAwait(false);
        if (metadata is null)
        {
            throw new ContentException("متادیتای مقاله یافت نشد.", ContentErrorCodes.NotFound);
        }

        try
        {
            var difficulty = ParseDifficulty(request.DifficultyLevel);
            metadata.Update(
                request.CategoryId,
                difficulty,
                request.ReadingTimeMinutes,
                request.IsFeatured,
                request.AllowComments,
                request.TableOfContentsEnabled,
                _clock.UtcNow);

            await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            return Map(metadata);
        }
        catch (DomainException ex)
        {
            throw new ContentException(ex.Message, ContentErrorCodes.Validation, ex);
        }
    }

    private async Task<Domain.Entities.Content> LoadArticleContentAsync(
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

        if (content.Type != ContentType.Article)
        {
            throw new ContentException("این محتوا از نوع مقاله نیست.", ContentErrorCodes.Validation);
        }

        return content;
    }

    private static DifficultyLevel ParseDifficulty(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)
            || !Enum.TryParse<DifficultyLevel>(value.Trim(), ignoreCase: true, out var difficulty)
            || !Enum.IsDefined(difficulty))
        {
            throw new DomainException("سطح دشواری معتبر نیست.");
        }

        return difficulty;
    }

    private static ArticleMetadataDto Map(ArticleMetadata metadata) =>
        new(
            metadata.Id,
            metadata.ContentId,
            metadata.CategoryId,
            metadata.DifficultyLevel.ToString(),
            metadata.ReadingTimeMinutes,
            metadata.IsFeatured,
            metadata.AllowComments,
            metadata.TableOfContentsEnabled,
            metadata.CreatedAtUtc,
            metadata.UpdatedAtUtc);
}

using HelpDev.Modules.Media.Application.Options;
using HelpDev.Modules.Media.Application.Persistence;
using HelpDev.Modules.Media.Application.Storage;
using HelpDev.Modules.Media.Application.Validation;
using HelpDev.Modules.Media.Domain.Assets;
using HelpDev.Modules.Media.Domain.ValueObjects;
using HelpDev.SharedKernel.Exceptions;
using HelpDev.SharedKernel.Time;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace HelpDev.Modules.Media.Application.Assets;

/// <summary>
/// Upload flow: validate → inspect → store → persist → commit once.
/// Consistency: store first, then DB. On DB failure, attempt storage cleanup (best-effort).
/// No Outbox events in v1.
/// </summary>
public sealed class MediaAssetService : IMediaAssetService
{
    private readonly IMediaAssetRepository _repository;
    private readonly IMediaAssetQueries _queries;
    private readonly IMediaStorage _storage;
    private readonly IImageFileInspector _inspector;
    private readonly IMediaDbContext _dbContext;
    private readonly IDateTimeProvider _clock;
    private readonly MediaOptions _options;
    private readonly ILogger<MediaAssetService> _logger;

    public MediaAssetService(
        IMediaAssetRepository repository,
        IMediaAssetQueries queries,
        IMediaStorage storage,
        IImageFileInspector inspector,
        IMediaDbContext dbContext,
        IDateTimeProvider clock,
        IOptions<MediaOptions> options,
        ILogger<MediaAssetService> logger)
    {
        _repository = repository;
        _queries = queries;
        _storage = storage;
        _inspector = inspector;
        _dbContext = dbContext;
        _clock = clock;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<MediaAssetDto> UploadAsync(
        MediaManagementActor actor,
        UploadMediaAssetRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(actor);
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(request.Content);

        if (request.SizeBytes <= 0)
        {
            throw new MediaException("فایل خالی است.", MediaErrorCodes.Validation);
        }

        if (request.SizeBytes > _options.MaxUploadBytes)
        {
            throw new MediaException(
                $"حجم فایل از حداکثر مجاز ({_options.MaxUploadBytes} بایت) بیشتر است.",
                MediaErrorCodes.PayloadTooLarge);
        }

        MediaFileName originalName;
        try
        {
            originalName = MediaFileName.Create(request.OriginalFileName, _options.MaxOriginalFileNameLength);
        }
        catch (DomainException ex)
        {
            throw new MediaException(ex.Message, MediaErrorCodes.Validation, ex);
        }

        // Buffer once so inspection and storage share a rewindable stream.
        await using var buffer = new MemoryStream(capacity: (int)Math.Min(request.SizeBytes, int.MaxValue));
        await request.Content.CopyToAsync(buffer, cancellationToken).ConfigureAwait(false);
        if (buffer.Length <= 0)
        {
            throw new MediaException("فایل خالی است.", MediaErrorCodes.Validation);
        }

        if (buffer.Length > _options.MaxUploadBytes)
        {
            throw new MediaException(
                $"حجم فایل از حداکثر مجاز ({_options.MaxUploadBytes} بایت) بیشتر است.",
                MediaErrorCodes.PayloadTooLarge);
        }

        buffer.Position = 0;
        ImageInspectionResult inspection;
        try
        {
            inspection = await _inspector.InspectAsync(buffer, request.DeclaredContentType, cancellationToken)
                .ConfigureAwait(false);
        }
        catch (MediaException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new MediaException("تصویر نامعتبر است.", MediaErrorCodes.UnsupportedType, ex);
        }

        if (!_options.AllowedContentTypes.Contains(inspection.DetectedContentType, StringComparer.OrdinalIgnoreCase))
        {
            throw new MediaException("نوع تصویر پشتیبانی نمی‌شود.", MediaErrorCodes.UnsupportedType);
        }

        if (inspection.Width > _options.MaxWidth || inspection.Height > _options.MaxHeight)
        {
            throw new MediaException(
                $"ابعاد تصویر از حد مجاز ({_options.MaxWidth}×{_options.MaxHeight}) بیشتر است.",
                MediaErrorCodes.Validation);
        }

        MediaContentType contentType;
        try
        {
            contentType = MediaContentType.Create(inspection.DetectedContentType);
        }
        catch (DomainException ex)
        {
            throw new MediaException(ex.Message, MediaErrorCodes.UnsupportedType, ex);
        }

        var assetId = Guid.NewGuid();
        var now = _clock.UtcNow;
        var storageKeyValue = $"{now:yyyy}/{now:MM}/{assetId:N}{inspection.SafeExtension}";
        var storageKey = MediaStorageKey.Create(storageKeyValue);
        var publicBase = _options.PublicBasePath.TrimEnd('/');
        var publicUrl = $"{publicBase}/{storageKey.Value}";

        buffer.Position = 0;
        try
        {
            await _storage.StoreAsync(buffer, storageKey.Value, contentType.Value, cancellationToken)
                .ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Media storage failed for asset {AssetId}", assetId);
            throw new MediaException("ذخیره‌سازی فایل ناموفق بود.", MediaErrorCodes.StorageFailed, ex);
        }

        MediaAsset asset;
        try
        {
            asset = MediaAsset.Create(
                assetId,
                originalName,
                storageKey,
                contentType,
                buffer.Length,
                inspection.Width,
                inspection.Height,
                publicUrl,
                actor.UserId,
                now,
                request.AltText,
                request.Caption,
                _options.MaxAltTextLength,
                _options.MaxCaptionLength);
        }
        catch (DomainException ex)
        {
            await TryCleanupStorageAsync(storageKey.Value, cancellationToken).ConfigureAwait(false);
            throw new MediaException(ex.Message, MediaErrorCodes.Validation, ex);
        }

        try
        {
            await _repository.AddAsync(asset, cancellationToken).ConfigureAwait(false);
            await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Media DB commit failed for asset {AssetId}; attempting storage cleanup",
                assetId);
            await TryCleanupStorageAsync(storageKey.Value, cancellationToken).ConfigureAwait(false);
            throw new MediaException("ذخیرهٔ متادیتای رسانه ناموفق بود.", MediaErrorCodes.StorageFailed, ex);
        }

        return Map(asset);
    }

    public async Task<MediaAssetDto> GetManagedByIdAsync(
        MediaManagementActor actor,
        Guid id,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(actor);

        var detail = await _queries.GetByIdAsync(id, cancellationToken).ConfigureAwait(false);
        if (detail is null)
        {
            throw new MediaException("رسانه یافت نشد.", MediaErrorCodes.NotFound);
        }

        if (!actor.CanManageAllAssets && detail.UploadedByUserId != actor.UserId)
        {
            throw new MediaException("رسانه یافت نشد.", MediaErrorCodes.NotFound);
        }

        return detail;
    }

    private async Task TryCleanupStorageAsync(string storageKey, CancellationToken cancellationToken)
    {
        try
        {
            await _storage.DeleteAsync(storageKey, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Media storage cleanup failed for key length {KeyLength}", storageKey.Length);
        }
    }

    public static MediaAssetDto Map(MediaAsset asset) =>
        new(
            asset.Id,
            asset.OriginalFileName,
            asset.ContentType,
            asset.SizeBytes,
            asset.Width,
            asset.Height,
            asset.PublicUrl,
            asset.AltText,
            asset.Caption,
            asset.UploadedByUserId,
            asset.CreatedAtUtc,
            asset.UpdatedAtUtc,
            asset.Status.ToString());
}

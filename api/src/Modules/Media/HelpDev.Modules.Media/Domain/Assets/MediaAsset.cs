using HelpDev.Modules.Media.Domain.Enums;
using HelpDev.Modules.Media.Domain.ValueObjects;
using HelpDev.SharedKernel.Common;
using HelpDev.SharedKernel.Exceptions;

namespace HelpDev.Modules.Media.Domain.Assets;

/// <summary>
/// Media asset aggregate. File bytes live in object storage; PostgreSQL stores metadata only.
/// V1 does not raise Outbox domain events (no subscribers).
/// </summary>
public sealed class MediaAsset : AggregateRoot<Guid>
{
    private MediaAsset()
    {
    }

    private MediaAsset(Guid id)
        : base(id)
    {
    }

    public string OriginalFileName { get; private set; } = string.Empty;

    public string StorageKey { get; private set; } = string.Empty;

    public string ContentType { get; private set; } = string.Empty;

    public long SizeBytes { get; private set; }

    public int Width { get; private set; }

    public int Height { get; private set; }

    /// <summary>Public URL path (e.g. /media/2026/07/{id}.jpg). Never a filesystem path.</summary>
    public string PublicUrl { get; private set; } = string.Empty;

    public string? AltText { get; private set; }

    public string? Caption { get; private set; }

    public Guid UploadedByUserId { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    public DateTime UpdatedAtUtc { get; private set; }

    public MediaAssetStatus Status { get; private set; } = MediaAssetStatus.Active;

    public bool UpdateMetadata(
        string? altText,
        string? caption,
        DateTime updatedAtUtc,
        int maxAltTextLength = 200,
        int maxCaptionLength = 500)
    {
        var normalizedAlt = NormalizeOptional(altText, maxAltTextLength, "متن جایگزین");
        var normalizedCaption = NormalizeOptional(caption, maxCaptionLength, "توضیح");
        if (string.Equals(AltText, normalizedAlt, StringComparison.Ordinal)
            && string.Equals(Caption, normalizedCaption, StringComparison.Ordinal))
        {
            return false;
        }

        AltText = normalizedAlt;
        Caption = normalizedCaption;
        UpdatedAtUtc = updatedAtUtc;
        return true;
    }

    public bool Archive(DateTime updatedAtUtc)
    {
        if (Status == MediaAssetStatus.Archived)
        {
            return false;
        }

        Status = MediaAssetStatus.Archived;
        UpdatedAtUtc = updatedAtUtc;
        return true;
    }

    public static MediaAsset Create(
        Guid id,
        MediaFileName originalFileName,
        MediaStorageKey storageKey,
        MediaContentType contentType,
        long sizeBytes,
        int width,
        int height,
        string publicUrl,
        Guid uploadedByUserId,
        DateTime createdAtUtc,
        string? altText = null,
        string? caption = null,
        int maxAltTextLength = 200,
        int maxCaptionLength = 500)
    {
        ArgumentNullException.ThrowIfNull(originalFileName);
        ArgumentNullException.ThrowIfNull(storageKey);
        ArgumentNullException.ThrowIfNull(contentType);

        if (id == Guid.Empty)
        {
            throw new DomainException("شناسه رسانه معتبر نیست.");
        }

        if (uploadedByUserId == Guid.Empty)
        {
            throw new DomainException("شناسه بارگذار الزامی است.");
        }

        if (sizeBytes <= 0)
        {
            throw new DomainException("اندازه فایل باید مثبت باشد.");
        }

        if (width <= 0 || height <= 0)
        {
            throw new DomainException("ابعاد تصویر باید مثبت باشد.");
        }

        if (string.IsNullOrWhiteSpace(publicUrl)
            || publicUrl.Contains('\\', StringComparison.Ordinal)
            || publicUrl.Contains("..", StringComparison.Ordinal)
            || !(publicUrl.StartsWith("/", StringComparison.Ordinal)
                 || publicUrl.StartsWith("http://", StringComparison.OrdinalIgnoreCase)
                 || publicUrl.StartsWith("https://", StringComparison.OrdinalIgnoreCase)))
        {
            throw new DomainException("نشانی عمومی رسانه معتبر نیست.");
        }

        if (publicUrl.StartsWith("file:", StringComparison.OrdinalIgnoreCase)
            || LooksLikeFilesystemPath(publicUrl))
        {
            throw new DomainException("نشانی عمومی نباید مسیر فایل‌سیستم باشد.");
        }

        var asset = new MediaAsset(id)
        {
            OriginalFileName = originalFileName.Value,
            StorageKey = storageKey.Value,
            ContentType = contentType.Value,
            SizeBytes = sizeBytes,
            Width = width,
            Height = height,
            PublicUrl = publicUrl.Trim(),
            UploadedByUserId = uploadedByUserId,
            CreatedAtUtc = createdAtUtc,
            UpdatedAtUtc = createdAtUtc,
            Status = MediaAssetStatus.Active,
            AltText = NormalizeOptional(altText, maxAltTextLength, "متن جایگزین"),
            Caption = NormalizeOptional(caption, maxCaptionLength, "توضیح"),
        };

        return asset;
    }

    private static bool LooksLikeFilesystemPath(string url) =>
        url.Length >= 2 && char.IsLetter(url[0]) && url[1] == ':';

    private static string? NormalizeOptional(string? value, int maxLength, string fieldLabel)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var trimmed = value.Trim();
        if (trimmed.Length > maxLength)
        {
            throw new DomainException($"{fieldLabel} نباید بیش از {maxLength} نویسه باشد.");
        }

        return trimmed;
    }
}

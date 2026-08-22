namespace HelpDev.Modules.Media.Application.Assets;

public sealed record MediaAssetDto(
    Guid Id,
    string OriginalFileName,
    string ContentType,
    long SizeBytes,
    int Width,
    int Height,
    string PublicUrl,
    string? AltText,
    string? Caption,
    Guid UploadedByUserId,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc,
    string Status);

public sealed record MediaAssetListItemDto(
    Guid Id,
    string OriginalFileName,
    string ContentType,
    long SizeBytes,
    int Width,
    int Height,
    string PublicUrl,
    string? AltText,
    Guid UploadedByUserId,
    DateTime CreatedAtUtc,
    string Status);

public sealed class MediaAssetListQuery
{
    public const int DefaultPage = 1;
    public const int DefaultPageSize = 24;
    public const int MaxPageSize = 100;

    public int Page { get; init; } = DefaultPage;

    public int PageSize { get; init; } = DefaultPageSize;

    public string? Search { get; init; }

    public string? ContentType { get; init; }

    /// <summary>Admin-only filter. Writers are always scoped to their own user id.</summary>
    public Guid? UploadedByUserId { get; init; }

    public static MediaAssetListQuery Create(
        int? page = null,
        int? pageSize = null,
        string? search = null,
        string? contentType = null,
        Guid? uploadedByUserId = null)
    {
        var normalizedPage = page is null or < 1 ? DefaultPage : page.Value;
        var normalizedPageSize = pageSize is null or < 1
            ? DefaultPageSize
            : Math.Min(pageSize.Value, MaxPageSize);

        return new MediaAssetListQuery
        {
            Page = normalizedPage,
            PageSize = normalizedPageSize,
            Search = string.IsNullOrWhiteSpace(search) ? null : search.Trim(),
            ContentType = string.IsNullOrWhiteSpace(contentType) ? null : contentType.Trim().ToLowerInvariant(),
            UploadedByUserId = uploadedByUserId,
        };
    }
}

/// <summary>Application-safe upload payload. Stream ownership remains with the caller.</summary>
public sealed class UploadMediaAssetRequest
{
    public required Stream Content { get; init; }

    public required string OriginalFileName { get; init; }

    public required string DeclaredContentType { get; init; }

    public required long SizeBytes { get; init; }

    public string? AltText { get; init; }

    public string? Caption { get; init; }
}

public sealed class UpdateMediaAssetRequest
{
    public string? AltText { get; init; }

    public string? Caption { get; init; }
}

public sealed record MediaLibraryConfigDto(
    long MaxUploadBytes,
    int MaxWidth,
    int MaxHeight,
    IReadOnlyList<string> AllowedContentTypes,
    int MaxAltTextLength,
    int MaxCaptionLength);

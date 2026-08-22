using Asp.Versioning;
using HelpDev.API.Contracts;
using HelpDev.API.Extensions;
using HelpDev.API.OpenApi;
using HelpDev.API.Security;
using HelpDev.Modules.Identity.Application.Auth;
using HelpDev.Modules.Media.Application.Assets;
using HelpDev.Modules.Media.Application.Common;
using HelpDev.Modules.Media.Application.Options;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;

namespace HelpDev.API.Controllers;

/// <summary>
/// Admin Media Library. Routed under /api/v1/admin/media.
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/admin/media")]
[Route("api/v{version:apiVersion}/admin/media")]
[Authorize(Policy = AuthorizationPolicies.WriterOrAdmin)]
[ApiAudience(ApiAudiences.Admin)]
[Tags(ApiTags.Media)]
[EnableRateLimiting(RateLimitPolicyNames.AdminMutation)]
public sealed class MediaManagementController : ControllerBase
{
    private readonly IMediaAssetService _mediaService;
    private readonly IMediaAssetQueries _mediaQueries;
    private readonly MediaOptions _options;

    public MediaManagementController(
        IMediaAssetService mediaService,
        IMediaAssetQueries mediaQueries,
        IOptions<MediaOptions> options)
    {
        _mediaService = mediaService;
        _mediaQueries = mediaQueries;
        _options = options.Value;
    }

    [HttpGet]
    [OpenApiOperationId("MediaManagement_List")]
    [OpenApiSummary("List media assets", "Paged media library list with ownership scoping.")]
    [ProducesResponseType(typeof(PagedResult<MediaAssetListItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<PagedResult<MediaAssetListItemDto>>> List(
        [FromQuery] string? search,
        [FromQuery] string? contentType,
        [FromQuery] int? page,
        [FromQuery] int? pageSize,
        CancellationToken cancellationToken)
    {
        if (!TryResolveActor(out var actor, out var unauthorized))
        {
            return unauthorized;
        }

        var query = MediaAssetListQuery.Create(page, pageSize, search, contentType);
        var result = await _mediaQueries.GetPagedAsync(actor, query, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [OpenApiOperationId("MediaManagement_GetById")]
    [OpenApiSummary("Get media asset", "Returns media detail with ownership masking.")]
    [ProducesResponseType(typeof(MediaAssetDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<MediaAssetDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        if (!TryResolveActor(out var actor, out var unauthorized))
        {
            return unauthorized;
        }

        var asset = await _mediaService.GetManagedByIdAsync(actor, id, cancellationToken);
        return Ok(asset);
    }

    [HttpGet("config")]
    [OpenApiOperationId("MediaManagement_GetConfig")]
    [OpenApiSummary("Media library limits", "Returns upload limits used by the editor and media picker.")]
    [ProducesResponseType(typeof(MediaLibraryConfigDto), StatusCodes.Status200OK)]
    public ActionResult<MediaLibraryConfigDto> GetConfig()
    {
        return Ok(new MediaLibraryConfigDto(
            _options.MaxUploadBytes,
            _options.MaxWidth,
            _options.MaxHeight,
            _options.AllowedContentTypes,
            _options.MaxAltTextLength,
            _options.MaxCaptionLength));
    }

    [HttpPut("{id:guid}")]
    [OpenApiOperationId("MediaManagement_UpdateMetadata")]
    [OpenApiSummary("Update media metadata", "Updates alt text and caption for an owned media asset.")]
    [ProducesResponseType(typeof(MediaAssetDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<MediaAssetDto>> UpdateMetadata(
        Guid id,
        [FromBody] UpdateMediaAssetRequest request,
        CancellationToken cancellationToken)
    {
        if (!TryResolveActor(out var actor, out var unauthorized))
        {
            return unauthorized;
        }

        var asset = await _mediaService.UpdateMetadataAsync(actor, id, request, cancellationToken);
        return Ok(asset);
    }

    [HttpDelete("{id:guid}")]
    [OpenApiOperationId("MediaManagement_Delete")]
    [OpenApiSummary("Delete media asset", "Archives the asset and removes stored bytes. Requires confirmation in the UI.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        if (!TryResolveActor(out var actor, out var unauthorized))
        {
            return unauthorized;
        }

        await _mediaService.DeleteAsync(actor, id, cancellationToken);
        return NoContent();
    }

    [HttpPost]
    [OpenApiOperationId("MediaManagement_Upload")]
    [OpenApiSummary("Upload media asset", "Uploads a single image (JPEG/PNG/WebP). Signature-validated.")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(20 * 1024 * 1024)]
    [RequestFormLimits(MultipartBodyLengthLimit = 20 * 1024 * 1024)]
    [ProducesResponseType(typeof(MediaAssetDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status413PayloadTooLarge)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status415UnsupportedMediaType)]
    public async Task<ActionResult<MediaAssetDto>> Upload(
        IFormFile? file,
        [FromForm] string? altText,
        [FromForm] string? caption,
        CancellationToken cancellationToken)
    {
        if (!TryResolveActor(out var actor, out var unauthorized))
        {
            return unauthorized;
        }

        if (file is null || file.Length <= 0)
        {
            throw new MediaException("فایل الزامی است.", MediaErrorCodes.Validation);
        }

        if (file.Length > _options.MaxUploadBytes)
        {
            throw new MediaException(
                $"حجم فایل از حداکثر مجاز ({_options.MaxUploadBytes} بایت) بیشتر است.",
                MediaErrorCodes.PayloadTooLarge);
        }

        await using var stream = file.OpenReadStream();
        var request = new UploadMediaAssetRequest
        {
            Content = stream,
            OriginalFileName = file.FileName,
            DeclaredContentType = file.ContentType,
            SizeBytes = file.Length,
            AltText = altText,
            Caption = caption,
        };

        var asset = await _mediaService.UploadAsync(actor, request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = asset.Id, version = "1.0" }, asset);
    }

    private bool TryResolveActor(
        out MediaManagementActor actor,
        out ActionResult unauthorized)
    {
        var userId = User.GetUserId();
        if (userId is null)
        {
            actor = null!;
            unauthorized = Unauthorized();
            return false;
        }

        actor = new MediaManagementActor(
            userId.Value,
            canManageAllAssets: User.IsInRole(AppRoles.Admin));
        unauthorized = null!;
        return true;
    }
}

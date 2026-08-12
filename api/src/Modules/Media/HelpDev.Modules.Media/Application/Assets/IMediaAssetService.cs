using HelpDev.Modules.Media.Application.Common;

namespace HelpDev.Modules.Media.Application.Assets;

public interface IMediaAssetQueries
{
    Task<PagedResult<MediaAssetListItemDto>> GetPagedAsync(
        MediaManagementActor actor,
        MediaAssetListQuery query,
        CancellationToken cancellationToken = default);

    Task<MediaAssetDto?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);
}

public interface IMediaAssetService
{
    Task<MediaAssetDto> UploadAsync(
        MediaManagementActor actor,
        UploadMediaAssetRequest request,
        CancellationToken cancellationToken = default);

    Task<MediaAssetDto> GetManagedByIdAsync(
        MediaManagementActor actor,
        Guid id,
        CancellationToken cancellationToken = default);
}

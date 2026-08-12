using HelpDev.Modules.Media.Application.Assets;
using HelpDev.Modules.Media.Application.Common;

namespace HelpDev.API.Tests.Fakes;

internal sealed class FakeMediaAssetService : IMediaAssetService
{
    public MediaManagementActor? LastActor { get; private set; }

    public Guid? LastId { get; private set; }

    public UploadMediaAssetRequest? LastUpload { get; private set; }

    public string? LastOperation { get; private set; }

    public MediaAssetDto DetailToReturn { get; set; } = CreateSample();

    public Task<MediaAssetDto> UploadAsync(
        MediaManagementActor actor,
        UploadMediaAssetRequest request,
        CancellationToken cancellationToken = default)
    {
        LastActor = actor;
        LastUpload = request;
        LastOperation = nameof(UploadAsync);
        return Task.FromResult(DetailToReturn);
    }

    public Task<MediaAssetDto> GetManagedByIdAsync(
        MediaManagementActor actor,
        Guid id,
        CancellationToken cancellationToken = default)
    {
        LastActor = actor;
        LastId = id;
        LastOperation = nameof(GetManagedByIdAsync);
        return Task.FromResult(DetailToReturn);
    }

    internal static MediaAssetDto CreateSample() =>
        new(
            Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
            "sample.png",
            "image/png",
            100,
            10,
            10,
            "/media/2026/07/sample.png",
            null,
            null,
            Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"),
            DateTime.UtcNow,
            DateTime.UtcNow,
            "Active");
}

internal sealed class FakeMediaAssetQueries : IMediaAssetQueries
{
    public MediaAssetListQuery? LastQuery { get; private set; }

    public MediaManagementActor? LastActor { get; private set; }

    public PagedResult<MediaAssetListItemDto> Result { get; set; } =
        new([], 1, 24, 0);

    public Task<PagedResult<MediaAssetListItemDto>> GetPagedAsync(
        MediaManagementActor actor,
        MediaAssetListQuery query,
        CancellationToken cancellationToken = default)
    {
        LastActor = actor;
        LastQuery = query;
        return Task.FromResult(Result);
    }

    public Task<MediaAssetDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        Task.FromResult<MediaAssetDto?>(null);
}

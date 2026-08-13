using HelpDev.Modules.Media.Application.Assets;
using HelpDev.Modules.Media.Application.Options;
using HelpDev.Modules.Media.Application.Persistence;
using HelpDev.Modules.Media.Application.Storage;
using HelpDev.Modules.Media.Application.Validation;
using HelpDev.Modules.Media.Domain.Assets;
using HelpDev.Modules.Media.Domain.ValueObjects;
using HelpDev.Modules.Media.Infrastructure.Inspection;
using HelpDev.Modules.Media.Infrastructure.Storage;
using HelpDev.SharedKernel.Exceptions;
using HelpDev.SharedKernel.Time;
using HelpDev.Modules.Media.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace HelpDev.Media.Tests;

public sealed class MediaAssetDomainTests
{
    [Fact]
    public void Create_valid_asset_succeeds()
    {
        var id = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var asset = MediaAsset.Create(
            id,
            MediaFileName.Create("photo.png"),
            MediaStorageKey.Create("2026/07/abc.jpg"),
            MediaContentType.Create(MediaContentType.Png),
            sizeBytes: 100,
            width: 10,
            height: 20,
            publicUrl: "/media/2026/07/abc.jpg",
            uploadedByUserId: userId,
            createdAtUtc: DateTime.UtcNow,
            altText: "Alt",
            caption: null);

        Assert.Equal(id, asset.Id);
        Assert.Equal("photo.png", asset.OriginalFileName);
        Assert.Equal(MediaContentType.Png, asset.ContentType);
    }

    [Fact]
    public void MediaFileName_strips_path_segments()
    {
        var name = MediaFileName.Create(@"C:\fake\path\image.png");
        Assert.Equal("image.png", name.Value);
    }

    [Fact]
    public void MediaContentType_rejects_svg()
    {
        Assert.Throws<DomainException>(() => MediaContentType.Create("image/svg+xml"));
    }

    [Fact]
    public void Create_rejects_filesystem_public_url()
    {
        Assert.Throws<DomainException>(() => MediaAsset.Create(
            Guid.NewGuid(),
            MediaFileName.Create("x.png"),
            MediaStorageKey.Create("2026/07/x.png"),
            MediaContentType.Create(MediaContentType.Png),
            1,
            1,
            1,
            @"C:\secret\file.png",
            Guid.NewGuid(),
            DateTime.UtcNow));
    }
}

public sealed class LocalMediaStorageTests
{
    [Fact]
    public async Task Store_and_read_stays_inside_root()
    {
        var root = Path.Combine(Path.GetTempPath(), "helpdev-media-test-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);

        try
        {
            var options = Options.Create(new MediaOptions { LocalStorageRoot = root });
            var storage = new LocalMediaStorage(options, NullLogger<LocalMediaStorage>.Instance);
            var key = "2026/07/" + Guid.NewGuid().ToString("N") + ".png";
            var bytes = ImageTestFixtures.CreatePngBytes();
            await using var input = new MemoryStream(bytes);
            await storage.StoreAsync(input, key, MediaContentType.Png);

            Assert.True(await storage.ExistsAsync(key));
            await using var output = await storage.OpenReadAsync(key);
            using var ms = new MemoryStream();
            await output.CopyToAsync(ms);
            Assert.Equal(bytes.Length, ms.Length);
        }
        finally
        {
            if (Directory.Exists(root))
            {
                Directory.Delete(root, recursive: true);
            }
        }
    }

    [Fact]
    public async Task Store_rejects_traversal_key()
    {
        var root = Path.Combine(Path.GetTempPath(), "helpdev-media-test-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        try
        {
            var storage = new LocalMediaStorage(
                Options.Create(new MediaOptions { LocalStorageRoot = root }),
                NullLogger<LocalMediaStorage>.Instance);
            await using var input = new MemoryStream([1, 2, 3]);
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                storage.StoreAsync(input, "../escape.png", MediaContentType.Png));
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }
}

public sealed class ImageFileInspectorTests
{
    private readonly ImageFileInspector _inspector = new(Options.Create(new MediaOptions()));

    [Fact]
    public async Task Valid_png_passes()
    {
        await using var stream = new MemoryStream(ImageTestFixtures.CreatePngBytes());
        var result = await _inspector.InspectAsync(stream, "image/png");
        Assert.Equal(MediaContentType.Png, result.DetectedContentType);
        Assert.Equal(".png", result.SafeExtension);
    }

    [Fact]
    public async Task Valid_jpeg_passes()
    {
        await using var stream = new MemoryStream(ImageTestFixtures.CreateJpegBytes());
        var result = await _inspector.InspectAsync(stream, "image/jpeg");
        Assert.Equal(MediaContentType.Jpeg, result.DetectedContentType);
    }

    [Fact]
    public async Task Fake_jpeg_extension_rejected()
    {
        await using var stream = new MemoryStream(ImageTestFixtures.FakeJpegExtensionBytes());
        await Assert.ThrowsAsync<MediaException>(() => _inspector.InspectAsync(stream, "image/jpeg"));
    }

    [Fact]
    public async Task Svg_rejected()
    {
        await using var stream = new MemoryStream(ImageTestFixtures.SvgBytes());
        await Assert.ThrowsAsync<MediaException>(() => _inspector.InspectAsync(stream, "image/svg+xml"));
    }

    [Fact]
    public async Task Png_with_xmp_xml_in_header_passes()
    {
        var bytes = ImageTestFixtures.CreatePngBytesWithXmlMetadata();
        var header = System.Text.Encoding.ASCII.GetString(bytes, 0, Math.Min(512, bytes.Length));
        Assert.Contains("<?xml", header, StringComparison.OrdinalIgnoreCase);

        await using var stream = new MemoryStream(bytes);
        var result = await _inspector.InspectAsync(stream, "image/png");
        Assert.Equal(MediaContentType.Png, result.DetectedContentType);
        Assert.Equal(".png", result.SafeExtension);
    }
}

public sealed class MediaAssetServiceTests
{
    [Fact]
    public async Task Upload_persists_once_and_returns_dto()
    {
        var root = Path.Combine(Path.GetTempPath(), "helpdev-media-upload-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        try
        {
            var options = Options.Create(new MediaOptions { LocalStorageRoot = root, PublicBasePath = "/media" });
            var storage = new LocalMediaStorage(options, NullLogger<LocalMediaStorage>.Instance);
            var inspector = new ImageFileInspector(options);
            var db = new FakeMediaDbContext();
            var repo = new FakeMediaRepository(db);
            var queries = new FakeMediaQueries(db);
            var service = new MediaAssetService(
                repo,
                queries,
                storage,
                inspector,
                db,
                new FixedClock(new DateTime(2026, 7, 22, 12, 0, 0, DateTimeKind.Utc)),
                options,
                NullLogger<MediaAssetService>.Instance);

            var actor = new MediaManagementActor(Guid.NewGuid(), canManageAllAssets: false);
            await using var stream = new MemoryStream(ImageTestFixtures.CreatePngBytes());
            var dto = await service.UploadAsync(
                actor,
                new UploadMediaAssetRequest
                {
                    Content = stream,
                    OriginalFileName = "test.png",
                    DeclaredContentType = "image/png",
                    SizeBytes = stream.Length,
                    AltText = "Alt",
                },
                CancellationToken.None);

            Assert.Equal("test.png", dto.OriginalFileName);
            Assert.StartsWith("/media/", dto.PublicUrl);
            Assert.Equal(1, db.SaveCount);
            Assert.DoesNotContain("StorageKey", typeof(MediaAssetDto).GetProperties().Select(p => p.Name));
        }
        finally
        {
            if (Directory.Exists(root))
            {
                Directory.Delete(root, recursive: true);
            }
        }
    }

    [Fact]
    public async Task Cross_owner_writer_gets_not_found()
    {
        var db = new FakeMediaDbContext();
        var ownerId = Guid.NewGuid();
        var asset = MediaAsset.Create(
            Guid.NewGuid(),
            MediaFileName.Create("a.png"),
            MediaStorageKey.Create("2026/07/a.png"),
            MediaContentType.Create(MediaContentType.Png),
            10,
            1,
            1,
            "/media/2026/07/a.png",
            ownerId,
            DateTime.UtcNow);
        db.MediaAssets.Add(asset);
        await db.SaveChangesAsync();

        var service = new MediaAssetService(
            new FakeMediaRepository(db),
            new FakeMediaQueries(db),
            new NoOpStorage(),
            new ImageFileInspector(Options.Create(new MediaOptions())),
            db,
            new FixedClock(DateTime.UtcNow),
            Options.Create(new MediaOptions()),
            NullLogger<MediaAssetService>.Instance);

        var ex = await Assert.ThrowsAsync<MediaException>(() =>
            service.GetManagedByIdAsync(new MediaManagementActor(Guid.NewGuid(), false), asset.Id, CancellationToken.None));
        Assert.Equal(MediaErrorCodes.NotFound, ex.Code);
    }

    private sealed class FixedClock : IDateTimeProvider
    {
        public FixedClock(DateTime utc) => UtcNow = utc;
        public DateTime UtcNow { get; }
    }

    private sealed class FakeMediaDbContext : DbContext, IMediaDbContext
    {
        public int SaveCount { get; private set; }

        public FakeMediaDbContext()
            : base(new DbContextOptionsBuilder<FakeMediaDbContext>()
                .UseInMemoryDatabase("media-test-" + Guid.NewGuid().ToString("N"))
                .Options)
        {
        }

        public DbSet<MediaAsset> MediaAssets => Set<MediaAsset>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new MediaAssetConfiguration());
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            SaveCount++;
            return base.SaveChangesAsync(cancellationToken);
        }
    }

    private sealed class FakeMediaRepository(FakeMediaDbContext db) : IMediaAssetRepository
    {
        public Task AddAsync(MediaAsset asset, CancellationToken cancellationToken = default)
        {
            db.MediaAssets.Add(asset);
            return Task.CompletedTask;
        }

        public Task<MediaAsset?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
            Task.FromResult(db.MediaAssets.FirstOrDefault(a => a.Id == id));
    }

    private sealed class FakeMediaQueries(FakeMediaDbContext db) : IMediaAssetQueries
    {
        public Task<Modules.Media.Application.Common.PagedResult<MediaAssetListItemDto>> GetPagedAsync(
            MediaManagementActor actor,
            MediaAssetListQuery query,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(Modules.Media.Application.Common.PagedResult<MediaAssetListItemDto>.Empty(1, 20));

        public Task<MediaAssetDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var asset = db.MediaAssets.FirstOrDefault(a => a.Id == id);
            return Task.FromResult(asset is null ? null : MediaAssetService.Map(asset));
        }
    }

    private sealed class NoOpStorage : IMediaStorage
    {
        public Task StoreAsync(Stream content, string storageKey, string contentType, CancellationToken cancellationToken = default) =>
            Task.CompletedTask;

        public Task<Stream> OpenReadAsync(string storageKey, CancellationToken cancellationToken = default) =>
            Task.FromResult<Stream>(new MemoryStream());

        public Task<bool> ExistsAsync(string storageKey, CancellationToken cancellationToken = default) =>
            Task.FromResult(true);

        public Task DeleteAsync(string storageKey, CancellationToken cancellationToken = default) =>
            Task.CompletedTask;
    }
}

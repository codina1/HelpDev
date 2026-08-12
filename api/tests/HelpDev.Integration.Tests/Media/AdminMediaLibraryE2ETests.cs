using HelpDev.Infrastructure.Persistence;
using HelpDev.Integration.Tests.Helpers;
using HelpDev.Modules.Identity.Domain.Entities;
using HelpDev.Modules.Identity.Domain.Enums;
using HelpDev.Modules.Media.Application.Assets;
using HelpDev.Modules.Media.Application.Storage;
using HelpDev.Testing.PostgreSQL;
using HelpDev.Testing.PostgreSQL.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace HelpDev.Integration.Tests.Media;

[Collection(PostgreSqlCollection.Name)]
public sealed class AdminMediaLibraryE2ETests : IntegrationTestClassBase
{
    public AdminMediaLibraryE2ETests(PostgreSqlFixture fixture)
        : base(fixture)
    {
    }

    [PostgreSqlFact]
    public async Task Upload_persists_metadata_and_file_and_public_route_serves_bytes()
    {
        var storageRoot = Path.Combine(Path.GetTempPath(), "helpdev-media-int-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(storageRoot);

        try
        {
            await using var factory = new HelpDevWebApplicationFactory(
                Fixture.ConnectionString,
                configurationOverrides: new Dictionary<string, string?>
                {
                    ["Media:LocalStorageRoot"] = storageRoot,
                });

            var authorId = await SeedUserAsync(factory);
            var png = HelpDev.Media.Tests.ImageTestFixtures.CreatePngBytes();

            MediaAssetDto uploaded;
            await using (var scope = factory.Services.CreateAsyncScope())
            {
                var service = scope.ServiceProvider.GetRequiredService<IMediaAssetService>();
                await using var stream = new MemoryStream(png);
                uploaded = await service.UploadAsync(
                    new MediaManagementActor(authorId, canManageAllAssets: false),
                    new UploadMediaAssetRequest
                    {
                        Content = stream,
                        OriginalFileName = "integration.png",
                        DeclaredContentType = "image/png",
                        SizeBytes = png.Length,
                        AltText = "Integration alt",
                    },
                    CancellationToken.None);
            }

            Assert.StartsWith("/media/", uploaded.PublicUrl);
            Assert.Equal("image/png", uploaded.ContentType);

            await using (var scope = factory.Services.CreateAsyncScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var row = await context.MediaAssets.AsNoTracking().SingleAsync(a => a.Id == uploaded.Id);
                Assert.Equal("integration.png", row.OriginalFileName);
                Assert.DoesNotContain(storageRoot, row.PublicUrl, StringComparison.OrdinalIgnoreCase);

                var storage = scope.ServiceProvider.GetRequiredService<IMediaStorage>();
                Assert.True(await storage.ExistsAsync(row.StorageKey, CancellationToken.None));

                var expectedPhysical = Path.Combine(
                    storageRoot,
                    row.StorageKey.Replace('/', Path.DirectorySeparatorChar));
                Assert.True(File.Exists(expectedPhysical));
            }

            var client = factory.CreateClient();
            var response = await client.GetAsync(uploaded.PublicUrl);
            Assert.True(response.IsSuccessStatusCode);
            Assert.Equal("image/png", response.Content.Headers.ContentType?.MediaType);
            var body = await response.Content.ReadAsByteArrayAsync();
            Assert.True(body.Length > 0);

            await using (var scope = factory.Services.CreateAsyncScope())
            {
                var service = scope.ServiceProvider.GetRequiredService<IMediaAssetService>();
                var ex = await Assert.ThrowsAsync<MediaException>(() =>
                    service.GetManagedByIdAsync(
                        new MediaManagementActor(Guid.NewGuid(), canManageAllAssets: false),
                        uploaded.Id,
                        CancellationToken.None));
                Assert.Equal(MediaErrorCodes.NotFound, ex.Code);
            }

            await using (var scope = factory.Services.CreateAsyncScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var migrations = await context.Database.GetAppliedMigrationsAsync();
                Assert.Equal(PostgreSqlDatabaseHelper.ExpectedMigrationCount, migrations.Count());
            }
        }
        finally
        {
            if (Directory.Exists(storageRoot))
            {
                Directory.Delete(storageRoot, recursive: true);
            }
        }
    }

    [PostgreSqlFact]
    public async Task Invalid_svg_upload_does_not_persist_row_or_file()
    {
        var storageRoot = Path.Combine(Path.GetTempPath(), "helpdev-media-int-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(storageRoot);

        try
        {
            await using var factory = new HelpDevWebApplicationFactory(
                Fixture.ConnectionString,
                configurationOverrides: new Dictionary<string, string?> { ["Media:LocalStorageRoot"] = storageRoot });

            var authorId = await SeedUserAsync(factory);
            var svg = HelpDev.Media.Tests.ImageTestFixtures.SvgBytes();

            await using var scope = factory.Services.CreateAsyncScope();
            var service = scope.ServiceProvider.GetRequiredService<IMediaAssetService>();
            await using var stream = new MemoryStream(svg);

            await Assert.ThrowsAsync<MediaException>(() => service.UploadAsync(
                new MediaManagementActor(authorId, canManageAllAssets: false),
                new UploadMediaAssetRequest
                {
                    Content = stream,
                    OriginalFileName = "evil.svg",
                    DeclaredContentType = "image/svg+xml",
                    SizeBytes = svg.Length,
                },
                CancellationToken.None));

            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            Assert.Equal(0, await context.MediaAssets.CountAsync(a => a.OriginalFileName == "evil.svg"));
            Assert.False(Directory.EnumerateFiles(storageRoot, "*", SearchOption.AllDirectories).Any());
        }
        finally
        {
            if (Directory.Exists(storageRoot))
            {
                Directory.Delete(storageRoot, recursive: true);
            }
        }
    }

    private static async Task<Guid> SeedUserAsync(HelpDevWebApplicationFactory factory)
    {
        var userId = Guid.NewGuid();
        await using var scope = factory.Services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        context.Users.Add(new User
        {
            Id = userId,
            Mobile = TestIds.Truncate($"09{Guid.NewGuid():N}", 11),
            FullName = "Media Author",
            FirstName = "Media",
            LastName = "Author",
            Role = UserRole.User,
            CreatedAt = DateTime.UtcNow,
        });
        await context.SaveChangesAsync();
        return userId;
    }
}

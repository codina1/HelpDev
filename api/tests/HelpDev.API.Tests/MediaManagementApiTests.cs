using HelpDev.API.Controllers;
using HelpDev.API.Extensions;
using HelpDev.API.Tests.Fakes;
using HelpDev.Modules.Identity.Application.Auth;
using HelpDev.Modules.Media.Application.Assets;
using HelpDev.Modules.Media.Application.Common;
using HelpDev.Modules.Media.Application.Storage;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace HelpDev.API.Tests;

public sealed class MediaManagementApiTests
{
    [Fact]
    public void Controller_requires_writer_or_admin_policy()
    {
        var attribute = Assert.Single(
            typeof(MediaManagementController)
                .GetCustomAttributes(typeof(AuthorizeAttribute), inherit: true)
                .Cast<AuthorizeAttribute>());

        Assert.Equal(AuthorizationPolicies.WriterOrAdmin, attribute.Policy);
    }

    [Fact]
    public void Controller_has_no_delete_action()
    {
        var methods = typeof(MediaManagementController).GetMethods()
            .Where(m => m.GetCustomAttributes(typeof(HttpDeleteAttribute), false).Length > 0)
            .ToArray();
        Assert.Empty(methods);
    }

    [Fact]
    public async Task List_scopes_writer_to_own_assets()
    {
        var queries = new FakeMediaAssetQueries();
        var controller = CreateController(new FakeMediaAssetService(), queries);
        var userId = Guid.NewGuid();
        ControllerTestHelper.SetUser(controller, userId, AppRoles.Writer);

        await controller.List(null, null, null, null, CancellationToken.None);

        Assert.NotNull(queries.LastActor);
        Assert.Equal(userId, queries.LastActor!.UserId);
        Assert.False(queries.LastActor.CanManageAllAssets);
    }

    [Fact]
    public async Task GetById_forwards_actor_and_id()
    {
        var service = new FakeMediaAssetService();
        var controller = CreateController(service, new FakeMediaAssetQueries());
        var userId = Guid.NewGuid();
        ControllerTestHelper.SetUser(controller, userId, AppRoles.Admin);
        var id = Guid.NewGuid();

        var result = await controller.GetById(id, CancellationToken.None);

        Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(id, service.LastId);
        Assert.True(service.LastActor!.CanManageAllAssets);
        Assert.Equal(nameof(IMediaAssetService.GetManagedByIdAsync), service.LastOperation);
    }

    [Fact]
    public void MediaAssetDto_does_not_expose_storage_key()
    {
        var props = typeof(MediaAssetDto).GetProperties().Select(p => p.Name).ToArray();
        Assert.DoesNotContain("StorageKey", props);
        Assert.DoesNotContain(props, p => p.Contains("Path", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Upload_endpoint_is_post_without_id_segment()
    {
        var method = typeof(MediaManagementController).GetMethod(nameof(MediaManagementController.Upload));
        Assert.NotNull(method);
        Assert.NotNull(method!.GetCustomAttributes(typeof(HttpPostAttribute), false).Single());
    }

    [Fact]
    public async Task Public_media_allows_cross_origin_embedding()
    {
        var controller = new PublicMediaController(
            new ExistingMediaStorage(),
            Options.Create(new Modules.Media.Application.Options.MediaOptions()));
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext(),
        };

        var result = await controller.Get(2026, 8, "sample.png", CancellationToken.None);

        Assert.IsType<FileStreamResult>(result);
        Assert.Equal("cross-origin", controller.Response.Headers.CrossOriginResourcePolicy);
    }

    private static MediaManagementController CreateController(
        IMediaAssetService service,
        IMediaAssetQueries queries) =>
        new(service, queries, Options.Create(new Modules.Media.Application.Options.MediaOptions()));

    private sealed class ExistingMediaStorage : IMediaStorage
    {
        public Task StoreAsync(
            Stream content,
            string storageKey,
            string contentType,
            CancellationToken cancellationToken = default) =>
            Task.CompletedTask;

        public Task<Stream> OpenReadAsync(
            string storageKey,
            CancellationToken cancellationToken = default) =>
            Task.FromResult<Stream>(new MemoryStream([1, 2, 3]));

        public Task<bool> ExistsAsync(
            string storageKey,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(true);

        public Task DeleteAsync(
            string storageKey,
            CancellationToken cancellationToken = default) =>
            Task.CompletedTask;
    }
}

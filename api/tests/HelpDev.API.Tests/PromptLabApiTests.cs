using System.Reflection;
using System.Text.Json;
using HelpDev.API.Controllers;
using HelpDev.API.Filters;
using HelpDev.API.Tests.Fakes;
using HelpDev.Modules.Identity.Application.Auth;
using HelpDev.Modules.PromptLab.Application;
using HelpDev.Modules.PromptLab.Application.Catalog;
using HelpDev.Modules.PromptLab.Application.Favorites;
using HelpDev.Modules.PromptLab.Application.History;
using HelpDev.Modules.PromptLab.Application.Rendering;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;

namespace HelpDev.API.Tests;

public sealed class PromptLabApiTests
{
    [Theory]
    [InlineData(typeof(PromptLabAdminCategoriesController))]
    [InlineData(typeof(PromptLabAdminPromptsController))]
    public void Admin_controllers_require_AdminOnly(Type controllerType)
    {
        var attribute = Assert.Single(
            controllerType.GetCustomAttributes(typeof(AuthorizeAttribute), inherit: true)
                .Cast<AuthorizeAttribute>());
        Assert.Equal(AuthorizationPolicies.AdminOnly, attribute.Policy);
    }

    [Fact]
    public void Me_controller_requires_Authenticated()
    {
        var attribute = Assert.Single(
            typeof(PromptLabMeController).GetCustomAttributes(typeof(AuthorizeAttribute), inherit: true)
                .Cast<AuthorizeAttribute>());
        Assert.Equal(AuthorizationPolicies.Authenticated, attribute.Policy);
    }

    [Fact]
    public void Catalog_controller_depends_on_render_service_and_catalog_queries()
    {
        var parameters = typeof(PromptLabCatalogController).GetConstructors().Single().GetParameters();
        Assert.Contains(parameters, p => p.ParameterType == typeof(IPromptRenderService));
        Assert.Contains(parameters, p => p.ParameterType == typeof(IPromptCatalogQueries));
        Assert.DoesNotContain(
            parameters,
            p => p.ParameterType.Name.Contains("Repository", StringComparison.Ordinal)
                || p.ParameterType.Name.Contains("DbContext", StringComparison.Ordinal));
    }

    [Fact]
    public async Task Catalog_and_render_forward_to_services()
    {
        var catalog = new FakeCatalogQueries();
        var render = new FakeRenderService();
        var controller = new PromptLabCatalogController(catalog, render);
        ControllerTestHelper.SetUser(controller, userId: null);

        Assert.IsType<OkObjectResult>((await controller.GetCategories(CancellationToken.None)).Result);
        Assert.IsType<OkObjectResult>((await controller.GetPrompts(null, null, null, 1, 20, CancellationToken.None)).Result);

        var values = new Dictionary<string, JsonElement>(StringComparer.OrdinalIgnoreCase)
        {
            ["code"] = JsonSerializer.SerializeToElement("hi"),
        };
        var renderResult = await controller.Render(
            "code-review",
            new RenderPromptRequest(values),
            CancellationToken.None);
        Assert.IsType<OkObjectResult>(renderResult.Result);
        Assert.Equal("code-review", render.LastSlug);
    }

    [Fact]
    public async Task Favorites_use_authenticated_user_id_not_body()
    {
        var favorites = new FakeFavoriteService();
        var history = new FakeHistoryQueries();
        var controller = new PromptLabMeController(favorites, history);
        var userId = Guid.NewGuid();
        ControllerTestHelper.SetUser(controller, userId);

        await controller.AddFavorite(Guid.NewGuid(), CancellationToken.None);
        await controller.ListFavorites(CancellationToken.None);

        Assert.Equal(userId, favorites.LastUserId);
        Assert.Null(typeof(PromptFavoriteDto).GetProperty("UserId"));
    }

    [Theory]
    [InlineData(PromptLabApplicationErrorCodes.PromptNotFound, StatusCodes.Status404NotFound)]
    [InlineData(PromptLabApplicationErrorCodes.RenderRequiresAuthentication, StatusCodes.Status401Unauthorized)]
    [InlineData(PromptLabApplicationErrorCodes.PromptSlugDuplicate, StatusCodes.Status409Conflict)]
    [InlineData(PromptLabApplicationErrorCodes.RenderPatternTimeout, StatusCodes.Status400BadRequest)]
    public void Exception_filter_maps_codes(string code, int status)
    {
        var filter = new PromptLabExceptionFilter();
        var actionContext = new ActionContext(new DefaultHttpContext(), new RouteData(), new ActionDescriptor());
        var context = new ExceptionContext(actionContext, [])
        {
            Exception = new PromptLabException("failed", code),
        };

        filter.OnException(context);

        var result = Assert.IsType<ObjectResult>(context.Result);
        Assert.Equal(status, result.StatusCode);
        Assert.True(context.ExceptionHandled);
    }

    private sealed class FakeCatalogQueries : IPromptCatalogQueries
    {
        public Task<IReadOnlyList<PromptCategoryDto>> GetCategoriesAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<PromptCategoryDto>>([]);

        public Task<PromptCatalogPageDto> GetPromptsAsync(
            PromptCatalogFilter filter,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(new PromptCatalogPageDto(1, 20, 0, []));

        public Task<PromptDetailsDto?> GetBySlugAsync(
            string slug,
            Guid? currentUserId = null,
            CancellationToken cancellationToken = default) =>
            Task.FromResult<PromptDetailsDto?>(null);
    }

    private sealed class FakeRenderService : IPromptRenderService
    {
        public string? LastSlug { get; private set; }

        public Task<PromptRenderResultDto> RenderAsync(
            string slug,
            RenderPromptRequest request,
            Guid? userId = null,
            CancellationToken cancellationToken = default)
        {
            LastSlug = slug;
            return Task.FromResult(new PromptRenderResultDto(
                null,
                slug,
                1,
                true,
                "rendered",
                null,
                null,
                1,
                DateTime.UtcNow));
        }
    }

    private sealed class FakeFavoriteService : IPromptFavoriteService
    {
        public Guid? LastUserId { get; private set; }

        public Task AddAsync(Guid userId, Guid promptId, CancellationToken cancellationToken = default)
        {
            LastUserId = userId;
            return Task.CompletedTask;
        }

        public Task RemoveAsync(Guid userId, Guid promptId, CancellationToken cancellationToken = default)
        {
            LastUserId = userId;
            return Task.CompletedTask;
        }

        public Task<IReadOnlyList<PromptFavoriteDto>> GetUserFavoritesAsync(
            Guid userId,
            CancellationToken cancellationToken = default)
        {
            LastUserId = userId;
            return Task.FromResult<IReadOnlyList<PromptFavoriteDto>>([]);
        }
    }

    private sealed class FakeHistoryQueries : IPromptRenderHistoryQueries
    {
        public Task<PromptRenderHistoryPageDto> GetMyHistoryAsync(
            Guid userId,
            PromptRenderHistoryFilter filter,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(new PromptRenderHistoryPageDto(1, 20, 0, []));

        public Task<PromptRenderHistoryItemDto?> GetMyRenderAsync(
            Guid userId,
            Guid id,
            CancellationToken cancellationToken = default) =>
            Task.FromResult<PromptRenderHistoryItemDto?>(null);
    }
}

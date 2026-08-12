using System.Reflection;
using System.Text.Json;
using HelpDev.API.Controllers;
using HelpDev.API.Filters;
using HelpDev.API.Tests.Fakes;
using HelpDev.Modules.Identity.Application.Auth;
using HelpDev.Modules.Toolbox.Application.Catalog;
using HelpDev.Modules.Toolbox.Application.Execution;
using HelpDev.Modules.Toolbox.Application.Favorites;
using HelpDev.Modules.Toolbox.Application.History;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;

namespace HelpDev.API.Tests;

public sealed class ToolboxApiTests
{
    [Theory]
    [InlineData(typeof(ToolboxAdminCategoriesController))]
    [InlineData(typeof(ToolboxAdminToolsController))]
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
            typeof(ToolboxMeController).GetCustomAttributes(typeof(AuthorizeAttribute), inherit: true)
                .Cast<AuthorizeAttribute>());
        Assert.Equal(AuthorizationPolicies.Authenticated, attribute.Policy);
    }

    [Fact]
    public void Catalog_controller_depends_on_execution_service_abstraction()
    {
        var parameters = typeof(ToolboxCatalogController).GetConstructors().Single().GetParameters();
        Assert.Contains(parameters, p => p.ParameterType == typeof(IToolExecutionService));
        Assert.Contains(parameters, p => p.ParameterType == typeof(IToolCatalogQueries));
        Assert.DoesNotContain(
            parameters,
            p => p.ParameterType.Name.Contains("Executor", StringComparison.Ordinal)
                || p.ParameterType.Name.Contains("DbContext", StringComparison.Ordinal));
    }

    [Fact]
    public async Task Catalog_and_execute_forward_to_services()
    {
        var catalog = new FakeCatalogQueries();
        var execution = new FakeExecutionService();
        var controller = new ToolboxCatalogController(catalog, execution);
        ControllerTestHelper.SetUser(controller, userId: null);

        Assert.IsType<OkObjectResult>((await controller.GetCategories(CancellationToken.None)).Result);
        Assert.IsType<OkObjectResult>((await controller.GetTools(null, null, 1, 20, CancellationToken.None)).Result);

        using var doc = JsonDocument.Parse("""{"text":"{}"}""");
        var executeResult = await controller.Execute(
            "json-formatter",
            new ExecuteToolRequest(doc.RootElement.Clone()),
            CancellationToken.None);
        Assert.IsType<OkObjectResult>(executeResult.Result);
        Assert.Equal("json-formatter", execution.LastSlug);
    }

    [Fact]
    public async Task Favorites_use_authenticated_user_id_not_body()
    {
        var favorites = new FakeFavoriteService();
        var history = new FakeHistoryQueries();
        var controller = new ToolboxMeController(favorites, history);
        var userId = Guid.NewGuid();
        ControllerTestHelper.SetUser(controller, userId);

        await controller.AddFavorite(Guid.NewGuid(), CancellationToken.None);
        await controller.ListFavorites(CancellationToken.None);

        Assert.Equal(userId, favorites.LastUserId);
        Assert.Null(typeof(ToolFavoriteDto).GetProperty("UserId"));
    }

    [Theory]
    [InlineData(ToolboxApplicationErrorCodes.ToolNotFound, StatusCodes.Status404NotFound)]
    [InlineData(ToolboxApplicationErrorCodes.ToolRequiresAuthentication, StatusCodes.Status401Unauthorized)]
    [InlineData(ToolboxApplicationErrorCodes.ToolSlugDuplicate, StatusCodes.Status409Conflict)]
    [InlineData(ToolboxApplicationErrorCodes.RegexTimeout, StatusCodes.Status400BadRequest)]
    public void Exception_filter_maps_codes(string code, int status)
    {
        var filter = new ToolboxExceptionFilter();
        var actionContext = new ActionContext(new DefaultHttpContext(), new RouteData(), new ActionDescriptor());
        var context = new ExceptionContext(actionContext, [])
        {
            Exception = new ToolboxException("failed", code),
        };

        filter.OnException(context);

        var result = Assert.IsType<ObjectResult>(context.Result);
        Assert.Equal(status, result.StatusCode);
        Assert.True(context.ExceptionHandled);
    }

    private sealed class FakeCatalogQueries : IToolCatalogQueries
    {
        public Task<IReadOnlyList<ToolCategoryDto>> GetCategoriesAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<ToolCategoryDto>>([]);

        public Task<ToolCatalogPageDto> GetToolsAsync(
            ToolCatalogFilter filter,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(new ToolCatalogPageDto(1, 20, 0, []));

        public Task<ToolDetailsDto?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default) =>
            Task.FromResult<ToolDetailsDto?>(null);
    }

    private sealed class FakeExecutionService : IToolExecutionService
    {
        public string? LastSlug { get; private set; }

        public Task<ToolExecutionResultDto> ExecuteAsync(
            string slug,
            ExecuteToolRequest request,
            Guid? userId = null,
            CancellationToken cancellationToken = default)
        {
            LastSlug = slug;
            return Task.FromResult(new ToolExecutionResultDto(
                null,
                slug,
                "JsonFormatter",
                true,
                JsonDocument.Parse("{}").RootElement.Clone(),
                null,
                null,
                1,
                false,
                DateTime.UtcNow));
        }
    }

    private sealed class FakeFavoriteService : IToolFavoriteService
    {
        public Guid? LastUserId { get; private set; }

        public Task AddAsync(Guid userId, Guid toolId, CancellationToken cancellationToken = default)
        {
            LastUserId = userId;
            return Task.CompletedTask;
        }

        public Task RemoveAsync(Guid userId, Guid toolId, CancellationToken cancellationToken = default)
        {
            LastUserId = userId;
            return Task.CompletedTask;
        }

        public Task<IReadOnlyList<ToolFavoriteDto>> GetUserFavoritesAsync(
            Guid userId,
            CancellationToken cancellationToken = default)
        {
            LastUserId = userId;
            return Task.FromResult<IReadOnlyList<ToolFavoriteDto>>([]);
        }
    }

    private sealed class FakeHistoryQueries : IToolExecutionHistoryQueries
    {
        public Task<ToolExecutionHistoryPageDto> GetMyHistoryAsync(
            Guid userId,
            ToolExecutionHistoryFilter filter,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(new ToolExecutionHistoryPageDto(1, 20, 0, []));

        public Task<ToolExecutionHistoryItemDto?> GetMyExecutionAsync(
            Guid userId,
            Guid id,
            CancellationToken cancellationToken = default) =>
            Task.FromResult<ToolExecutionHistoryItemDto?>(null);
    }
}

using System.Reflection;
using System.Text.Json;
using HelpDev.API.Controllers;
using HelpDev.API.Filters;
using HelpDev.API.Tests.Fakes;
using HelpDev.Modules.Identity.Application.Auth;
using HelpDev.Modules.PromptLab.Application;
using HelpDev.Modules.PromptLab.Application.Catalog;
using HelpDev.Modules.PromptLab.Application.Prompts;
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
    [InlineData(typeof(PromptLabAdminReviewController))]
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
    public void Writer_controller_requires_WriterOrAdmin()
    {
        var attribute = Assert.Single(
            typeof(PromptLabWriterController).GetCustomAttributes(typeof(AuthorizeAttribute), inherit: true)
                .Cast<AuthorizeAttribute>());
        Assert.Equal(AuthorizationPolicies.WriterOrAdmin, attribute.Policy);
        Assert.DoesNotContain(
            typeof(PromptLabWriterController).GetConstructors().Single().GetParameters(),
            p => p.ParameterType.Name.Contains("Repository", StringComparison.Ordinal)
                || p.ParameterType.Name.Contains("DbContext", StringComparison.Ordinal));
    }

    [Fact]
    public async Task Writer_endpoints_use_authenticated_user_id()
    {
        var writer = new FakeWriterService();
        var queries = new FakeWriterQueries();
        var controller = new PromptLabWriterController(writer, queries);
        var userId = Guid.NewGuid();
        ControllerTestHelper.SetUser(controller, userId, AppRoles.Writer);

        var created = await controller.Create(
            new CreateWriterPromptRequest(
                "Title",
                "title",
                null,
                "body",
                null,
                "Text",
                Guid.NewGuid(),
                Guid.NewGuid()),
            CancellationToken.None);
        Assert.IsType<CreatedAtActionResult>(created.Result);

        Assert.IsType<OkObjectResult>((await controller.List(null, 1, 20, CancellationToken.None)).Result);
        var missing = await Assert.ThrowsAsync<PromptLabException>(
            () => controller.GetById(Guid.NewGuid(), CancellationToken.None));
        Assert.Equal(PromptLabApplicationErrorCodes.PromptNotFound, missing.Code);

        await controller.Update(
            writer.LastId,
            new UpdateWriterPromptRequest(
                "Title",
                "title",
                null,
                "body",
                null,
                "Text",
                Guid.NewGuid(),
                Guid.NewGuid()),
            CancellationToken.None);
        await controller.Submit(writer.LastId, CancellationToken.None);

        Assert.Equal(userId, writer.LastAuthorId);
        Assert.Equal(userId, queries.LastAuthorId);
        Assert.False(writer.PublishedAutomatically);
    }

    [Fact]
    public async Task Review_endpoints_use_authenticated_admin_id()
    {
        var queries = new FakeReviewQueries();
        var review = new FakeReviewService();
        var controller = new PromptLabAdminReviewController(queries, review);
        var userId = Guid.NewGuid();
        ControllerTestHelper.SetUser(controller, userId, AppRoles.Admin);

        Assert.IsType<OkObjectResult>((await controller.List("Submitted", 1, 20, CancellationToken.None)).Result);
        Assert.Equal("Submitted", queries.LastStatus);

        var missing = await Assert.ThrowsAsync<PromptLabException>(
            () => controller.GetById(Guid.NewGuid(), CancellationToken.None));
        Assert.Equal(PromptLabApplicationErrorCodes.PromptNotFound, missing.Code);

        await controller.Approve(review.LastId, CancellationToken.None);
        await controller.Reject(
            review.LastId,
            new RejectAdminPromptRequest("عنوان مبهم است."),
            CancellationToken.None);

        Assert.Equal(userId, review.LastActorId);
        Assert.Equal("عنوان مبهم است.", review.LastReason);
    }

    [Fact]
    public void Catalog_controller_depends_on_render_catalog_and_public_queries()
    {
        var parameters = typeof(PromptLabCatalogController).GetConstructors().Single().GetParameters();
        Assert.Contains(parameters, p => p.ParameterType == typeof(IPromptRenderService));
        Assert.Contains(parameters, p => p.ParameterType == typeof(IPromptCatalogQueries));
        Assert.Contains(parameters, p => p.ParameterType == typeof(IPromptPublicQueries));
        Assert.DoesNotContain(
            parameters,
            p => p.ParameterType.Name.Contains("Repository", StringComparison.Ordinal)
                || p.ParameterType.Name.Contains("DbContext", StringComparison.Ordinal));
    }

    [Fact]
    public async Task Catalog_and_render_forward_to_services()
    {
        var catalog = new FakeCatalogQueries();
        var publicQueries = new FakePublicQueries();
        var render = new FakeRenderService();
        var controller = new PromptLabCatalogController(catalog, publicQueries, render);
        ControllerTestHelper.SetUser(controller, userId: null);

        Assert.IsType<OkObjectResult>((await controller.GetCategories(CancellationToken.None)).Result);
        Assert.IsType<OkObjectResult>((await controller.GetAiModels(CancellationToken.None)).Result);
        Assert.IsType<OkObjectResult>(
            (await controller.GetPrompts(null, null, null, null, false, 1, 20, CancellationToken.None)).Result);

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
        Assert.NotNull(publicQueries.LastFilter);
    }

    [Fact]
    public async Task Public_list_forwards_filters_to_public_queries()
    {
        var publicQueries = new FakePublicQueries();
        var controller = new PromptLabCatalogController(
            new FakeCatalogQueries(),
            publicQueries,
            new FakeRenderService());

        await controller.GetPrompts(
            "coding",
            "chatgpt",
            "Image",
            "review",
            popular: true,
            page: 2,
            pageSize: 10,
            CancellationToken.None);

        var filter = Assert.IsType<PublicPromptFilter>(publicQueries.LastFilter);
        Assert.Equal("coding", filter.Category);
        Assert.Equal("chatgpt", filter.AiModel);
        Assert.Equal("Image", filter.MediaType);
        Assert.Equal("review", filter.Search);
        Assert.True(filter.Popular);
        Assert.Equal(2, filter.Page);
        Assert.Equal(10, filter.PageSize);
    }

    [Fact]
    public async Task Unpublished_slug_is_not_found()
    {
        var controller = new PromptLabCatalogController(
            new FakeCatalogQueries(),
            new FakePublicQueries { Details = null },
            new FakeRenderService());

        var ex = await Assert.ThrowsAsync<PromptLabException>(
            () => controller.GetBySlug("secret-draft", CancellationToken.None));
        Assert.Equal(PromptLabApplicationErrorCodes.PromptNotFound, ex.Code);
    }

    [Fact]
    public void Public_dtos_do_not_expose_workflow_status()
    {
        Assert.DoesNotContain(
            typeof(PublicPromptListItemDto).GetProperties().Select(property => property.Name),
            name => name.Contains("Status", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(
            typeof(PublicPromptDetailsDto).GetProperties().Select(property => property.Name),
            name => name.Contains("Status", StringComparison.OrdinalIgnoreCase));
        Assert.Null(typeof(PublicPromptListItemDto).GetProperty("Content"));
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
    [InlineData(PromptLabApplicationErrorCodes.PromptEditForbidden, StatusCodes.Status403Forbidden)]
    [InlineData(PromptLabApplicationErrorCodes.PromptNotDraft, StatusCodes.Status409Conflict)]
    [InlineData(PromptLabApplicationErrorCodes.PromptRejectionReasonRequired, StatusCodes.Status400BadRequest)]
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

    private sealed class FakeReviewQueries : IPromptAdminReviewQueries
    {
        public string? LastStatus { get; private set; }

        public Task<AdminPromptReviewPageDto> GetPromptsAsync(
            AdminPromptReviewFilter filter,
            CancellationToken cancellationToken = default)
        {
            LastStatus = filter.Status;
            return Task.FromResult(new AdminPromptReviewPageDto(filter.Page, filter.PageSize, 0, []));
        }

        public Task<AdminPromptReviewDetailsDto?> GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken = default) =>
            Task.FromResult<AdminPromptReviewDetailsDto?>(null);
    }

    private sealed class FakeReviewService : IPromptAdminReviewService
    {
        public Guid LastActorId { get; private set; }

        public Guid LastId { get; } = Guid.NewGuid();

        public string? LastReason { get; private set; }

        public Task<AdminPromptReviewDetailsDto> ApproveAsync(
            Guid actorUserId,
            Guid id,
            CancellationToken cancellationToken = default)
        {
            LastActorId = actorUserId;
            return Task.FromResult(Details("Approved"));
        }

        public Task<AdminPromptReviewDetailsDto> RejectAsync(
            Guid actorUserId,
            Guid id,
            RejectAdminPromptRequest request,
            CancellationToken cancellationToken = default)
        {
            LastActorId = actorUserId;
            LastReason = request.Reason;
            return Task.FromResult(Details("Rejected"));
        }

        private AdminPromptReviewDetailsDto Details(string status) =>
            new(
                LastId,
                "Title",
                "title",
                null,
                "body",
                null,
                "Text",
                Guid.NewGuid(),
                Guid.NewGuid(),
                "Coding",
                Guid.NewGuid(),
                status,
                LastReason,
                0,
                0,
                DateTime.UtcNow,
                DateTime.UtcNow,
                PublishedAt: status == "Approved" ? DateTime.UtcNow : null);
    }

    private sealed class FakeWriterService : IPromptWriterService
    {
        public Guid LastAuthorId { get; private set; }

        public Guid LastId { get; private set; } = Guid.NewGuid();

        public bool PublishedAutomatically { get; private set; }

        public Task<WriterPromptDetailsDto> CreateAsync(
            Guid authorId,
            CreateWriterPromptRequest request,
            CancellationToken cancellationToken = default)
        {
            LastAuthorId = authorId;
            return Task.FromResult(Details(nameof(HelpDev.Modules.PromptLab.Domain.Prompts.PromptStatus.Draft)));
        }

        public Task<WriterPromptDetailsDto> UpdateAsync(
            Guid authorId,
            Guid id,
            UpdateWriterPromptRequest request,
            CancellationToken cancellationToken = default)
        {
            LastAuthorId = authorId;
            LastId = id;
            return Task.FromResult(Details(nameof(HelpDev.Modules.PromptLab.Domain.Prompts.PromptStatus.Draft)));
        }

        public Task<WriterPromptDetailsDto> SubmitAsync(
            Guid authorId,
            Guid id,
            CancellationToken cancellationToken = default)
        {
            LastAuthorId = authorId;
            LastId = id;
            PublishedAutomatically = false;
            return Task.FromResult(Details(nameof(HelpDev.Modules.PromptLab.Domain.Prompts.PromptStatus.Submitted)));
        }

        private WriterPromptDetailsDto Details(string status) =>
            new(
                LastId,
                "Title",
                "title",
                null,
                "body",
                null,
                "Text",
                Guid.NewGuid(),
                Guid.NewGuid(),
                status,
                0,
                0,
                DateTime.UtcNow,
                DateTime.UtcNow,
                PublishedAt: null);
    }

    private sealed class FakeWriterQueries : IPromptWriterQueries
    {
        public Guid LastAuthorId { get; private set; }

        public Task<WriterPromptPageDto> GetMyPromptsAsync(
            Guid authorId,
            WriterPromptFilter filter,
            CancellationToken cancellationToken = default)
        {
            LastAuthorId = authorId;
            return Task.FromResult(new WriterPromptPageDto(filter.Page, filter.PageSize, 0, []));
        }

        public Task<WriterPromptDetailsDto?> GetMyByIdAsync(
            Guid authorId,
            Guid id,
            CancellationToken cancellationToken = default)
        {
            LastAuthorId = authorId;
            return Task.FromResult<WriterPromptDetailsDto?>(null);
        }
    }

    private sealed class FakePublicQueries : IPromptPublicQueries
    {
        public PublicPromptFilter? LastFilter { get; private set; }

        public PublicPromptDetailsDto? Details { get; init; }

        public Task<PublicPromptPageDto> GetPromptsAsync(
            PublicPromptFilter filter,
            CancellationToken cancellationToken = default)
        {
            LastFilter = filter;
            return Task.FromResult(new PublicPromptPageDto(filter.Page, filter.PageSize, 0, []));
        }

        public Task<PublicPromptDetailsDto?> GetBySlugAsync(
            string slug,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(Details);
    }

    private sealed class FakeCatalogQueries : IPromptCatalogQueries
    {
        public Task<IReadOnlyList<PromptCategoryDto>> GetCategoriesAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<PromptCategoryDto>>([]);

        public Task<IReadOnlyList<PromptAiModelDto>> GetAiModelsAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<PromptAiModelDto>>([]);

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

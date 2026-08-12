using System.Reflection;
using HelpDev.API.Controllers;
using HelpDev.API.Filters;
using HelpDev.API.Tests.Fakes;
using HelpDev.Modules.Identity.Application.Auth;
using HelpDev.Modules.Search.Application.Knowledge;
using HelpDev.Modules.Search.Application.Reindex;
using HelpDev.Modules.Search.Application.Semantic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;

namespace HelpDev.API.Tests;

public sealed class SearchManageControllerTests
{
    [Fact]
    public void Endpoint_requires_AdminOnly_policy()
    {
        var attribute = Assert.Single(
            typeof(SearchManageController)
                .GetCustomAttributes(typeof(AuthorizeAttribute), inherit: true)
                .Cast<AuthorizeAttribute>());

        Assert.Equal(AuthorizationPolicies.AdminOnly, attribute.Policy);
        Assert.NotEqual(AuthorizationPolicies.WriterOrAdmin, attribute.Policy);
    }

    [Fact]
    public async Task Admin_can_invoke_reindex()
    {
        var service = new FakeSearchReindexService();
        var controller = CreateController(service);
        ControllerTestHelper.SetUser(controller, Guid.NewGuid(), AppRoles.Admin);

        var result = await controller.Reindex(
            new SearchReindexHttpRequest { SourceType = "content", BatchSize = 25 },
            CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.IsType<SearchReindexResultDto>(ok.Value);
        Assert.Equal("content", service.LastRequest!.SourceType);
        Assert.Equal(25, service.LastRequest.BatchSize);
    }

    [Fact]
    public async Task Knowledge_dashboard_returns_dto_without_embeddings()
    {
        var knowledge = new FakeKnowledgeDashboardQueries();
        var controller = CreateController(knowledge: knowledge);
        ControllerTestHelper.SetUser(controller, Guid.NewGuid(), AppRoles.Admin);

        var result = await controller.Knowledge(sourceType: null, CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.IsType<KnowledgeDashboardDto>(ok.Value);
        Assert.DoesNotContain(
            typeof(KnowledgeDashboardDto).GetProperties(),
            p => p.Name.Contains("Embedding", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Request_contract_exposes_only_sourceType_and_batchSize()
    {
        var names = typeof(SearchReindexHttpRequest)
            .GetProperties(BindingFlags.Instance | BindingFlags.Public)
            .Select(property => property.Name)
            .OrderBy(name => name)
            .ToArray();

        Assert.Equal(["BatchSize", "SourceType"], names);
        Assert.Null(typeof(SearchReindexHttpRequest).GetProperty("UserId"));
        Assert.Null(typeof(SearchReindexHttpRequest).GetProperty("LockId"));
    }

    [Fact]
    public void Controller_depends_on_application_ports_only()
    {
        var parameters = typeof(SearchManageController).GetConstructors().Single().GetParameters();
        Assert.Equal(3, parameters.Length);
        Assert.Contains(parameters, p => p.ParameterType == typeof(ISearchReindexService));
        Assert.Contains(parameters, p => p.ParameterType == typeof(IKnowledgeDashboardQueries));
        Assert.Contains(parameters, p => p.ParameterType == typeof(ISemanticSearchQueries));
        Assert.DoesNotContain(parameters, p => p.ParameterType.Name.Contains("DbContext", StringComparison.Ordinal));
    }

    [Fact]
    public async Task Forwards_CancellationToken_and_defaults_batch_size()
    {
        var service = new FakeSearchReindexService();
        var controller = CreateController(service);
        using var cts = new CancellationTokenSource();

        await controller.Reindex(new SearchReindexHttpRequest(), cts.Token);

        Assert.Null(service.LastRequest!.SourceType);
        Assert.Equal(SearchReindexService.DefaultBatchSize, service.LastRequest.BatchSize);
        Assert.Equal(cts.Token, service.LastCancellationToken);
    }

    private static SearchManageController CreateController(
        ISearchReindexService? reindex = null,
        IKnowledgeDashboardQueries? knowledge = null,
        ISemanticSearchQueries? semantic = null) =>
        new(
            reindex ?? new FakeSearchReindexService(),
            knowledge ?? new FakeKnowledgeDashboardQueries(),
            semantic ?? new FakeSemanticSearchQueries());
}

public sealed class SearchReindexExceptionFilterTests
{
    [Theory]
    [InlineData(SearchReindexErrorCodes.SourceInvalid, StatusCodes.Status400BadRequest)]
    [InlineData(SearchReindexErrorCodes.BatchSizeInvalid, StatusCodes.Status400BadRequest)]
    [InlineData(SearchReindexErrorCodes.AlreadyRunning, StatusCodes.Status409Conflict)]
    public void Filter_maps_reindex_codes(string code, int expectedStatus)
    {
        var filter = new SearchExceptionFilter();
        var httpContext = new DefaultHttpContext();
        var actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor());
        var context = new ExceptionContext(actionContext, new List<IFilterMetadata>())
        {
            Exception = new SearchReindexException("boom", code),
        };

        filter.OnException(context);

        Assert.True(context.ExceptionHandled);
        var result = Assert.IsType<ObjectResult>(context.Result);
        Assert.Equal(expectedStatus, result.StatusCode);
    }
}

internal sealed class FakeSearchReindexService : ISearchReindexService
{
    public SearchReindexRequest? LastRequest { get; private set; }

    public CancellationToken LastCancellationToken { get; private set; }

    public SearchReindexResultDto Result { get; set; } =
        new(0, 0, 0, 0, 0, DateTime.UtcNow, DateTime.UtcNow);

    public Exception? ExceptionToThrow { get; set; }

    public Task<SearchReindexResultDto> ReindexAsync(
        SearchReindexRequest request,
        CancellationToken cancellationToken = default)
    {
        LastRequest = request;
        LastCancellationToken = cancellationToken;
        if (ExceptionToThrow is not null)
        {
            throw ExceptionToThrow;
        }

        return Task.FromResult(Result);
    }
}

internal sealed class FakeKnowledgeDashboardQueries : IKnowledgeDashboardQueries
{
    public Task<KnowledgeDashboardDto> GetAsync(
        string? sourceType = null,
        CancellationToken cancellationToken = default) =>
        Task.FromResult(new KnowledgeDashboardDto(0, 0, 0, 0, sourceType, [], []));
}

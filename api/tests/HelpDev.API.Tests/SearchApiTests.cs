using HelpDev.API.Controllers;
using HelpDev.API.Filters;
using HelpDev.Modules.Search.Application.Dtos;
using HelpDev.Modules.Search.Application.Rag;
using HelpDev.Modules.Search.Application.Search;
using HelpDev.Modules.Search.Application.Semantic;
using HelpDev.Modules.Search.Domain;
using HelpDev.Testing.Analytics;
using HelpDev.Testing.Auditing;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging.Abstractions;
using System.Reflection;

namespace HelpDev.API.Tests;

public sealed class SearchControllerTests
{
    [Fact]
    public void Search_allows_anonymous_access_metadata()
    {
        Assert.Null(typeof(SearchController).GetCustomAttribute<Microsoft.AspNetCore.Authorization.AuthorizeAttribute>());
        Assert.Null(typeof(SearchController).GetMethod(nameof(SearchController.Search))!
            .GetCustomAttribute<Microsoft.AspNetCore.Authorization.AuthorizeAttribute>());
        Assert.Null(typeof(SearchController).GetMethod(nameof(SearchController.Semantic))!
            .GetCustomAttribute<Microsoft.AspNetCore.Authorization.AuthorizeAttribute>());
        Assert.Null(typeof(SearchController).GetMethod(nameof(SearchController.Ask))!
            .GetCustomAttribute<Microsoft.AspNetCore.Authorization.AuthorizeAttribute>());
    }

    [Fact]
    public async Task Search_returns_envelope_from_application_service()
    {
        var service = new FakeSearchService
        {
            Result = new SearchResultDto(
                "csharp",
                1,
                20,
                1,
                [
                    new SearchItemDto(
                        SearchSourceTypes.Content,
                        Guid.NewGuid(),
                        "CSharp",
                        "csharp",
                        "Summary",
                        "/content/csharp",
                        DateTime.UtcNow,
                        DateTime.UtcNow),
                ]),
        };
        var controller = CreateController(service);

        var result = await controller.Search("csharp", null, 1, 20, CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var dto = Assert.IsType<SearchResultDto>(ok.Value);
        Assert.Equal("csharp", dto.Query);
        Assert.Equal(1, dto.Total);
        Assert.Equal("csharp", service.LastQuery);
        Assert.Null(typeof(SearchItemDto).GetProperty("LastEventId"));
        Assert.Null(typeof(SearchItemDto).GetProperty("IsPublished"));
        Assert.Null(typeof(SearchController).GetConstructors().Single().GetParameters()
            .FirstOrDefault(parameter =>
                parameter.ParameterType.Name.Contains("DbContext", StringComparison.Ordinal)
                || parameter.ParameterType.Name.Contains("Repository", StringComparison.Ordinal)
                || parameter.ParameterType.Name.Contains("Outbox", StringComparison.Ordinal)));
    }

    [Fact]
    public async Task Search_forwards_type_page_and_page_size()
    {
        var service = new FakeSearchService
        {
            Result = new SearchResultDto("q", 2, 10, 0, []),
        };
        var controller = CreateController(service);

        await controller.Search("q", "course", 2, 10, CancellationToken.None);

        Assert.Equal("course", service.LastType);
        Assert.Equal(2, service.LastPage);
        Assert.Equal(10, service.LastPageSize);
    }

    [Fact]
    public async Task Semantic_returns_results_without_vectors()
    {
        var semantic = new FakeSemanticSearchQueries
        {
            Result = new SearchContextDto(
                "dotnet",
                [new SearchContextItemDto("Title", "Snippet", "/content/x", "content", Guid.NewGuid(), 0.81)]),
        };
        var controller = CreateController(semantic: semantic);

        var result = await controller.Semantic("dotnet", 5, CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var dto = Assert.IsType<SemanticSearchResponseDto>(ok.Value);
        Assert.Equal("dotnet", dto.Query);
        Assert.Equal("content", Assert.Single(dto.Results).Type);
        Assert.DoesNotContain(typeof(SemanticSearchResultDto).GetProperties(), p => p.Name.Contains("Embedding", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(typeof(SemanticSearchResultDto).GetProperties(), p => p.Name.Contains("Vector", StringComparison.OrdinalIgnoreCase));
        Assert.Equal("dotnet", semantic.LastQuery);
    }

    [Fact]
    public async Task Ask_returns_rag_answer_contract()
    {
        var rag = new FakeRagAnswerService
        {
            Result = new RagAnswerDto("answer", [], DateTime.UtcNow),
        };
        var controller = CreateController(rag: rag);

        var result = await controller.Ask(new SearchAskRequest { Question = "What is HelpDev?" }, CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var dto = Assert.IsType<RagAnswerDto>(ok.Value);
        Assert.Equal("answer", dto.Answer);
        Assert.DoesNotContain(typeof(RagAnswerDto).GetProperties(), p => p.Name.Contains("Confidence", StringComparison.OrdinalIgnoreCase));
        Assert.Equal("What is HelpDev?", rag.LastQuestion);
    }

    private static SearchController CreateController(
        ISearchService? search = null,
        ISemanticSearchQueries? semantic = null,
        IRagAnswerService? rag = null) =>
        new(
            search ?? new FakeSearchService(),
            semantic ?? new FakeSemanticSearchQueries(),
            rag ?? new FakeRagAnswerService(),
            new NoOpAnalyticsEventIngestor(),
            new NoOpAuditRecorder(),
            NullLogger<SearchController>.Instance);
}

public sealed class SearchExceptionFilterTests
{
    [Theory]
    [InlineData(SearchErrorCodes.QueryRequired)]
    [InlineData(SearchErrorCodes.QueryTooLong)]
    [InlineData(SearchErrorCodes.PageInvalid)]
    [InlineData(SearchErrorCodes.PageSizeInvalid)]
    [InlineData(SearchErrorCodes.TypeInvalid)]
    public void Filter_maps_search_exceptions_to_400(string code)
    {
        var filter = new SearchExceptionFilter();
        var httpContext = new DefaultHttpContext();
        var actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor());
        var context = new ExceptionContext(actionContext, new List<IFilterMetadata>())
        {
            Exception = new SearchException("boom", code),
        };

        filter.OnException(context);

        Assert.True(context.ExceptionHandled);
        var result = Assert.IsType<ObjectResult>(context.Result);
        Assert.Equal(StatusCodes.Status400BadRequest, result.StatusCode);
    }
}

internal sealed class FakeSearchService : ISearchService
{
    public SearchResultDto Result { get; set; } =
        new("q", 1, 20, 0, []);

    public string? LastQuery { get; private set; }

    public string? LastType { get; private set; }

    public int LastPage { get; private set; }

    public int LastPageSize { get; private set; }

    public Task<SearchResultDto> SearchAsync(
        string? query,
        string? sourceType,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        LastQuery = query;
        LastType = sourceType;
        LastPage = page;
        LastPageSize = pageSize;
        return Task.FromResult(Result);
    }
}

internal sealed class FakeSemanticSearchQueries : ISemanticSearchQueries
{
    public SearchContextDto Result { get; set; } = new("q", []);

    public string? LastQuery { get; private set; }

    public Task<SearchContextDto> SearchSimilarAsync(
        string query,
        int take = 8,
        CancellationToken cancellationToken = default)
    {
        LastQuery = query;
        return Task.FromResult(Result);
    }

    public Task<SearchContextDto> RetrieveContextAsync(
        string query,
        int take = 6,
        CancellationToken cancellationToken = default) =>
        SearchSimilarAsync(query, take, cancellationToken);

    public Task<SearchContextDto> SearchRelatedToSourceAsync(
        string sourceType,
        Guid sourceId,
        int take = 6,
        CancellationToken cancellationToken = default) =>
        Task.FromResult(Result);
}

internal sealed class FakeRagAnswerService : IRagAnswerService
{
    public RagAnswerDto Result { get; set; } = new("a", [], DateTime.UtcNow);

    public string? LastQuestion { get; private set; }

    public Task<RagAnswerDto> AskAsync(string question, CancellationToken cancellationToken = default)
    {
        LastQuestion = question;
        return Task.FromResult(Result);
    }
}

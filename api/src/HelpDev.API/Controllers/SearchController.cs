using Asp.Versioning;
using HelpDev.API.Contracts;
using HelpDev.API.Extensions;
using HelpDev.API.OpenApi;
using HelpDev.API.Security;
using HelpDev.Modules.Search.Application.Dtos;
using HelpDev.Modules.Search.Application.Rag;
using HelpDev.Modules.Search.Application.Search;
using HelpDev.Modules.Search.Application.Semantic;
using HelpDev.SharedContracts.Analytics;
using HelpDev.SharedContracts.Auditing;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace HelpDev.API.Controllers;

[ApiController]
[ApiVersion("1.0")]
[ApiAudience(ApiAudiences.Public)]
[Tags(ApiTags.Search)]
[Route("api/search")]
[Route("api/v{version:apiVersion}/search")]
public sealed class SearchController : ControllerBase
{
    private readonly ISearchService _searchService;
    private readonly ISemanticSearchQueries _semanticSearchQueries;
    private readonly IRagAnswerService _ragAnswerService;
    private readonly IAnalyticsEventIngestor _analyticsIngestor;
    private readonly IAuditRecorder _auditRecorder;
    private readonly ILogger<SearchController> _logger;

    public SearchController(
        ISearchService searchService,
        ISemanticSearchQueries semanticSearchQueries,
        IRagAnswerService ragAnswerService,
        IAnalyticsEventIngestor analyticsIngestor,
        IAuditRecorder auditRecorder,
        ILogger<SearchController> logger)
    {
        _searchService = searchService;
        _semanticSearchQueries = semanticSearchQueries;
        _ragAnswerService = ragAnswerService;
        _analyticsIngestor = analyticsIngestor;
        _auditRecorder = auditRecorder;
        _logger = logger;
    }

    [HttpGet]
    [EnableRateLimiting(RateLimitPolicyNames.Search)]
    [OpenApiOperationId("Search_Search")]
    [OpenApiSummary("Search", "Searches published content, courses, tools, and prompts.")]
    [ProducesResponseType(typeof(SearchResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<SearchResultDto>> Search(
        [FromQuery] string? q,
        [FromQuery] string? type,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = SearchService.DefaultPageSize,
        CancellationToken cancellationToken = default)
    {
        var result = await _searchService.SearchAsync(q, type, page, pageSize, cancellationToken);
        Guid? userId = null;
        if (HttpContext is not null)
        {
            try { userId = User.GetUserId(); } catch { /* context principal unavailable */ }
        }
        await TryIngestSearchAsync(result, userId, cancellationToken);
        return Ok(result);
    }

    [HttpGet("semantic")]
    [EnableRateLimiting(RateLimitPolicyNames.Search)]
    [OpenApiOperationId("Search_Semantic")]
    [OpenApiSummary("Semantic search", "Vector similarity over indexed HelpDev knowledge chunks. Anonymous.")]
    [ProducesResponseType(typeof(SemanticSearchResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<SemanticSearchResponseDto>> Semantic(
        [FromQuery] string? q,
        [FromQuery] int take = 8,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(q) || q.Trim().Length < 2)
        {
            return BadRequest(new { message = "Query is required.", code = "search_query_invalid" });
        }

        var result = await _semanticSearchQueries.SearchSimilarAsync(q.Trim(), take, cancellationToken);
        await TryAuditSemanticAsync(result.Items.Count, cancellationToken);

        return Ok(new SemanticSearchResponseDto(
            result.Query,
            result.Items
                .Select(i => new SemanticSearchResultDto(
                    i.Title,
                    i.SourceType,
                    i.Snippet,
                    i.SourceUrl,
                    i.Similarity))
                .ToList()));
    }

    [HttpPost("ask")]
    [EnableRateLimiting(RateLimitPolicyNames.Search)]
    [OpenApiOperationId("Search_Ask")]
    [OpenApiSummary("Ask HelpDev knowledge", "RAG answer grounded in retrieved HelpDev snippets only. Anonymous.")]
    [ProducesResponseType(typeof(RagAnswerDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<RagAnswerDto>> Ask(
        [FromBody] SearchAskRequest? request,
        CancellationToken cancellationToken = default)
    {
        var question = request?.Question?.Trim() ?? string.Empty;
        if (question.Length is < 2 or > 1000)
        {
            return BadRequest(new { message = "Question is invalid.", code = "search_question_invalid" });
        }

        var answer = await _ragAnswerService.AskAsync(question, cancellationToken);
        return Ok(answer);
    }

    private async Task TryAuditSemanticAsync(int sourceCount, CancellationToken cancellationToken)
    {
        try
        {
            await _auditRecorder.RecordAsync(
                new AuditRecordInput(
                    AuditCategories.SearchRag,
                    AuditActions.SemanticSearchRequested,
                    AuditOutcomes.Success,
                    ActorUserId: null,
                    AuditActorTypes.Anonymous,
                    Metadata: new Dictionary<string, string>(StringComparer.Ordinal)
                    {
                        ["sourceCount"] = sourceCount.ToString(),
                    }),
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Semantic search audit skipped.");
        }
    }

    private async Task TryIngestSearchAsync(
        SearchResultDto result,
        Guid? userId,
        CancellationToken cancellationToken)
    {
        try
        {
            var occurredAt = DateTime.UtcNow;
            var dimensions = new Dictionary<string, string>
            {
                [AnalyticsDimensionKeys.ResultBucket] = AnalyticsResultBuckets.FromResultCount(result.Total),
                [AnalyticsDimensionKeys.IsAuthenticated] = userId.HasValue ? "true" : "false",
            };

            await _analyticsIngestor.IngestAsync(
                new AnalyticsEventEnvelope(
                    Guid.NewGuid(),
                    AnalyticsEventTypes.SearchExecuted,
                    occurredAt,
                    userId,
                    SubjectId: null,
                    SubjectType: null,
                    dimensions),
                cancellationToken);

            if (result.Total == 0)
            {
                await _analyticsIngestor.IngestAsync(
                    new AnalyticsEventEnvelope(
                        Guid.NewGuid(),
                        AnalyticsEventTypes.SearchZeroResults,
                        occurredAt,
                        userId,
                        SubjectId: null,
                        SubjectType: null,
                        dimensions),
                    cancellationToken);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Analytics search ingestion skipped.");
        }
    }
}

public sealed class SearchAskRequest
{
    public string? Question { get; set; }
}

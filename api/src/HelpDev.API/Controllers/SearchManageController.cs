using Asp.Versioning;
using HelpDev.API.Contracts;
using HelpDev.API.OpenApi;
using HelpDev.Modules.Identity.Application.Auth;
using HelpDev.Modules.Search.Application.Knowledge;
using HelpDev.Modules.Search.Application.Reindex;
using HelpDev.Modules.Search.Application.Semantic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HelpDev.API.Controllers;

[ApiController]
[ApiVersion("1.0")]
[ApiAudience(ApiAudiences.Admin)]
[Tags(ApiTags.Search)]
[Authorize(Policy = AuthorizationPolicies.AdminOnly)]
[Route("api/search/manage")]
[Route("api/v{version:apiVersion}/search/manage")]
public sealed class SearchManageController : ControllerBase
{
    private readonly ISearchReindexService _reindexService;
    private readonly IKnowledgeDashboardQueries _knowledgeDashboardQueries;
    private readonly ISemanticSearchQueries _semanticSearchQueries;

    public SearchManageController(
        ISearchReindexService reindexService,
        IKnowledgeDashboardQueries knowledgeDashboardQueries,
        ISemanticSearchQueries semanticSearchQueries)
    {
        _reindexService = reindexService;
        _knowledgeDashboardQueries = knowledgeDashboardQueries;
        _semanticSearchQueries = semanticSearchQueries;
    }

    [HttpPost("reindex")]
    [OpenApiOperationId("SearchManage_Reindex")]
    [OpenApiSummary("Reindex search", "Triggers a search index rebuild for one or all source types.")]
    [ProducesResponseType(typeof(SearchReindexResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<SearchReindexResultDto>> Reindex(
        [FromBody] SearchReindexHttpRequest? request,
        CancellationToken cancellationToken)
    {
        var batchSize = request?.BatchSize ?? SearchReindexService.DefaultBatchSize;
        var result = await _reindexService.ReindexAsync(
            new SearchReindexRequest(request?.SourceType, batchSize),
            cancellationToken);
        return Ok(result);
    }

    [HttpGet("knowledge")]
    [OpenApiOperationId("SearchManage_KnowledgeDashboard")]
    [OpenApiSummary("Knowledge dashboard", "Indexed documents/chunks/status. No raw embeddings.")]
    [ProducesResponseType(typeof(KnowledgeDashboardDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<KnowledgeDashboardDto>> Knowledge(
        [FromQuery] string? sourceType = null,
        CancellationToken cancellationToken = default)
    {
        return Ok(await _knowledgeDashboardQueries.GetAsync(sourceType, cancellationToken));
    }

    [HttpGet("related")]
    [OpenApiOperationId("SearchManage_RelatedKnowledge")]
    [OpenApiSummary("Related knowledge", "Similar indexed sources for Content Studio. No automatic linking.")]
    [ProducesResponseType(typeof(SearchContextDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<SearchContextDto>> Related(
        [FromQuery] string sourceType,
        [FromQuery] Guid sourceId,
        [FromQuery] int take = 6,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(sourceType) || sourceId == Guid.Empty)
        {
            return BadRequest(new { message = "Source is required.", code = "search_source_invalid" });
        }

        var result = await _semanticSearchQueries.SearchRelatedToSourceAsync(
            sourceType,
            sourceId,
            take,
            cancellationToken);
        return Ok(result);
    }
}

public sealed class SearchReindexHttpRequest
{
    public string? SourceType { get; set; }

    public int? BatchSize { get; set; }
}

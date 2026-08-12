using Asp.Versioning;
using HelpDev.API.Contracts;
using HelpDev.API.Extensions;
using HelpDev.API.OpenApi;
using HelpDev.API.Security;
using HelpDev.Modules.Toolbox.Application.Catalog;
using HelpDev.Modules.Toolbox.Application.Execution;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace HelpDev.API.Controllers;

[ApiController]
[ApiVersion("1.0")]
[ApiAudience(ApiAudiences.Public)]
[Tags(ApiTags.Toolbox)]
[AllowAnonymous]
[Route("api/tools")]
[Route("api/v{version:apiVersion}/tools")]
public sealed class ToolboxCatalogController : ControllerBase
{
    private readonly IToolCatalogQueries _catalogQueries;
    private readonly IToolExecutionService _executionService;

    public ToolboxCatalogController(
        IToolCatalogQueries catalogQueries,
        IToolExecutionService executionService)
    {
        _catalogQueries = catalogQueries;
        _executionService = executionService;
    }

    [HttpGet("categories")]
    [OpenApiOperationId("ToolboxCatalog_GetCategories")]
    [OpenApiSummary("List tool categories", "Returns all published tool categories.")]
    [ProducesResponseType(typeof(IReadOnlyList<ToolCategoryDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ToolCategoryDto>>> GetCategories(
        CancellationToken cancellationToken)
    {
        return Ok(await _catalogQueries.GetCategoriesAsync(cancellationToken));
    }

    [HttpGet]
    [OpenApiOperationId("ToolboxCatalog_GetTools")]
    [OpenApiSummary("List tools", "Returns a paginated catalog of published tools.")]
    [ProducesResponseType(typeof(ToolCatalogPageDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<ToolCatalogPageDto>> GetTools(
        [FromQuery] string? category,
        [FromQuery] string? search,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = ToolboxPaging.DefaultPageSize,
        CancellationToken cancellationToken = default)
    {
        var result = await _catalogQueries.GetToolsAsync(
            new ToolCatalogFilter(category, search, page, pageSize),
            cancellationToken);
        return Ok(result);
    }

    [HttpGet("{slug}")]
    [OpenApiOperationId("ToolboxCatalog_GetBySlug")]
    [OpenApiSummary("Get tool by slug", "Returns a published tool by its slug.")]
    [ProducesResponseType(typeof(ToolDetailsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ToolDetailsDto>> GetBySlug(
        string slug,
        CancellationToken cancellationToken)
    {
        var tool = await _catalogQueries.GetBySlugAsync(slug, cancellationToken);
        if (tool is null)
        {
            throw new ToolboxException(
                "Tool was not found.",
                ToolboxApplicationErrorCodes.ToolNotFound);
        }

        return Ok(tool);
    }

    [HttpPost("{slug}/execute")]
    [EnableRateLimiting(RateLimitPolicyNames.ToolboxExecution)]
    [RequestSizeLimit(128 * 1024)]
    [OpenApiOperationId("ToolboxCatalog_Execute")]
    [OpenApiSummary("Execute tool", "Executes a published tool with the provided input.")]
    [ProducesResponseType(typeof(ToolExecutionResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ToolExecutionResultDto>> Execute(
        string slug,
        [FromBody] ExecuteToolRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _executionService.ExecuteAsync(
            slug,
            request,
            User.GetUserId(),
            cancellationToken);
        return Ok(result);
    }
}

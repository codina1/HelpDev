using Asp.Versioning;
using HelpDev.API.Contracts;
using HelpDev.API.Extensions;
using HelpDev.API.OpenApi;
using HelpDev.API.Security;
using HelpDev.Modules.PromptLab.Application;
using HelpDev.Modules.PromptLab.Application.Catalog;
using HelpDev.Modules.PromptLab.Application.Rendering;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace HelpDev.API.Controllers;

[ApiController]
[ApiVersion("1.0")]
[ApiAudience(ApiAudiences.Public)]
[Tags(ApiTags.PromptLab)]
[AllowAnonymous]
[Route("api/prompts")]
[Route("api/v{version:apiVersion}/prompts")]
public sealed class PromptLabCatalogController : ControllerBase
{
    private readonly IPromptCatalogQueries _catalogQueries;
    private readonly IPromptRenderService _renderService;

    public PromptLabCatalogController(
        IPromptCatalogQueries catalogQueries,
        IPromptRenderService renderService)
    {
        _catalogQueries = catalogQueries;
        _renderService = renderService;
    }

    [HttpGet("categories")]
    [OpenApiOperationId("PromptLabCatalog_GetCategories")]
    [OpenApiSummary("List prompt categories", "Returns all published prompt categories.")]
    [ProducesResponseType(typeof(IReadOnlyList<PromptCategoryDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<PromptCategoryDto>>> GetCategories(
        CancellationToken cancellationToken)
    {
        return Ok(await _catalogQueries.GetCategoriesAsync(cancellationToken));
    }

    [HttpGet]
    [OpenApiOperationId("PromptLabCatalog_GetPrompts")]
    [OpenApiSummary("List prompts", "Returns a paginated catalog of published prompts.")]
    [ProducesResponseType(typeof(PromptCatalogPageDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<PromptCatalogPageDto>> GetPrompts(
        [FromQuery] string? category,
        [FromQuery] string? purpose,
        [FromQuery] string? search,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = PromptLabPaging.DefaultPageSize,
        CancellationToken cancellationToken = default)
    {
        var result = await _catalogQueries.GetPromptsAsync(
            new PromptCatalogFilter(category, purpose, search, page, pageSize),
            cancellationToken);
        return Ok(result);
    }

    [HttpGet("{slug}")]
    [OpenApiOperationId("PromptLabCatalog_GetBySlug")]
    [OpenApiSummary("Get prompt by slug", "Returns a published prompt by its slug.")]
    [ProducesResponseType(typeof(PromptDetailsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PromptDetailsDto>> GetBySlug(
        string slug,
        CancellationToken cancellationToken)
    {
        var prompt = await _catalogQueries.GetBySlugAsync(slug, User.GetUserId(), cancellationToken);
        if (prompt is null)
        {
            throw new PromptLabException(
                "Prompt was not found.",
                PromptLabApplicationErrorCodes.PromptNotFound);
        }

        return Ok(prompt);
    }

    [HttpPost("{slug}/render")]
    [EnableRateLimiting(RateLimitPolicyNames.PromptRender)]
    [RequestSizeLimit(128 * 1024)]
    [OpenApiOperationId("PromptLabCatalog_Render")]
    [OpenApiSummary("Render prompt", "Renders a published prompt with the provided variables.")]
    [ProducesResponseType(typeof(PromptRenderResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PromptRenderResultDto>> Render(
        string slug,
        [FromBody] RenderPromptRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _renderService.RenderAsync(
            slug,
            request,
            User.GetUserId(),
            cancellationToken);
        return Ok(result);
    }
}

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
    private readonly IPromptPublicQueries _publicQueries;
    private readonly IPromptRenderService _renderService;

    public PromptLabCatalogController(
        IPromptCatalogQueries catalogQueries,
        IPromptPublicQueries publicQueries,
        IPromptRenderService renderService)
    {
        _catalogQueries = catalogQueries;
        _publicQueries = publicQueries;
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

    [HttpGet("ai-models")]
    [OpenApiOperationId("PromptLabCatalog_GetAiModels")]
    [OpenApiSummary("List AI models", "Returns all active AI models that prompts can target.")]
    [ProducesResponseType(typeof(IReadOnlyList<PromptAiModelDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<PromptAiModelDto>>> GetAiModels(
        CancellationToken cancellationToken)
    {
        return Ok(await _catalogQueries.GetAiModelsAsync(cancellationToken));
    }

    [HttpGet]
    [OpenApiOperationId("PromptLabCatalog_GetPrompts")]
    [OpenApiSummary(
        "List prompts",
        "Returns a paginated catalog of approved public prompts. Draft, submitted, and rejected prompts are never returned.")]
    [ProducesResponseType(typeof(PublicPromptPageDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<PublicPromptPageDto>> GetPrompts(
        [FromQuery] string? category,
        [FromQuery] string? aiModel,
        [FromQuery] string? mediaType,
        [FromQuery] string? search,
        [FromQuery] bool popular = false,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = PromptLabPaging.DefaultPageSize,
        CancellationToken cancellationToken = default)
    {
        var result = await _publicQueries.GetPromptsAsync(
            new PublicPromptFilter(category, aiModel, mediaType, search, popular, page, pageSize),
            cancellationToken);
        return Ok(result);
    }

    [HttpGet("{slug}")]
    [OpenApiOperationId("PromptLabCatalog_GetBySlug")]
    [OpenApiSummary(
        "Get prompt by slug",
        "Returns an approved public prompt by its slug. Unpublished prompts are indistinguishable from missing ones.")]
    [ProducesResponseType(typeof(PublicPromptDetailsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PublicPromptDetailsDto>> GetBySlug(
        string slug,
        CancellationToken cancellationToken)
    {
        var prompt = await _publicQueries.GetBySlugAsync(slug, cancellationToken);
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

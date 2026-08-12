using Asp.Versioning;
using HelpDev.API.Contracts;
using HelpDev.API.Extensions;
using HelpDev.API.OpenApi;
using HelpDev.Modules.Identity.Application.Auth;
using HelpDev.Modules.PromptLab.Application;
using HelpDev.Modules.PromptLab.Application.Catalog;
using HelpDev.Modules.PromptLab.Application.Favorites;
using HelpDev.Modules.PromptLab.Application.History;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HelpDev.API.Controllers;

[ApiController]
[ApiVersion("1.0")]
[ApiAudience(ApiAudiences.Authenticated)]
[Tags(ApiTags.PromptLab)]
[Authorize(Policy = AuthorizationPolicies.Authenticated)]
[Route("api/me")]
[Route("api/v{version:apiVersion}/me")]
public sealed class PromptLabMeController : ControllerBase
{
    private readonly IPromptFavoriteService _favoriteService;
    private readonly IPromptRenderHistoryQueries _historyQueries;

    public PromptLabMeController(
        IPromptFavoriteService favoriteService,
        IPromptRenderHistoryQueries historyQueries)
    {
        _favoriteService = favoriteService;
        _historyQueries = historyQueries;
    }

    [HttpGet("prompt-favorites")]
    [OpenApiOperationId("PromptLabMe_ListFavorites")]
    [OpenApiSummary("List prompt favorites", "Returns the authenticated user's favorite prompts.")]
    [ProducesResponseType(typeof(IReadOnlyList<PromptFavoriteDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IReadOnlyList<PromptFavoriteDto>>> ListFavorites(
        CancellationToken cancellationToken)
    {
        var userId = RequireUserId();
        return Ok(await _favoriteService.GetUserFavoritesAsync(userId, cancellationToken));
    }

    [HttpPut("prompt-favorites/{promptId:guid}")]
    [OpenApiOperationId("PromptLabMe_AddFavorite")]
    [OpenApiSummary("Add prompt favorite", "Adds a prompt to the authenticated user's favorites.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> AddFavorite(Guid promptId, CancellationToken cancellationToken)
    {
        var userId = RequireUserId();
        await _favoriteService.AddAsync(userId, promptId, cancellationToken);
        return NoContent();
    }

    [HttpDelete("prompt-favorites/{promptId:guid}")]
    [OpenApiOperationId("PromptLabMe_RemoveFavorite")]
    [OpenApiSummary("Remove prompt favorite", "Removes a prompt from the authenticated user's favorites.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> RemoveFavorite(Guid promptId, CancellationToken cancellationToken)
    {
        var userId = RequireUserId();
        await _favoriteService.RemoveAsync(userId, promptId, cancellationToken);
        return NoContent();
    }

    [HttpGet("prompt-history")]
    [OpenApiOperationId("PromptLabMe_ListHistory")]
    [OpenApiSummary("List prompt render history", "Returns the authenticated user's prompt render history.")]
    [ProducesResponseType(typeof(PromptRenderHistoryPageDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<PromptRenderHistoryPageDto>> ListHistory(
        [FromQuery] Guid? promptId,
        [FromQuery] bool? succeeded,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = PromptLabPaging.DefaultPageSize,
        CancellationToken cancellationToken = default)
    {
        var userId = RequireUserId();
        var pageDto = await _historyQueries.GetMyHistoryAsync(
            userId,
            new PromptRenderHistoryFilter(promptId, succeeded, page, pageSize),
            cancellationToken);
        return Ok(pageDto);
    }

    [HttpGet("prompt-history/{id:guid}")]
    [OpenApiOperationId("PromptLabMe_GetHistoryItem")]
    [OpenApiSummary("Get prompt render history item", "Returns a single prompt render history record.")]
    [ProducesResponseType(typeof(PromptRenderHistoryItemDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PromptRenderHistoryItemDto>> GetHistoryItem(
        Guid id,
        CancellationToken cancellationToken)
    {
        var userId = RequireUserId();
        var item = await _historyQueries.GetMyRenderAsync(userId, id, cancellationToken);
        if (item is null)
        {
            throw new PromptLabException(
                "Render history was not found.",
                PromptLabApplicationErrorCodes.HistoryNotFound);
        }

        return Ok(item);
    }

    private Guid RequireUserId()
    {
        var userId = User.GetUserId();
        if (userId is null)
        {
            throw new PromptLabException(
                "Authentication is required.",
                PromptLabApplicationErrorCodes.FavoriteRequiresAuthentication);
        }

        return userId.Value;
    }
}

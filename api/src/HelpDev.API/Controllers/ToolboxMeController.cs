using Asp.Versioning;
using HelpDev.API.Contracts;
using HelpDev.API.Extensions;
using HelpDev.API.OpenApi;
using HelpDev.Modules.Identity.Application.Auth;
using HelpDev.Modules.Toolbox.Application.Catalog;
using HelpDev.Modules.Toolbox.Application.Execution;
using HelpDev.Modules.Toolbox.Application.Favorites;
using HelpDev.Modules.Toolbox.Application.History;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HelpDev.API.Controllers;

[ApiController]
[ApiVersion("1.0")]
[ApiAudience(ApiAudiences.Authenticated)]
[Tags(ApiTags.Toolbox)]
[Authorize(Policy = AuthorizationPolicies.Authenticated)]
[Route("api/me")]
[Route("api/v{version:apiVersion}/me")]
public sealed class ToolboxMeController : ControllerBase
{
    private readonly IToolFavoriteService _favoriteService;
    private readonly IToolExecutionHistoryQueries _historyQueries;

    public ToolboxMeController(
        IToolFavoriteService favoriteService,
        IToolExecutionHistoryQueries historyQueries)
    {
        _favoriteService = favoriteService;
        _historyQueries = historyQueries;
    }

    [HttpGet("tool-favorites")]
    [OpenApiOperationId("ToolboxMe_ListFavorites")]
    [OpenApiSummary("List tool favorites", "Returns the authenticated user's favorite tools.")]
    [ProducesResponseType(typeof(IReadOnlyList<ToolFavoriteDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IReadOnlyList<ToolFavoriteDto>>> ListFavorites(
        CancellationToken cancellationToken)
    {
        var userId = RequireUserId();
        return Ok(await _favoriteService.GetUserFavoritesAsync(userId, cancellationToken));
    }

    [HttpPut("tool-favorites/{toolId:guid}")]
    [OpenApiOperationId("ToolboxMe_AddFavorite")]
    [OpenApiSummary("Add tool favorite", "Adds a tool to the authenticated user's favorites.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> AddFavorite(Guid toolId, CancellationToken cancellationToken)
    {
        var userId = RequireUserId();
        await _favoriteService.AddAsync(userId, toolId, cancellationToken);
        return NoContent();
    }

    [HttpDelete("tool-favorites/{toolId:guid}")]
    [OpenApiOperationId("ToolboxMe_RemoveFavorite")]
    [OpenApiSummary("Remove tool favorite", "Removes a tool from the authenticated user's favorites.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> RemoveFavorite(Guid toolId, CancellationToken cancellationToken)
    {
        var userId = RequireUserId();
        await _favoriteService.RemoveAsync(userId, toolId, cancellationToken);
        return NoContent();
    }

    [HttpGet("tool-history")]
    [OpenApiOperationId("ToolboxMe_ListHistory")]
    [OpenApiSummary("List tool execution history", "Returns the authenticated user's tool execution history.")]
    [ProducesResponseType(typeof(ToolExecutionHistoryPageDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ToolExecutionHistoryPageDto>> ListHistory(
        [FromQuery] Guid? toolId,
        [FromQuery] bool? succeeded,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = ToolboxPaging.DefaultPageSize,
        CancellationToken cancellationToken = default)
    {
        var userId = RequireUserId();
        var pageDto = await _historyQueries.GetMyHistoryAsync(
            userId,
            new ToolExecutionHistoryFilter(toolId, succeeded, page, pageSize),
            cancellationToken);
        return Ok(pageDto);
    }

    [HttpGet("tool-history/{id:guid}")]
    [OpenApiOperationId("ToolboxMe_GetHistoryItem")]
    [OpenApiSummary("Get tool execution history item", "Returns a single tool execution history record.")]
    [ProducesResponseType(typeof(ToolExecutionHistoryItemDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ToolExecutionHistoryItemDto>> GetHistoryItem(
        Guid id,
        CancellationToken cancellationToken)
    {
        var userId = RequireUserId();
        var item = await _historyQueries.GetMyExecutionAsync(userId, id, cancellationToken);
        if (item is null)
        {
            throw new ToolboxException(
                "Execution history was not found.",
                ToolboxApplicationErrorCodes.HistoryNotFound);
        }

        return Ok(item);
    }

    private Guid RequireUserId()
    {
        var userId = User.GetUserId();
        if (userId is null)
        {
            throw new ToolboxException(
                "Authentication is required.",
                ToolboxApplicationErrorCodes.FavoriteRequiresAuthentication);
        }

        return userId.Value;
    }
}

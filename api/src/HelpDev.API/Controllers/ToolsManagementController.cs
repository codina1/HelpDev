using Asp.Versioning;
using HelpDev.API.Contracts;
using HelpDev.API.Extensions;
using HelpDev.API.OpenApi;
using HelpDev.Modules.Content.Application.Contents;
using HelpDev.Modules.Content.Application.Tools;
using HelpDev.Modules.Content.Application.Tools.Dtos;
using HelpDev.Modules.Identity.Application.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HelpDev.API.Controllers;

/// <summary>
/// Tool Library list (WriterOrAdmin). Ownership-scoped via ContentManagementActor.
/// Named *Management* (not *Admin*) because the Admin class-name prefix is reserved
/// for AdminOnly controllers.
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[ApiAudience(ApiAudiences.Authenticated)]
[Tags(ApiTags.Content)]
[Route("api/admin/tools")]
[Route("api/v{version:apiVersion}/admin/tools")]
[Authorize(Policy = AuthorizationPolicies.WriterOrAdmin)]
public sealed class ToolsManagementController : ControllerBase
{
    private readonly IToolQueries _toolQueries;

    public ToolsManagementController(IToolQueries toolQueries)
    {
        _toolQueries = toolQueries;
    }

    [HttpGet]
    [OpenApiOperationId("ToolsManagement_List")]
    [OpenApiSummary("List tools", "Lists tool library entries joined to Content. Writers see only their own.")]
    [ProducesResponseType(typeof(IReadOnlyList<ToolListItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IReadOnlyList<ToolListItemDto>>> List(CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        var actor = new ContentManagementActor(
            userId.Value,
            canManageAllContent: User.IsInRole(AppRoles.Admin));

        var items = await _toolQueries.ListAsync(actor, cancellationToken);
        return Ok(items);
    }
}

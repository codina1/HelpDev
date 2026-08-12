using Asp.Versioning;
using HelpDev.API.Contracts;
using HelpDev.API.Extensions;
using HelpDev.API.OpenApi;
using HelpDev.Modules.Content.Application.Common;
using HelpDev.Modules.Content.Application.Contents;
using HelpDev.Modules.Content.Application.SeoAnalysis.Dashboard;
using HelpDev.Modules.Identity.Application.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HelpDev.API.Controllers;

[ApiController]
[ApiVersion("1.0")]
[ApiAudience(ApiAudiences.Authenticated)]
[Tags(ApiTags.Content)]
[Route("api/admin/seo")]
[Route("api/v{version:apiVersion}/admin/seo")]
[Authorize(Policy = AuthorizationPolicies.WriterOrAdmin)]
public sealed class SeoDashboardController : ControllerBase
{
    private readonly ISeoDashboardQueries _dashboardQueries;

    public SeoDashboardController(ISeoDashboardQueries dashboardQueries)
    {
        _dashboardQueries = dashboardQueries;
    }

    [HttpGet("dashboard")]
    [OpenApiOperationId("SeoDashboard_Get")]
    [OpenApiSummary("SEO dashboard", "Aggregate SEO metadata coverage from stored content. No scores or persisted analysis history.")]
    [ProducesResponseType(typeof(SeoDashboardDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<SeoDashboardDto>> GetDashboard(CancellationToken cancellationToken)
    {
        if (!TryResolveActor(out var actor, out var unauthorized))
        {
            return unauthorized;
        }

        var dashboard = await _dashboardQueries.GetAsync(actor, cancellationToken);
        return Ok(dashboard);
    }

    private bool TryResolveActor(
        out ContentManagementActor actor,
        out ActionResult unauthorized)
    {
        var userId = User.GetUserId();
        if (userId is null)
        {
            actor = null!;
            unauthorized = Unauthorized();
            return false;
        }

        actor = new ContentManagementActor(
            userId.Value,
            canManageAllContent: User.IsInRole(AppRoles.Admin));
        unauthorized = null!;
        return true;
    }
}

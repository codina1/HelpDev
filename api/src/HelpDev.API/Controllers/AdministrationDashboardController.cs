using Asp.Versioning;
using HelpDev.API.Contracts;
using HelpDev.API.Extensions;
using HelpDev.API.OpenApi;
using HelpDev.Modules.Administration.Application.Dashboard;
using HelpDev.Modules.Identity.Application.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HelpDev.API.Controllers;

[ApiController]
[ApiVersion("1.0")]
[ApiAudience(ApiAudiences.Admin)]
[Tags(ApiTags.Administration)]
[Authorize(Policy = AuthorizationPolicies.AdminOnly)]
[Route("api/admin/dashboard")]
[Route("api/v{version:apiVersion}/admin/dashboard")]
public sealed class AdministrationDashboardController : ControllerBase
{
    private readonly IAdministrationDashboardQueries _queries;

    public AdministrationDashboardController(IAdministrationDashboardQueries queries)
    {
        _queries = queries;
    }

    [HttpGet]
    [OpenApiOperationId("AdministrationDashboard_Get")]
    [OpenApiSummary("Get admin dashboard", "Returns administration dashboard summary metrics.")]
    [ProducesResponseType(typeof(AdministrationDashboardDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<AdministrationDashboardDto>> Get(CancellationToken cancellationToken)
    {
        var dashboard = await _queries.GetAsync(cancellationToken);
        return Ok(dashboard);
    }
}

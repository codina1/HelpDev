using Asp.Versioning;
using HelpDev.API.Contracts;
using HelpDev.API.OpenApi;
using HelpDev.Modules.Analytics.Application.Queries;
using HelpDev.Modules.Identity.Application.Auth;
using HelpDev.SharedContracts.Ai;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HelpDev.API.Controllers;

[ApiController]
[ApiVersion("1.0")]
[ApiAudience(ApiAudiences.Admin)]
[Tags(ApiTags.Analytics)]
[Authorize(Policy = AuthorizationPolicies.AdminOnly)]
[Route("api/admin/ai")]
[Route("api/v{version:apiVersion}/admin/ai")]
public sealed class AiAdminController : ControllerBase
{
    private readonly IAiAnalyticsQueries _queries;

    public AiAdminController(IAiAnalyticsQueries queries)
    {
        _queries = queries;
    }

    [HttpGet]
    [OpenApiOperationId("AiAdmin_GetDashboard")]
    [OpenApiSummary("AI operations dashboard", "Real persisted AI usage metrics only. Never includes prompts or generated text.")]
    [ProducesResponseType(typeof(AiDashboardDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    public Task<AiDashboardDto> GetDashboard(CancellationToken cancellationToken) =>
        _queries.GetDashboardAsync(cancellationToken);

    [HttpGet("policy")]
    [OpenApiOperationId("AiAdmin_GetPolicy")]
    [OpenApiSummary("AI governance policy", "Admin documentation of HelpDev AI rules.")]
    [ProducesResponseType(typeof(AiPolicyDto), StatusCodes.Status200OK)]
    public ActionResult<AiPolicyDto> GetPolicy() =>
        Ok(new AiPolicyDto(AiPolicy.DocumentTitle, AiPolicy.Rules));
}

public sealed record AiPolicyDto(string Title, IReadOnlyList<string> Rules);

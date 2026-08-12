using Asp.Versioning;
using HelpDev.API.Contracts;
using HelpDev.API.OpenApi;
using HelpDev.Modules.Auditing.Application.Queries;
using HelpDev.Modules.Auditing.Domain;
using HelpDev.Modules.Identity.Application.Auth;
using HelpDev.SharedContracts.Auditing;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace HelpDev.API.Controllers;

[ApiController]
[ApiVersion("1.0")]
[ApiAudience(ApiAudiences.Admin)]
[Tags(ApiTags.Audit)]
[Route("api/admin/audit")]
[Route("api/v{version:apiVersion}/admin/audit")]
[Authorize(Policy = AuthorizationPolicies.AdminOnly)]
[EnableRateLimiting(Security.RateLimitPolicyNames.AdminMutation)]
public sealed class AuditAdminController : ControllerBase
{
    private readonly IAuditQueries _queries;

    public AuditAdminController(IAuditQueries queries)
    {
        _queries = queries;
    }

    [HttpGet]
    [OpenApiOperationId("AuditAdmin_GetPage")]
    [OpenApiSummary("List audit records", "Returns a paginated list of audit records.")]
    [ProducesResponseType(typeof(AuditPageResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<AuditPageResult>> GetPage(
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] string? category,
        [FromQuery] string? action,
        [FromQuery] string? outcome,
        [FromQuery] Guid? actorUserId,
        [FromQuery] Guid? subjectId,
        [FromQuery] string? subjectType,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var result = await _queries.GetPageAsync(
            new AuditQueryFilter(from, to, category, action, outcome, actorUserId, subjectId, subjectType, page, pageSize),
            cancellationToken);

        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [OpenApiOperationId("AuditAdmin_GetById")]
    [OpenApiSummary("Get audit record", "Returns a single audit record by identifier.")]
    [ProducesResponseType(typeof(AuditRecordDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AuditRecordDto>> GetById(Guid id, CancellationToken cancellationToken = default)
    {
        var result = await _queries.GetByIdAsync(id, cancellationToken);
        return Ok(result);
    }

    [HttpGet("actions")]
    [OpenApiOperationId("AuditAdmin_GetActions")]
    [OpenApiSummary("List audit actions", "Returns supported audit action identifiers.")]
    [ProducesResponseType(typeof(IReadOnlyList<string>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    public ActionResult<IReadOnlyList<string>> GetActions() =>
        Ok(GetSupportedActions());

    [HttpGet("categories")]
    [OpenApiOperationId("AuditAdmin_GetCategories")]
    [OpenApiSummary("List audit categories", "Returns supported audit category identifiers.")]
    [ProducesResponseType(typeof(IReadOnlyList<string>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    public ActionResult<IReadOnlyList<string>> GetCategories() =>
        Ok(GetSupportedCategories());

    private static IReadOnlyList<string> GetSupportedActions() =>
    [
        AuditActions.AuthenticationOtpRequested,
        AuditActions.AuthenticationOtpVerified,
        AuditActions.AuthenticationOtpVerificationFailed,
        AuditActions.AuthenticationRateLimited,
        AuditActions.AuthenticationLoginSucceeded,
        AuditActions.AuthorizationAccessDenied,
        AuditActions.AdministrationFeatureFlagCreated,
        AuditActions.AdministrationFeatureFlagUpdated,
        AuditActions.AdministrationFeatureFlagEnabled,
        AuditActions.AdministrationFeatureFlagDisabled,
        AuditActions.AdministrationSettingCreated,
        AuditActions.AdministrationSettingUpdated,
        AuditActions.SecurityRateLimitExceeded,
    ];

    private static IReadOnlyList<string> GetSupportedCategories() =>
    [
        AuditCategories.Authentication,
        AuditCategories.Authorization,
        AuditCategories.Administration,
        AuditCategories.ToolboxManagement,
        AuditCategories.PromptManagement,
        AuditCategories.OutboxOperations,
        AuditCategories.Security,
    ];
}

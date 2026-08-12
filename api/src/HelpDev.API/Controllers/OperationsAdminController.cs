using Asp.Versioning;
using HelpDev.API.Contracts;
using HelpDev.API.Deployment;
using HelpDev.API.OpenApi;
using HelpDev.API.Security;
using HelpDev.Modules.Identity.Application.Auth;
using HelpDev.SharedContracts.Observability;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace HelpDev.API.Controllers;

[ApiController]
[ApiVersion("1.0")]
[ApiAudience(ApiAudiences.Admin)]
[Tags(ApiTags.Operations)]
[Route("api/admin/operations")]
[Route("api/v{version:apiVersion}/admin/operations")]
[Authorize(Policy = AuthorizationPolicies.AdminOnly)]
[EnableRateLimiting(RateLimitPolicyNames.AdminMutation)]
public sealed class OperationsAdminController : ControllerBase
{
    private readonly IOperationalStatusService _statusService;
    private readonly IOutboxOperationalQueries _outboxQueries;
    private readonly ISearchOperationalQueries _searchQueries;
    private readonly IAnalyticsOperationalQueries _analyticsQueries;
    private readonly IAuditOperationalQueries _auditQueries;
    private readonly IReleaseInfoProvider _releaseInfoProvider;
    private readonly IConfiguration _configuration;

    public OperationsAdminController(
        IOperationalStatusService statusService,
        IOutboxOperationalQueries outboxQueries,
        ISearchOperationalQueries searchQueries,
        IAnalyticsOperationalQueries analyticsQueries,
        IAuditOperationalQueries auditQueries,
        IReleaseInfoProvider releaseInfoProvider,
        IConfiguration configuration)
    {
        _statusService = statusService;
        _outboxQueries = outboxQueries;
        _searchQueries = searchQueries;
        _analyticsQueries = analyticsQueries;
        _auditQueries = auditQueries;
        _releaseInfoProvider = releaseInfoProvider;
        _configuration = configuration;
    }

    [HttpGet("version")]
    [OpenApiOperationId("OperationsAdmin_GetVersion")]
    [OpenApiSummary("Get release metadata", "Returns sanitized, Admin-only release and version metadata.")]
    [ProducesResponseType(typeof(ReleaseInfoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    public ActionResult<ReleaseInfoDto> GetVersion() =>
        Ok(_releaseInfoProvider.GetReleaseInfo());

    [HttpGet("status")]
    [OpenApiOperationId("OperationsAdmin_GetStatus")]
    [OpenApiSummary("Get operations summary", "Returns a high-level operational status summary.")]
    [ProducesResponseType(typeof(OperationsSummaryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<OperationsSummaryDto>> GetStatus(CancellationToken cancellationToken) =>
        Ok(await _statusService.GetSummaryAsync(cancellationToken));

    [HttpGet("health")]
    [OpenApiOperationId("OperationsAdmin_GetHealth")]
    [OpenApiSummary("Get detailed health", "Returns detailed health status for platform subsystems.")]
    [ProducesResponseType(typeof(OperationalStatusDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<OperationalStatusDto>> GetHealth(CancellationToken cancellationToken) =>
        Ok(await _statusService.GetDetailedStatusAsync(cancellationToken));

    [HttpGet("outbox")]
    [OpenApiOperationId("OperationsAdmin_GetOutbox")]
    [OpenApiSummary("Get outbox snapshot", "Returns an operational snapshot of the outbox.")]
    [ProducesResponseType(typeof(OutboxOperationalSnapshot), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<OutboxOperationalSnapshot>> GetOutbox(CancellationToken cancellationToken) =>
        Ok(await _outboxQueries.GetSnapshotAsync(cancellationToken));

    [HttpGet("search")]
    [OpenApiOperationId("OperationsAdmin_GetSearch")]
    [OpenApiSummary("Get search snapshot", "Returns an operational snapshot of the search index.")]
    [ProducesResponseType(typeof(SearchOperationalSnapshot), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<SearchOperationalSnapshot>> GetSearch(CancellationToken cancellationToken) =>
        Ok(await _searchQueries.GetSnapshotAsync(cancellationToken));

    [HttpGet("analytics")]
    [OpenApiOperationId("OperationsAdmin_GetAnalytics")]
    [OpenApiSummary("Get analytics snapshot", "Returns an operational snapshot of analytics ingestion.")]
    [ProducesResponseType(typeof(AnalyticsOperationalSnapshot), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<AnalyticsOperationalSnapshot>> GetAnalytics(CancellationToken cancellationToken) =>
        Ok(await _analyticsQueries.GetSnapshotAsync(cancellationToken));

    [HttpGet("audit")]
    [OpenApiOperationId("OperationsAdmin_GetAudit")]
    [OpenApiSummary("Get audit snapshot", "Returns an operational snapshot of audit storage.")]
    [ProducesResponseType(typeof(AuditOperationalSnapshot), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<AuditOperationalSnapshot>> GetAudit(CancellationToken cancellationToken) =>
        Ok(await _auditQueries.GetSnapshotAsync(cancellationToken));

    [HttpGet("logging")]
    [OpenApiOperationId("OperationsAdmin_GetLogging")]
    [OpenApiSummary("Get logging configuration", "Returns current logging configuration settings.")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    public ActionResult<object> GetLogging()
    {
        var defaultLevel = _configuration["Logging:LogLevel:Default"] ?? "Information";
        return Ok(new
        {
            minimumLogLevel = defaultLevel,
            redactionEnabled = true,
        });
    }
}

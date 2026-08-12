using Asp.Versioning;
using HelpDev.API.Contracts;
using HelpDev.API.Extensions;
using HelpDev.API.OpenApi;
using HelpDev.Infrastructure.Outbox.Operations;
using HelpDev.Modules.Identity.Application.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HelpDev.API.Controllers;

[ApiController]
[ApiVersion("1.0")]
[ApiAudience(ApiAudiences.Admin)]
[Tags(ApiTags.Outbox)]
[Authorize(Policy = AuthorizationPolicies.AdminOnly)]
[Route("api/admin/outbox")]
[Route("api/v{version:apiVersion}/admin/outbox")]
public sealed class OutboxManagementController : ControllerBase
{
    private readonly IOutboxOperationsQueries _queries;
    private readonly IOutboxOperationsService _service;

    public OutboxManagementController(
        IOutboxOperationsQueries queries,
        IOutboxOperationsService service)
    {
        _queries = queries;
        _service = service;
    }

    [HttpGet("status")]
    [OpenApiOperationId("OutboxManagement_GetStatus")]
    [OpenApiSummary("Get outbox status", "Returns aggregate outbox processing status.")]
    [ProducesResponseType(typeof(OutboxStatusDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<OutboxStatusDto>> GetStatus(CancellationToken cancellationToken)
    {
        var status = await _queries.GetStatusAsync(cancellationToken);
        return Ok(status);
    }

    [HttpGet("messages")]
    [OpenApiOperationId("OutboxManagement_ListMessages")]
    [OpenApiSummary("List outbox messages", "Returns a paginated list of outbox messages.")]
    [ProducesResponseType(typeof(OutboxMessagePageDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<OutboxMessagePageDto>> ListMessages(
        [FromQuery] string? status,
        [FromQuery] string? type,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = OutboxOperationsQueries.DefaultPageSize,
        CancellationToken cancellationToken = default)
    {
        var result = await _queries.ListAsync(
            new OutboxMessageFilter(status, type, page, pageSize),
            cancellationToken);
        return Ok(result);
    }

    [HttpGet("messages/{id:guid}")]
    [OpenApiOperationId("OutboxManagement_GetMessage")]
    [OpenApiSummary("Get outbox message", "Returns a single outbox message by identifier.")]
    [ProducesResponseType(typeof(OutboxMessageDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OutboxMessageDetailDto>> GetMessage(
        Guid id,
        CancellationToken cancellationToken)
    {
        var detail = await _queries.GetByIdAsync(id, cancellationToken);
        if (detail is null)
        {
            throw new OutboxOperationsException(
                "Outbox message was not found.",
                OutboxOperationsErrorCodes.MessageNotFound);
        }

        return Ok(detail);
    }

    [HttpPost("messages/{id:guid}/retry")]
    [OpenApiOperationId("OutboxManagement_RetryMessage")]
    [OpenApiSummary("Retry outbox message", "Retries processing of a failed outbox message.")]
    [ProducesResponseType(typeof(OutboxMessageDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<OutboxMessageDetailDto>> RetryMessage(
        Guid id,
        CancellationToken cancellationToken)
    {
        var detail = await _service.RetryAsync(id, User.GetUserId(), cancellationToken);
        return Ok(detail);
    }

    [HttpPost("retry-failed")]
    [OpenApiOperationId("OutboxManagement_RetryFailed")]
    [OpenApiSummary("Retry failed messages", "Retries a batch of failed outbox messages.")]
    [ProducesResponseType(typeof(RetryFailedOutboxResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<RetryFailedOutboxResultDto>> RetryFailed(
        [FromBody] RetryFailedOutboxHttpRequest? request,
        CancellationToken cancellationToken)
    {
        var limit = request?.Limit ?? OutboxOperationsService.DefaultRetryFailedLimit;
        var result = await _service.RetryFailedAsync(
            new RetryFailedOutboxRequest(limit, request?.Type),
            User.GetUserId(),
            cancellationToken);
        return Ok(result);
    }
}

public sealed class RetryFailedOutboxHttpRequest
{
    public int? Limit { get; set; }

    public string? Type { get; set; }
}

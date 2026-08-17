using Asp.Versioning;
using HelpDev.API.Contracts;
using HelpDev.API.Extensions;
using HelpDev.API.OpenApi;
using HelpDev.Modules.Identity.Application.Auth;
using HelpDev.Modules.PromptLab.Application;
using HelpDev.Modules.PromptLab.Application.Catalog;
using HelpDev.Modules.PromptLab.Application.Prompts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HelpDev.API.Controllers;

[ApiController]
[ApiVersion("1.0")]
[ApiAudience(ApiAudiences.Admin)]
[Tags(ApiTags.PromptLab)]
[Authorize(Policy = AuthorizationPolicies.AdminOnly)]
[Route("api/admin/prompts")]
[Route("api/v{version:apiVersion}/admin/prompts")]
public sealed class PromptLabAdminReviewController : ControllerBase
{
    private readonly IPromptAdminReviewQueries _queries;
    private readonly IPromptAdminReviewService _reviewService;

    public PromptLabAdminReviewController(
        IPromptAdminReviewQueries queries,
        IPromptAdminReviewService reviewService)
    {
        _queries = queries;
        _reviewService = reviewService;
    }

    [HttpGet]
    [OpenApiOperationId("PromptLabAdminReview_List")]
    [OpenApiSummary(
        "List prompts for review",
        "Returns writer library prompts filtered by Submitted, Approved, or Rejected.")]
    [ProducesResponseType(typeof(AdminPromptReviewPageDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<AdminPromptReviewPageDto>> List(
        [FromQuery] string status = "Submitted",
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = PromptLabPaging.DefaultPageSize,
        CancellationToken cancellationToken = default)
    {
        return Ok(await _queries.GetPromptsAsync(
            new AdminPromptReviewFilter(status, page, pageSize),
            cancellationToken));
    }

    [HttpGet("{id:guid}")]
    [OpenApiOperationId("PromptLabAdminReview_GetById")]
    [OpenApiSummary("Get prompt for review", "Returns a writer library prompt for admin review.")]
    [ProducesResponseType(typeof(AdminPromptReviewDetailsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AdminPromptReviewDetailsDto>> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var prompt = await _queries.GetByIdAsync(id, cancellationToken);
        if (prompt is null)
        {
            throw new PromptLabException(
                "Prompt was not found.",
                PromptLabApplicationErrorCodes.PromptNotFound);
        }

        return Ok(prompt);
    }

    [HttpPost("{id:guid}/approve")]
    [OpenApiOperationId("PromptLabAdminReview_Approve")]
    [OpenApiSummary("Approve prompt", "Admin: Submitted → Approved. The prompt becomes public.")]
    [ProducesResponseType(typeof(AdminPromptReviewDetailsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<AdminPromptReviewDetailsDto>> Approve(
        Guid id,
        CancellationToken cancellationToken)
    {
        return Ok(await _reviewService.ApproveAsync(RequireUserId(), id, cancellationToken));
    }

    [HttpPost("{id:guid}/reject")]
    [OpenApiOperationId("PromptLabAdminReview_Reject")]
    [OpenApiSummary("Reject prompt", "Admin: Submitted → Rejected with a required reason.")]
    [ProducesResponseType(typeof(AdminPromptReviewDetailsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<AdminPromptReviewDetailsDto>> Reject(
        Guid id,
        [FromBody] RejectAdminPromptRequest request,
        CancellationToken cancellationToken)
    {
        return Ok(await _reviewService.RejectAsync(RequireUserId(), id, request, cancellationToken));
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

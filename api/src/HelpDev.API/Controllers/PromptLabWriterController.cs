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
[ApiAudience(ApiAudiences.Authenticated)]
[Tags(ApiTags.PromptLab)]
[Authorize(Policy = AuthorizationPolicies.WriterOrAdmin)]
[Route("api/writer/prompts")]
[Route("api/v{version:apiVersion}/writer/prompts")]
public sealed class PromptLabWriterController : ControllerBase
{
    private readonly IPromptWriterService _writerService;
    private readonly IPromptWriterQueries _writerQueries;

    public PromptLabWriterController(
        IPromptWriterService writerService,
        IPromptWriterQueries writerQueries)
    {
        _writerService = writerService;
        _writerQueries = writerQueries;
    }

    [HttpPost]
    [OpenApiOperationId("PromptLabWriter_Create")]
    [OpenApiSummary(
        "Create prompt",
        "Creates a draft prompt owned by the authenticated writer. It is not published automatically.")]
    [ProducesResponseType(typeof(WriterPromptDetailsDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<WriterPromptDetailsDto>> Create(
        [FromBody] CreateWriterPromptRequest request,
        CancellationToken cancellationToken)
    {
        var created = await _writerService.CreateAsync(RequireUserId(), request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpGet]
    [OpenApiOperationId("PromptLabWriter_List")]
    [OpenApiSummary("List my prompts", "Returns prompts owned by the authenticated writer.")]
    [ProducesResponseType(typeof(WriterPromptPageDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<WriterPromptPageDto>> List(
        [FromQuery] string? status,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = PromptLabPaging.DefaultPageSize,
        CancellationToken cancellationToken = default)
    {
        var result = await _writerQueries.GetMyPromptsAsync(
            RequireUserId(),
            new WriterPromptFilter(status, page, pageSize),
            cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [OpenApiOperationId("PromptLabWriter_GetById")]
    [OpenApiSummary("Get my prompt", "Returns a prompt owned by the authenticated writer.")]
    [ProducesResponseType(typeof(WriterPromptDetailsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<WriterPromptDetailsDto>> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var prompt = await _writerQueries.GetMyByIdAsync(RequireUserId(), id, cancellationToken);
        if (prompt is null)
        {
            throw new PromptLabException(
                "Prompt was not found.",
                PromptLabApplicationErrorCodes.PromptNotFound);
        }

        return Ok(prompt);
    }

    [HttpPut("{id:guid}")]
    [OpenApiOperationId("PromptLabWriter_Update")]
    [OpenApiSummary(
        "Update my prompt",
        "Updates a draft prompt owned by the authenticated writer. Submitted prompts cannot be edited.")]
    [ProducesResponseType(typeof(WriterPromptDetailsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<WriterPromptDetailsDto>> Update(
        Guid id,
        [FromBody] UpdateWriterPromptRequest request,
        CancellationToken cancellationToken)
    {
        return Ok(await _writerService.UpdateAsync(RequireUserId(), id, request, cancellationToken));
    }

    [HttpPost("{id:guid}/submit")]
    [OpenApiOperationId("PromptLabWriter_Submit")]
    [OpenApiSummary(
        "Submit my prompt",
        "Moves a draft prompt to Submitted for review. It is not approved or published.")]
    [ProducesResponseType(typeof(WriterPromptDetailsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<WriterPromptDetailsDto>> Submit(
        Guid id,
        CancellationToken cancellationToken)
    {
        return Ok(await _writerService.SubmitAsync(RequireUserId(), id, cancellationToken));
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

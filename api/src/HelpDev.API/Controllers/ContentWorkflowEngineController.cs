using Asp.Versioning;
using HelpDev.API.Contracts;
using HelpDev.API.Extensions;
using HelpDev.API.OpenApi;
using HelpDev.Modules.Content.Application.AiWorkflow;
using HelpDev.Modules.Content.Application.Contents;
using HelpDev.Modules.Identity.Application.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HelpDev.API.Controllers;

[ApiController]
[ApiVersion("1.0")]
[ApiAudience(ApiAudiences.Authenticated)]
[Tags(ApiTags.Content)]
[Route("api/admin/content/workflows")]
[Route("api/v{version:apiVersion}/admin/content/workflows")]
[Authorize(Policy = AuthorizationPolicies.WriterOrAdmin)]
public sealed class ContentWorkflowEngineController : ControllerBase
{
    private readonly IAiContentWorkflowService _workflowService;

    public ContentWorkflowEngineController(IAiContentWorkflowService workflowService)
    {
        _workflowService = workflowService;
    }

    [HttpPost]
    [OpenApiOperationId("ContentWorkflowEngine_Create")]
    [OpenApiSummary("Create AI content workflow", "Creates a content idea and workflow session. AI never auto-publishes.")]
    [ProducesResponseType(typeof(AiContentWorkflowSessionDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<AiContentWorkflowSessionDto>> Create(
        [FromBody] CreateAiContentWorkflowRequest request,
        CancellationToken cancellationToken)
    {
        if (!TryResolveActor(out var actor, out var unauthorized))
        {
            return unauthorized;
        }

        return Ok(await _workflowService.CreateAsync(actor, request, cancellationToken));
    }

    [HttpGet]
    [OpenApiOperationId("ContentWorkflowEngine_List")]
    [OpenApiSummary("List AI content workflows", "Writers see own workflows; admins see all.")]
    [ProducesResponseType(typeof(IReadOnlyList<AiContentWorkflowListItemDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<AiContentWorkflowListItemDto>>> List(
        CancellationToken cancellationToken)
    {
        if (!TryResolveActor(out var actor, out var unauthorized))
        {
            return unauthorized;
        }

        return Ok(await _workflowService.ListAsync(actor, cancellationToken));
    }

    [HttpGet("{id:guid}")]
    [OpenApiOperationId("ContentWorkflowEngine_GetById")]
    [ProducesResponseType(typeof(AiContentWorkflowSessionDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<AiContentWorkflowSessionDto>> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        if (!TryResolveActor(out var actor, out var unauthorized))
        {
            return unauthorized;
        }

        return Ok(await _workflowService.GetByIdAsync(actor, id, cancellationToken));
    }

    [HttpPost("{id:guid}/research")]
    [OpenApiOperationId("ContentWorkflowEngine_Research")]
    [ProducesResponseType(typeof(AiResearchResultDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<AiResearchResultDto>> Research(
        Guid id,
        CancellationToken cancellationToken)
    {
        if (!TryResolveActor(out var actor, out var unauthorized))
        {
            return unauthorized;
        }

        return Ok(await _workflowService.ResearchAsync(actor, id, cancellationToken));
    }

    [HttpPost("{id:guid}/outline")]
    [OpenApiOperationId("ContentWorkflowEngine_Outline")]
    [ProducesResponseType(typeof(ContentOutlineDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<ContentOutlineDto>> Outline(
        Guid id,
        [FromBody] GenerateOutlineRequest? request,
        CancellationToken cancellationToken)
    {
        if (!TryResolveActor(out var actor, out var unauthorized))
        {
            return unauthorized;
        }

        return Ok(await _workflowService.GenerateOutlineAsync(
            actor,
            id,
            request ?? new GenerateOutlineRequest(null),
            cancellationToken));
    }

    [HttpPost("{id:guid}/draft")]
    [OpenApiOperationId("ContentWorkflowEngine_Draft")]
    [ProducesResponseType(typeof(DraftSuggestionDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<DraftSuggestionDto>> Draft(
        Guid id,
        [FromBody] GenerateDraftRequest request,
        CancellationToken cancellationToken)
    {
        if (!TryResolveActor(out var actor, out var unauthorized))
        {
            return unauthorized;
        }

        return Ok(await _workflowService.GenerateDraftAsync(actor, id, request, cancellationToken));
    }

    [HttpPost("{id:guid}/seo")]
    [OpenApiOperationId("ContentWorkflowEngine_Seo")]
    [ProducesResponseType(typeof(SeoOptimizationSuggestionDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<SeoOptimizationSuggestionDto>> Seo(
        Guid id,
        [FromBody] GenerateSeoRequest request,
        CancellationToken cancellationToken)
    {
        if (!TryResolveActor(out var actor, out var unauthorized))
        {
            return unauthorized;
        }

        return Ok(await _workflowService.GenerateSeoAsync(actor, id, request, cancellationToken));
    }

    [HttpPost("{id:guid}/apply-draft")]
    [OpenApiOperationId("ContentWorkflowEngine_ApplyDraft")]
    [OpenApiSummary("Apply AI draft", "Creates Draft content + revision. Does not publish.")]
    [ProducesResponseType(typeof(ApplyDraftResultDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApplyDraftResultDto>> ApplyDraft(
        Guid id,
        [FromBody] ApplyDraftRequest request,
        CancellationToken cancellationToken)
    {
        if (!TryResolveActor(out var actor, out var unauthorized))
        {
            return unauthorized;
        }

        return Ok(await _workflowService.ApplyDraftAsync(actor, id, request, cancellationToken));
    }

    private bool TryResolveActor(out ContentManagementActor actor, out ActionResult unauthorized)
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

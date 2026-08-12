using Asp.Versioning;
using HelpDev.API.Contracts;
using HelpDev.API.Extensions;
using HelpDev.API.OpenApi;
using HelpDev.Modules.Identity.Application.Auth;
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
[Route("api/admin/prompt-lab/prompts")]
[Route("api/v{version:apiVersion}/admin/prompt-lab/prompts")]
public sealed class PromptLabAdminPromptsController : ControllerBase
{
    private readonly IPromptDefinitionService _service;

    public PromptLabAdminPromptsController(IPromptDefinitionService service)
    {
        _service = service;
    }

    [HttpGet]
    [OpenApiOperationId("PromptLabAdminPrompts_List")]
    [OpenApiSummary("List prompt definitions", "Returns a paginated list of prompt definitions.")]
    [ProducesResponseType(typeof(PromptDefinitionPageDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<PromptDefinitionPageDto>> List(
        [FromQuery] Guid? categoryId,
        [FromQuery] string? purpose,
        [FromQuery] string? visibility,
        [FromQuery] bool? isPublished,
        [FromQuery] bool? isEnabled,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = PromptLabPaging.DefaultPageSize,
        CancellationToken cancellationToken = default)
    {
        var result = await _service.GetPageAsync(
            new PromptDefinitionFilter(categoryId, purpose, visibility, isPublished, isEnabled, page, pageSize),
            cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [OpenApiOperationId("PromptLabAdminPrompts_GetById")]
    [OpenApiSummary("Get prompt definition", "Returns a prompt definition by identifier.")]
    [ProducesResponseType(typeof(PromptDefinitionAdminDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<PromptDefinitionAdminDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        return Ok(await _service.GetByIdAsync(id, cancellationToken));
    }

    [HttpPost]
    [OpenApiOperationId("PromptLabAdminPrompts_Create")]
    [OpenApiSummary("Create prompt definition", "Creates a new prompt definition draft.")]
    [ProducesResponseType(typeof(PromptDefinitionAdminDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<PromptDefinitionAdminDto>> Create(
        [FromBody] CreatePromptDefinitionRequest request,
        CancellationToken cancellationToken)
    {
        var created = await _service.CreateDraftAsync(request, User.GetUserId(), cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:guid}")]
    [OpenApiOperationId("PromptLabAdminPrompts_Update")]
    [OpenApiSummary("Update prompt definition", "Updates a prompt definition's metadata.")]
    [ProducesResponseType(typeof(PromptDefinitionAdminDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<PromptDefinitionAdminDto>> Update(
        Guid id,
        [FromBody] UpdatePromptDefinitionRequest request,
        CancellationToken cancellationToken)
    {
        return Ok(await _service.UpdateMetadataAsync(id, request, User.GetUserId(), cancellationToken));
    }

    [HttpPost("{id:guid}/enable")]
    [OpenApiOperationId("PromptLabAdminPrompts_Enable")]
    [OpenApiSummary("Enable prompt", "Enables a prompt definition.")]
    [ProducesResponseType(typeof(PromptDefinitionAdminDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<PromptDefinitionAdminDto>> Enable(Guid id, CancellationToken cancellationToken)
    {
        return Ok(await _service.EnableAsync(id, User.GetUserId(), cancellationToken));
    }

    [HttpPost("{id:guid}/disable")]
    [OpenApiOperationId("PromptLabAdminPrompts_Disable")]
    [OpenApiSummary("Disable prompt", "Disables a prompt definition.")]
    [ProducesResponseType(typeof(PromptDefinitionAdminDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<PromptDefinitionAdminDto>> Disable(Guid id, CancellationToken cancellationToken)
    {
        return Ok(await _service.DisableAsync(id, User.GetUserId(), cancellationToken));
    }

    [HttpPost("{id:guid}/unpublish")]
    [OpenApiOperationId("PromptLabAdminPrompts_Unpublish")]
    [OpenApiSummary("Unpublish prompt", "Unpublishes a prompt definition.")]
    [ProducesResponseType(typeof(PromptDefinitionAdminDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<PromptDefinitionAdminDto>> Unpublish(Guid id, CancellationToken cancellationToken)
    {
        return Ok(await _service.UnpublishAsync(id, User.GetUserId(), cancellationToken));
    }

    [HttpGet("{id:guid}/versions")]
    [OpenApiOperationId("PromptLabAdminPrompts_ListVersions")]
    [OpenApiSummary("List prompt versions", "Returns all versions of a prompt definition.")]
    [ProducesResponseType(typeof(IReadOnlyList<PromptVersionAdminDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IReadOnlyList<PromptVersionAdminDto>>> ListVersions(
        Guid id,
        CancellationToken cancellationToken)
    {
        return Ok(await _service.GetVersionsAsync(id, cancellationToken));
    }

    [HttpGet("{id:guid}/versions/{versionNumber:int}")]
    [OpenApiOperationId("PromptLabAdminPrompts_GetVersion")]
    [OpenApiSummary("Get prompt version", "Returns a specific version of a prompt definition.")]
    [ProducesResponseType(typeof(PromptVersionAdminDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<PromptVersionAdminDto>> GetVersion(
        Guid id,
        int versionNumber,
        CancellationToken cancellationToken)
    {
        return Ok(await _service.GetVersionAsync(id, versionNumber, cancellationToken));
    }

    [HttpPost("{id:guid}/versions")]
    [OpenApiOperationId("PromptLabAdminPrompts_CreateVersion")]
    [OpenApiSummary("Create prompt version", "Creates a new version of a prompt definition.")]
    [ProducesResponseType(typeof(PromptVersionAdminDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<PromptVersionAdminDto>> CreateVersion(
        Guid id,
        [FromBody] CreatePromptVersionRequest request,
        CancellationToken cancellationToken)
    {
        var created = await _service.CreateVersionAsync(id, request, User.GetUserId(), cancellationToken);
        return CreatedAtAction(
            nameof(GetVersion),
            new { id, versionNumber = created.VersionNumber },
            created);
    }

    [HttpPost("{id:guid}/versions/{versionNumber:int}/publish")]
    [OpenApiOperationId("PromptLabAdminPrompts_PublishVersion")]
    [OpenApiSummary("Publish prompt version", "Publishes a specific version of a prompt definition.")]
    [ProducesResponseType(typeof(PromptDefinitionAdminDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<PromptDefinitionAdminDto>> PublishVersion(
        Guid id,
        int versionNumber,
        CancellationToken cancellationToken)
    {
        return Ok(await _service.PublishVersionAsync(id, versionNumber, User.GetUserId(), cancellationToken));
    }
}

using Asp.Versioning;
using HelpDev.API.Contracts;
using HelpDev.API.Extensions;
using HelpDev.API.OpenApi;
using HelpDev.Modules.Identity.Application.Auth;
using HelpDev.Modules.Toolbox.Application.Catalog;
using HelpDev.Modules.Toolbox.Application.Tools;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HelpDev.API.Controllers;

[ApiController]
[ApiVersion("1.0")]
[ApiAudience(ApiAudiences.Admin)]
[Tags(ApiTags.Toolbox)]
[Authorize(Policy = AuthorizationPolicies.AdminOnly)]
[Route("api/admin/toolbox/tools")]
[Route("api/v{version:apiVersion}/admin/toolbox/tools")]
public sealed class ToolboxAdminToolsController : ControllerBase
{
    private readonly IToolDefinitionService _service;

    public ToolboxAdminToolsController(IToolDefinitionService service)
    {
        _service = service;
    }

    [HttpGet]
    [OpenApiOperationId("ToolboxAdminTools_List")]
    [OpenApiSummary("List tool definitions", "Returns a paginated list of tool definitions.")]
    [ProducesResponseType(typeof(ToolDefinitionPageDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ToolDefinitionPageDto>> List(
        [FromQuery] Guid? categoryId,
        [FromQuery] string? type,
        [FromQuery] bool? isPublished,
        [FromQuery] bool? isEnabled,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = ToolboxPaging.DefaultPageSize,
        CancellationToken cancellationToken = default)
    {
        var result = await _service.GetPageAsync(
            new ToolDefinitionFilter(categoryId, type, isPublished, isEnabled, page, pageSize),
            cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [OpenApiOperationId("ToolboxAdminTools_GetById")]
    [OpenApiSummary("Get tool definition", "Returns a tool definition by identifier.")]
    [ProducesResponseType(typeof(ToolDefinitionAdminDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ToolDefinitionAdminDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        return Ok(await _service.GetByIdAsync(id, cancellationToken));
    }

    [HttpPost]
    [OpenApiOperationId("ToolboxAdminTools_Create")]
    [OpenApiSummary("Create tool definition", "Creates a new tool definition draft.")]
    [ProducesResponseType(typeof(ToolDefinitionAdminDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ToolDefinitionAdminDto>> Create(
        [FromBody] CreateToolDefinitionRequest request,
        CancellationToken cancellationToken)
    {
        var created = await _service.CreateDraftAsync(request, User.GetUserId(), cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:guid}")]
    [OpenApiOperationId("ToolboxAdminTools_Update")]
    [OpenApiSummary("Update tool definition", "Updates a tool definition's metadata.")]
    [ProducesResponseType(typeof(ToolDefinitionAdminDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ToolDefinitionAdminDto>> Update(
        Guid id,
        [FromBody] UpdateToolDefinitionRequest request,
        CancellationToken cancellationToken)
    {
        return Ok(await _service.UpdateAsync(id, request, User.GetUserId(), cancellationToken));
    }

    [HttpPut("{id:guid}/schema")]
    [OpenApiOperationId("ToolboxAdminTools_UpdateSchema")]
    [OpenApiSummary("Update tool schema", "Updates a tool definition's input/output schema.")]
    [ProducesResponseType(typeof(ToolDefinitionAdminDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ToolDefinitionAdminDto>> UpdateSchema(
        Guid id,
        [FromBody] UpdateToolSchemaRequest request,
        CancellationToken cancellationToken)
    {
        return Ok(await _service.UpdateSchemaAsync(id, request, User.GetUserId(), cancellationToken));
    }

    [HttpPost("{id:guid}/publish")]
    [OpenApiOperationId("ToolboxAdminTools_Publish")]
    [OpenApiSummary("Publish tool", "Publishes a tool definition.")]
    [ProducesResponseType(typeof(ToolDefinitionAdminDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ToolDefinitionAdminDto>> Publish(Guid id, CancellationToken cancellationToken)
    {
        return Ok(await _service.PublishAsync(id, User.GetUserId(), cancellationToken));
    }

    [HttpPost("{id:guid}/unpublish")]
    [OpenApiOperationId("ToolboxAdminTools_Unpublish")]
    [OpenApiSummary("Unpublish tool", "Unpublishes a tool definition.")]
    [ProducesResponseType(typeof(ToolDefinitionAdminDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ToolDefinitionAdminDto>> Unpublish(Guid id, CancellationToken cancellationToken)
    {
        return Ok(await _service.UnpublishAsync(id, User.GetUserId(), cancellationToken));
    }

    [HttpPost("{id:guid}/enable")]
    [OpenApiOperationId("ToolboxAdminTools_Enable")]
    [OpenApiSummary("Enable tool", "Enables a tool definition.")]
    [ProducesResponseType(typeof(ToolDefinitionAdminDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ToolDefinitionAdminDto>> Enable(Guid id, CancellationToken cancellationToken)
    {
        return Ok(await _service.EnableAsync(id, User.GetUserId(), cancellationToken));
    }

    [HttpPost("{id:guid}/disable")]
    [OpenApiOperationId("ToolboxAdminTools_Disable")]
    [OpenApiSummary("Disable tool", "Disables a tool definition.")]
    [ProducesResponseType(typeof(ToolDefinitionAdminDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ToolDefinitionAdminDto>> Disable(Guid id, CancellationToken cancellationToken)
    {
        return Ok(await _service.DisableAsync(id, User.GetUserId(), cancellationToken));
    }
}

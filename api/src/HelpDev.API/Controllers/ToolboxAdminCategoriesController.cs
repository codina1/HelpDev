using Asp.Versioning;
using HelpDev.API.Contracts;
using HelpDev.API.Extensions;
using HelpDev.API.OpenApi;
using HelpDev.Modules.Identity.Application.Auth;
using HelpDev.Modules.Toolbox.Application.Categories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HelpDev.API.Controllers;

[ApiController]
[ApiVersion("1.0")]
[ApiAudience(ApiAudiences.Admin)]
[Tags(ApiTags.Toolbox)]
[Authorize(Policy = AuthorizationPolicies.AdminOnly)]
[Route("api/admin/toolbox/categories")]
[Route("api/v{version:apiVersion}/admin/toolbox/categories")]
public sealed class ToolboxAdminCategoriesController : ControllerBase
{
    private readonly IToolCategoryService _service;

    public ToolboxAdminCategoriesController(IToolCategoryService service)
    {
        _service = service;
    }

    [HttpGet]
    [OpenApiOperationId("ToolboxAdminCategories_List")]
    [OpenApiSummary("List tool categories", "Returns all tool categories for administration.")]
    [ProducesResponseType(typeof(IReadOnlyList<ToolCategoryAdminDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IReadOnlyList<ToolCategoryAdminDto>>> List(
        CancellationToken cancellationToken)
    {
        return Ok(await _service.GetAllAsync(cancellationToken));
    }

    [HttpGet("{id:guid}")]
    [OpenApiOperationId("ToolboxAdminCategories_GetById")]
    [OpenApiSummary("Get tool category", "Returns a tool category by identifier.")]
    [ProducesResponseType(typeof(ToolCategoryAdminDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ToolCategoryAdminDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        return Ok(await _service.GetByIdAsync(id, cancellationToken));
    }

    [HttpPost]
    [OpenApiOperationId("ToolboxAdminCategories_Create")]
    [OpenApiSummary("Create tool category", "Creates a new tool category.")]
    [ProducesResponseType(typeof(ToolCategoryAdminDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ToolCategoryAdminDto>> Create(
        [FromBody] CreateToolCategoryRequest request,
        CancellationToken cancellationToken)
    {
        var created = await _service.CreateAsync(request, User.GetUserId(), cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:guid}")]
    [OpenApiOperationId("ToolboxAdminCategories_Update")]
    [OpenApiSummary("Update tool category", "Updates a tool category's metadata.")]
    [ProducesResponseType(typeof(ToolCategoryAdminDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ToolCategoryAdminDto>> Update(
        Guid id,
        [FromBody] UpdateToolCategoryRequest request,
        CancellationToken cancellationToken)
    {
        return Ok(await _service.UpdateAsync(id, request, User.GetUserId(), cancellationToken));
    }

    [HttpPost("{id:guid}/activate")]
    [OpenApiOperationId("ToolboxAdminCategories_Activate")]
    [OpenApiSummary("Activate tool category", "Activates a tool category.")]
    [ProducesResponseType(typeof(ToolCategoryAdminDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ToolCategoryAdminDto>> Activate(Guid id, CancellationToken cancellationToken)
    {
        return Ok(await _service.ActivateAsync(id, User.GetUserId(), cancellationToken));
    }

    [HttpPost("{id:guid}/deactivate")]
    [OpenApiOperationId("ToolboxAdminCategories_Deactivate")]
    [OpenApiSummary("Deactivate tool category", "Deactivates a tool category.")]
    [ProducesResponseType(typeof(ToolCategoryAdminDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ToolCategoryAdminDto>> Deactivate(Guid id, CancellationToken cancellationToken)
    {
        return Ok(await _service.DeactivateAsync(id, User.GetUserId(), cancellationToken));
    }
}

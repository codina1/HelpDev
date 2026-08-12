using Asp.Versioning;
using HelpDev.API.Contracts;
using HelpDev.API.Extensions;
using HelpDev.API.OpenApi;
using HelpDev.Modules.Identity.Application.Auth;
using HelpDev.Modules.PromptLab.Application.Categories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HelpDev.API.Controllers;

[ApiController]
[ApiVersion("1.0")]
[ApiAudience(ApiAudiences.Admin)]
[Tags(ApiTags.PromptLab)]
[Authorize(Policy = AuthorizationPolicies.AdminOnly)]
[Route("api/admin/prompt-lab/categories")]
[Route("api/v{version:apiVersion}/admin/prompt-lab/categories")]
public sealed class PromptLabAdminCategoriesController : ControllerBase
{
    private readonly IPromptCategoryService _service;

    public PromptLabAdminCategoriesController(IPromptCategoryService service)
    {
        _service = service;
    }

    [HttpGet]
    [OpenApiOperationId("PromptLabAdminCategories_List")]
    [OpenApiSummary("List prompt categories", "Returns all prompt categories for administration.")]
    [ProducesResponseType(typeof(IReadOnlyList<PromptCategoryAdminDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IReadOnlyList<PromptCategoryAdminDto>>> List(
        CancellationToken cancellationToken)
    {
        return Ok(await _service.GetAllAsync(cancellationToken));
    }

    [HttpGet("{id:guid}")]
    [OpenApiOperationId("PromptLabAdminCategories_GetById")]
    [OpenApiSummary("Get prompt category", "Returns a prompt category by identifier.")]
    [ProducesResponseType(typeof(PromptCategoryAdminDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<PromptCategoryAdminDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        return Ok(await _service.GetByIdAsync(id, cancellationToken));
    }

    [HttpPost]
    [OpenApiOperationId("PromptLabAdminCategories_Create")]
    [OpenApiSummary("Create prompt category", "Creates a new prompt category.")]
    [ProducesResponseType(typeof(PromptCategoryAdminDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<PromptCategoryAdminDto>> Create(
        [FromBody] CreatePromptCategoryRequest request,
        CancellationToken cancellationToken)
    {
        var created = await _service.CreateAsync(request, User.GetUserId(), cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:guid}")]
    [OpenApiOperationId("PromptLabAdminCategories_Update")]
    [OpenApiSummary("Update prompt category", "Updates a prompt category's metadata.")]
    [ProducesResponseType(typeof(PromptCategoryAdminDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<PromptCategoryAdminDto>> Update(
        Guid id,
        [FromBody] UpdatePromptCategoryRequest request,
        CancellationToken cancellationToken)
    {
        return Ok(await _service.UpdateAsync(id, request, User.GetUserId(), cancellationToken));
    }

    [HttpPost("{id:guid}/activate")]
    [OpenApiOperationId("PromptLabAdminCategories_Activate")]
    [OpenApiSummary("Activate prompt category", "Activates a prompt category.")]
    [ProducesResponseType(typeof(PromptCategoryAdminDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<PromptCategoryAdminDto>> Activate(Guid id, CancellationToken cancellationToken)
    {
        return Ok(await _service.ActivateAsync(id, User.GetUserId(), cancellationToken));
    }

    [HttpPost("{id:guid}/deactivate")]
    [OpenApiOperationId("PromptLabAdminCategories_Deactivate")]
    [OpenApiSummary("Deactivate prompt category", "Deactivates a prompt category.")]
    [ProducesResponseType(typeof(PromptCategoryAdminDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<PromptCategoryAdminDto>> Deactivate(Guid id, CancellationToken cancellationToken)
    {
        return Ok(await _service.DeactivateAsync(id, User.GetUserId(), cancellationToken));
    }
}

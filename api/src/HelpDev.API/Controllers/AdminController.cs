using Asp.Versioning;
using HelpDev.API.Contracts;
using HelpDev.API.OpenApi;
using HelpDev.Application.Admin;
using HelpDev.Modules.Identity.Application.Auth;
using HelpDev.Modules.Identity.Application.Profiles;
using HelpDev.API.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HelpDev.API.Controllers;

[ApiController]
[ApiVersion("1.0")]
[ApiAudience(ApiAudiences.Admin)]
[Tags(ApiTags.Administration)]
[Route("api/admin")]
[Route("api/v{version:apiVersion}/admin")]
[Authorize(Policy = AuthorizationPolicies.AdminOnly)]
public sealed class AdminController : ControllerBase
{
    private readonly IAdminUserService _adminUserService;

    public AdminController(IAdminUserService adminUserService)
    {
        _adminUserService = adminUserService;
    }

    [HttpGet("users")]
    [OpenApiOperationId("Admin_ListUsers")]
    [OpenApiSummary("List users", "Returns all registered users.")]
    [ProducesResponseType(typeof(IReadOnlyList<AdminUserListItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IReadOnlyList<AdminUserListItemDto>>> ListUsers(
        CancellationToken cancellationToken)
    {
        var users = await _adminUserService.ListUsersAsync(cancellationToken);
        return Ok(users);
    }

    [HttpGet("users/{id:guid}")]
    [OpenApiOperationId("Admin_GetUser")]
    [OpenApiSummary("Get user", "Returns a user by identifier.")]
    [ProducesResponseType(typeof(AdminUserDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AdminUserDetailDto>> GetUser(
        Guid id,
        CancellationToken cancellationToken)
    {
        try
        {
            var user = await _adminUserService.GetUserAsync(id, cancellationToken);
            return Ok(user);
        }
        catch (ProfileException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpPut("users/{id:guid}")]
    [OpenApiOperationId("Admin_UpdateUser")]
    [OpenApiSummary("Update user", "Updates a user's profile and role.")]
    [ProducesResponseType(typeof(AdminUserDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AdminUserDetailDto>> UpdateUser(
        Guid id,
        [FromBody] UpdateAdminUserRequest request,
        CancellationToken cancellationToken)
    {
        var actorId = User.GetUserId();
        if (actorId is null)
        {
            return Unauthorized();
        }

        try
        {
            var user = await _adminUserService.UpdateUserAsync(
                actorId.Value,
                id,
                request,
                cancellationToken);
            return Ok(user);
        }
        catch (ProfileException ex)
        {
            if (ex.Message.Contains("یافت نشد", StringComparison.Ordinal))
            {
                return NotFound(new { message = ex.Message });
            }

            return BadRequest(new { message = ex.Message });
        }
    }
}

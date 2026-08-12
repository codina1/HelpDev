using Asp.Versioning;
using HelpDev.API.Contracts;
using HelpDev.API.Extensions;
using HelpDev.API.OpenApi;
using HelpDev.Modules.Identity.Application.Auth;
using HelpDev.Modules.Identity.Application.Profiles;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HelpDev.API.Controllers;

[ApiController]
[ApiVersion("1.0")]
[ApiAudience(ApiAudiences.Authenticated)]
[Tags(ApiTags.Profile)]
[Route("api/profile")]
[Route("api/v{version:apiVersion}/profile")]
[Authorize(Policy = AuthorizationPolicies.Authenticated)]
public sealed class ProfileController : ControllerBase
{
    private readonly IProfileService _profileService;

    public ProfileController(IProfileService profileService)
    {
        _profileService = profileService;
    }

    [HttpGet("me")]
    [OpenApiOperationId("Profile_GetMyProfile")]
    [OpenApiSummary("Get my profile", "Returns the authenticated user's profile.")]
    [ProducesResponseType(typeof(ProfileDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProfileDto>> GetMyProfile(CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        try
        {
            var profile = await _profileService.GetMyProfileAsync(userId.Value, cancellationToken);
            return Ok(profile);
        }
        catch (ProfileException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpPut("me")]
    [OpenApiOperationId("Profile_UpdateMyProfile")]
    [OpenApiSummary("Update my profile", "Updates the authenticated user's profile.")]
    [ProducesResponseType(typeof(ProfileDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ProfileDto>> UpdateMyProfile(
        [FromBody] UpdateProfileRequest request,
        CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        try
        {
            var profile = await _profileService.UpdateMyProfileAsync(userId.Value, request, cancellationToken);
            return Ok(profile);
        }
        catch (ProfileException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}

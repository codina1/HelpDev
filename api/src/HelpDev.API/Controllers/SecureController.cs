using System.Security.Claims;
using Asp.Versioning;
using HelpDev.API.Contracts;
using HelpDev.API.Extensions;
using HelpDev.API.OpenApi;
using HelpDev.Modules.Identity.Application.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HelpDev.API.Controllers;

[ApiController]
[ApiVersion("1.0")]
[ApiAudience(ApiAudiences.Authenticated)]
[Tags(ApiTags.Authentication)]
[Route("api/secure")]
[Route("api/v{version:apiVersion}/secure")]
[Authorize(Policy = AuthorizationPolicies.Authenticated)]
public sealed class SecureController : ControllerBase
{
    [HttpGet]
    [OpenApiOperationId("Secure_Get")]
    [OpenApiSummary("Secure probe", "Returns the authenticated user's identity claims.")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    public IActionResult Get()
    {
        return Ok(new
        {
            message = "Access granted.",
            userId = User.FindFirstValue(JwtClaimTypes.UserId),
            role = User.FindFirstValue(JwtClaimTypes.Role),
            mobile = User.FindFirstValue(JwtClaimTypes.Mobile),
        });
    }
}

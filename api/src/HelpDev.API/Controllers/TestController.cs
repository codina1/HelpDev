using System.Security.Claims;
using Asp.Versioning;
using HelpDev.Application.Test;
using HelpDev.Application.Test.Dtos;
using HelpDev.API.Contracts;
using HelpDev.API.Extensions;
using HelpDev.API.OpenApi;
using HelpDev.Infrastructure.Persistence;
using HelpDev.Modules.Identity.Application.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HelpDev.API.Controllers;

[ApiController]
[ApiVersion("1.0")]
[ApiExplorerSettings(IgnoreApi = true)]
[Route("api/test")]
[Route("api/v{version:apiVersion}/test")]
public sealed class TestController : ControllerBase
{
    private readonly ITestService _testService;
    private readonly IDatabaseConnectionChecker _database;

    public TestController(ITestService testService, IDatabaseConnectionChecker database)
    {
        _testService = testService;
        _database = database;
    }

    /// <summary>
    /// Verifies JWT (any authenticated role) and published content in the database.
    /// </summary>
    [HttpGet("content")]
    [Authorize(Policy = AuthorizationPolicies.Authenticated)]
    [OpenApiOperationId("Test_GetContent")]
    [OpenApiSummary("Test content access", "Verifies JWT and published content in the database.")]
    [ProducesResponseType(typeof(TestContentResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(TestContentResponse), StatusCodes.Status503ServiceUnavailable)]
    public async Task<ActionResult<TestContentResponse>> GetContent(CancellationToken cancellationToken)
    {
        var authentication = GetAuthInfo();
        if (authentication is null)
        {
            return Unauthorized();
        }

        var databaseConnected = await _database.CanConnectAsync(cancellationToken);
        var response = await _testService.GetContentSummaryAsync(
            authentication,
            databaseConnected,
            cancellationToken);

        return databaseConnected
            ? Ok(response)
            : StatusCode(StatusCodes.Status503ServiceUnavailable, response);
    }

    /// <summary>
    /// Verifies JWT, Admin role policy, and users in the database.
    /// </summary>
    [HttpGet("users")]
    [Authorize(Policy = AuthorizationPolicies.AdminOnly)]
    [OpenApiOperationId("Test_GetUsers")]
    [OpenApiSummary("Test admin access", "Verifies JWT, Admin role policy, and users in the database.")]
    [ProducesResponseType(typeof(TestUsersResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(TestUsersResponse), StatusCodes.Status503ServiceUnavailable)]
    public async Task<ActionResult<TestUsersResponse>> GetUsers(CancellationToken cancellationToken)
    {
        var authentication = GetAuthInfo();
        if (authentication is null)
        {
            return Unauthorized();
        }

        var databaseConnected = await _database.CanConnectAsync(cancellationToken);
        var response = await _testService.GetUsersSummaryAsync(
            authentication,
            databaseConnected,
            cancellationToken);

        return databaseConnected
            ? Ok(response)
            : StatusCode(StatusCodes.Status503ServiceUnavailable, response);
    }

    private TestAuthInfoDto? GetAuthInfo()
    {
        var userId = User.GetUserId();
        var role = User.FindFirstValue(JwtClaimTypes.Role);
        var mobile = User.FindFirstValue(JwtClaimTypes.Mobile);

        if (userId is null || string.IsNullOrWhiteSpace(role) || string.IsNullOrWhiteSpace(mobile))
        {
            return null;
        }

        return new TestAuthInfoDto(userId.Value, role, mobile);
    }
}

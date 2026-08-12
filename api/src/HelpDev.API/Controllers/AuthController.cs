using Asp.Versioning;
using HelpDev.API.Contracts;
using HelpDev.API.OpenApi;
using HelpDev.Modules.Identity.Application.Auth;
using HelpDev.Modules.Identity.Application.Auth.Dtos;
using HelpDev.API.Security;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Npgsql;

namespace HelpDev.API.Controllers;

[ApiController]
[ApiVersion("1.0")]
[ApiAudience(ApiAudiences.Public)]
[Tags(ApiTags.Authentication)]
[Route("api/auth")]
[Route("api/v{version:apiVersion}/auth")]
public sealed class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IAuthService authService, ILogger<AuthController> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    [HttpPost("send-otp")]
    [EnableRateLimiting(RateLimitPolicyNames.OtpRequest)]
    [RequestSizeLimit(16 * 1024)]
    [OpenApiOperationId("Auth_RequestOtp")]
    [OpenApiSummary("Request a login OTP", "Requests a one-time password for mobile login. OTP values are never returned in Production. Resend, attempt, and expiration limits apply.")]
    [ProducesResponseType(typeof(SendOtpResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<SendOtpResponse>> SendOtp(
        [FromBody] SendOtpRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var response = await _authService.SendOtpAsync(request, cancellationToken);
            return Ok(response);
        }
        catch (AuthException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("verify-otp")]
    [EnableRateLimiting(RateLimitPolicyNames.OtpVerify)]
    [RequestSizeLimit(16 * 1024)]
    [OpenApiOperationId("Auth_VerifyOtp")]
    [OpenApiSummary("Verify OTP", "Verifies a one-time password and issues a JWT access token. Failed attempts and expiration limits apply.")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status503ServiceUnavailable)]
    public async Task<ActionResult<AuthResponse>> VerifyOtp(
        [FromBody] VerifyOtpRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var response = await _authService.VerifyOtpAsync(request, cancellationToken);
            return Ok(response);
        }
        catch (AuthException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex) when (IsDatabaseUnavailable(ex))
        {
            _logger.LogWarning(ex, "Database unavailable during OTP verification.");
            return StatusCode(
                StatusCodes.Status503ServiceUnavailable,
                new { message = "دیتابیس در دسترس نیست. PostgreSQL را روی پورت 5432 اجرا کنید." });
        }
    }

    private static bool IsDatabaseUnavailable(Exception exception) =>
        exception is NpgsqlException or InvalidOperationException;
}

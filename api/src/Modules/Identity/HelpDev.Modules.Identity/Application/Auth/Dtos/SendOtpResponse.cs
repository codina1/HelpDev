namespace HelpDev.Modules.Identity.Application.Auth.Dtos;

public sealed record SendOtpResponse(
    string Message,
    int ExpiresInSeconds,
    string? Otp = null);

using HelpDev.Modules.Identity.Application.Auth.Dtos;

namespace HelpDev.Modules.Identity.Application.Auth;

public interface IAuthService
{
    Task<SendOtpResponse> SendOtpAsync(SendOtpRequest request, CancellationToken cancellationToken = default);

    Task<AuthResponse> VerifyOtpAsync(VerifyOtpRequest request, CancellationToken cancellationToken = default);
}

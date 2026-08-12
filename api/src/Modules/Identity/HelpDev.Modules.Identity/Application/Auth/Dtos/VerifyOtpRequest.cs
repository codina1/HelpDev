namespace HelpDev.Modules.Identity.Application.Auth.Dtos;

public sealed class VerifyOtpRequest
{
    public string Mobile { get; set; } = string.Empty;

    public string Code { get; set; } = string.Empty;
}

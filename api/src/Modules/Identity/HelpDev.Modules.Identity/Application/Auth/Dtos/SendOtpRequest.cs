namespace HelpDev.Modules.Identity.Application.Auth.Dtos;

public sealed class SendOtpRequest
{
    public string Mobile { get; set; } = string.Empty;
}

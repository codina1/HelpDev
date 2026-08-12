namespace HelpDev.Modules.Identity.Application.Auth;

public class AuthSettings
{
    public const string SectionName = "Auth";

    public int OtpExpirationMinutes { get; set; } = 5;

    public bool ExposeOtpInResponse { get; set; }
}

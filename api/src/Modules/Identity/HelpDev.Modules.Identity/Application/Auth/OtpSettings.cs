namespace HelpDev.Modules.Identity.Application.Auth;

public class OtpSettings
{
    public const string SectionName = "Otp";

    public int MaxFailedAttempts { get; set; } = 5;
}

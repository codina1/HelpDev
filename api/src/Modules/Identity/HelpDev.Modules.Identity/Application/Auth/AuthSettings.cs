namespace HelpDev.Modules.Identity.Application.Auth;

public class AuthSettings
{
    public const string SectionName = "Auth";

    public int OtpExpirationMinutes { get; set; } = 5;

    public bool ExposeOtpInResponse { get; set; }

    /// <summary>
    /// Explicit temporary escape hatch for pre-SMS production testing.
    /// Keep false once a real OTP delivery provider is configured.
    /// </summary>
    public bool AllowOtpExposureInProduction { get; set; }
}

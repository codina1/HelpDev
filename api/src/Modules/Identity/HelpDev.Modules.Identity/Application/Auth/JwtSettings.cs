namespace HelpDev.Modules.Identity.Application.Auth;

public class JwtSettings
{
    public const string SectionName = "Jwt";

    public string Secret { get; set; } = string.Empty;

    public string Issuer { get; set; } = "HelpDev";

    public string Audience { get; set; } = "HelpDev.Client";

    public int ExpirationMinutes { get; set; } = 60;
}

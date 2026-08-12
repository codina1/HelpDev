namespace HelpDev.Modules.Identity.Application.Auth;

public static class AuthorizationPolicies
{
    public const string Authenticated = "Authenticated";

    public const string WriterOrAdmin = "WriterOrAdmin";

    public const string AdminOnly = "AdminOnly";
}

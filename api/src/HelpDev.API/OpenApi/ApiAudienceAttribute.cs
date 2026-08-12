namespace HelpDev.API.OpenApi;

/// <summary>
/// Declares the intended consumer audience for an API endpoint.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
public sealed class ApiAudienceAttribute : Attribute
{
    public ApiAudienceAttribute(string audience)
    {
        Audience = audience;
    }

    public string Audience { get; }
}

public static class ApiAudiences
{
    public const string Public = "public";
    public const string Authenticated = "authenticated";
    public const string Admin = "admin";
    public const string Operations = "operations";
    public const string InternalCompatibility = "internal-compatibility";
}

public static class ApiTags
{
    public const string Authentication = "Authentication";
    public const string Profile = "Profile";
    public const string Content = "Content";
    public const string Learning = "Learning";
    public const string Search = "Search";
    public const string Toolbox = "Toolbox";
    public const string PromptLab = "PromptLab";
    public const string Administration = "Administration";
    public const string Analytics = "Analytics";
    public const string Audit = "Audit";
    public const string Media = "Media";
    public const string Operations = "Operations";
    public const string Outbox = "Outbox";
    public const string Health = "Health";
}

public static class OpenApiDocumentNames
{
    public const string PublicV1 = "public-v1";
    public const string AuthenticatedV1 = "authenticated-v1";
    public const string AdminV1 = "admin-v1";
    public const string AllV1 = "all-v1";
}

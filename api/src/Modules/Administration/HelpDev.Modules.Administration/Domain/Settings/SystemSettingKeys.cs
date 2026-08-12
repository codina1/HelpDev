namespace HelpDev.Modules.Administration.Domain.Settings;

/// <summary>
/// Suggested safe initial setting keys. Not auto-seeded in this sprint.
/// </summary>
public static class SystemSettingKeys
{
    public const string SiteName = "SiteName";
    public const string SiteDescription = "SiteDescription";
    public const string SupportEmail = "SupportEmail";
    public const string DefaultLanguage = "DefaultLanguage";
    public const string DefaultPageSize = "DefaultPageSize";
    public const string MaxUploadSize = "MaxUploadSize";

    /// <summary>Boolean — feature toggle for Content AI Assistant (no secrets).</summary>
    public const string AiEnabled = "Ai.Enabled";

    /// <summary>String — display name of default model (not an API key).</summary>
    public const string AiDefaultModel = "Ai.DefaultModel";

    /// <summary>String — comma-separated allow-list of ContentAiTaskType names.</summary>
    public const string AiAllowedTasks = "Ai.AllowedTasks";
}

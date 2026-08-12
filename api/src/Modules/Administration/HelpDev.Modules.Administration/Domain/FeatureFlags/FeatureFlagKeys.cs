namespace HelpDev.Modules.Administration.Domain.FeatureFlags;

/// <summary>
/// Suggested predefined keys. Managed and exposed only; not wired to runtime enforcement in this sprint.
/// </summary>
public static class FeatureFlagKeys
{
    public const string SearchEnabled = "SearchEnabled";
    public const string RegistrationEnabled = "RegistrationEnabled";
    public const string LearningEnabled = "LearningEnabled";
    public const string PromptLabEnabled = "PromptLabEnabled";
    public const string MaintenanceMode = "MaintenanceMode";

    public static IReadOnlyList<string> All { get; } =
    [
        SearchEnabled,
        RegistrationEnabled,
        LearningEnabled,
        PromptLabEnabled,
        MaintenanceMode,
    ];
}

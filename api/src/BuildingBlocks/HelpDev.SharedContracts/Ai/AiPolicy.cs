namespace HelpDev.SharedContracts.Ai;

/// <summary>
/// HelpDev AI governance policy (suggestion-only, human approval required).
/// Exposed to Admin documentation surfaces — not a runtime enforcement engine.
/// </summary>
public static class AiPolicy
{
    public const string DocumentTitle = "HelpDev AI Governance Policy";

    public const string HumanApprovalRequired =
        "Human approval is required before any AI-assisted content becomes published Content.";

    public const string NoAutomaticPublishing =
        "AI must never auto-publish, auto-approve, or bypass Draft → ReviewPending → Approved → Published.";

    public const string NoSecretTransmission =
        "API keys, tokens, and credentials must never appear in prompts, logs, audits, or usage records.";

    public const string NoPrivateDraftExport =
        "Private drafts and unpublished bodies must not be exported to external systems except via the controlled provider adapter.";

    public const string SuggestionOnly =
        "AI output is a suggestion only. Authors remain responsible for accuracy, tone, and publication.";

    public static IReadOnlyList<string> Rules { get; } =
    [
        HumanApprovalRequired,
        NoAutomaticPublishing,
        NoSecretTransmission,
        NoPrivateDraftExport,
        SuggestionOnly,
    ];
}

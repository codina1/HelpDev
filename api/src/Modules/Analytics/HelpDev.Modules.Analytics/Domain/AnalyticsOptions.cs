namespace HelpDev.Modules.Analytics.Domain;

public sealed class AnalyticsOptions
{
    public const string SectionName = "Analytics";

    public int EventReceiptRetentionDays { get; init; } = 90;

    public int MaxQueryRangeDays { get; init; } = 366;

    public int DefaultTopLimit { get; init; } = 10;

    public int MaxTopLimit { get; init; } = 100;
}

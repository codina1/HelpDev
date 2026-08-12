namespace HelpDev.Modules.Analytics.Domain;

public static class AnalyticsLimits
{
    public const int MaxDimensions = 10;
    public const int MaxDimensionKeyLength = 50;
    public const int MaxDimensionValueLength = 100;
    public const int MaxEventTypeLength = 80;
    public const int MaxMetricKeyLength = 80;
    public const int MaxSubjectTypeLength = 40;
    public const int MaxDisplayNameLength = 200;
    public const int MaxSlugLength = 150;
    public const int MaxErrorCodeLength = 100;
    public const int MaxProcessingStatusLength = 20;
    public const long MaxQuantity = 1_000;
}

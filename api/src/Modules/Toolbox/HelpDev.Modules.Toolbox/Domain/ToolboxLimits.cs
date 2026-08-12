namespace HelpDev.Modules.Toolbox.Domain;

public static class ToolboxLimits
{
    public const int MaxRequestBytes = 128 * 1024;
    public const int MaxTextLength = 100_000;
    public const int MaxJsonLength = 100_000;
    public const int MaxOutputLength = 200_000;
    public const int MaxRegexTextLength = 20_000;
    public const int MaxRegexPatternLength = 500;
    public const int MaxRegexMatches = 100;
    public const int MaxUuidCount = 100;
    public const int MaxHistoryInputPreview = 500;
    public const int MaxHistoryOutputPreview = 500;
    public const int DefaultRegexTimeoutMs = 200;
    public const int MinRegexTimeoutMs = 50;
    public const int MaxRegexTimeoutMs = 1000;
    public const int MaxCaptureValueLength = 200;
}

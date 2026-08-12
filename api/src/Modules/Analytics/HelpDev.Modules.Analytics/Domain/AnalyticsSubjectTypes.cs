namespace HelpDev.Modules.Analytics.Domain;

public static class AnalyticsSubjectTypes
{
    public const string Content = "Content";
    public const string Course = "Course";
    public const string Tool = "Tool";
    public const string Prompt = "Prompt";

    public static bool IsSupported(string? subjectType) =>
        subjectType switch
        {
            Content or Course or Tool or Prompt => true,
            _ => false,
        };
}

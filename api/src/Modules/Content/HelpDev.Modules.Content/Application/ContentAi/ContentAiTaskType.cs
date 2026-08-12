namespace HelpDev.Modules.Content.Application.ContentAi;

public enum ContentAiTaskType
{
    ContentAnalysis = 0,
    TitleSuggestion = 1,
    MetaDescription = 2,
    OutlineGeneration = 3,
    FaqGeneration = 4,
}

public static class ContentAiTaskTypeCatalog
{
    public static string ToWireName(ContentAiTaskType taskType) => taskType.ToString();

    public static bool TryParse(string? value, out ContentAiTaskType taskType) =>
        Enum.TryParse(value, ignoreCase: true, out taskType);
}

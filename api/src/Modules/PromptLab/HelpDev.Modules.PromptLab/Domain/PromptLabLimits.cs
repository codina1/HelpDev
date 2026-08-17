namespace HelpDev.Modules.PromptLab.Domain;

public static class PromptLabLimits
{
    public const int MaxTemplateLength = 20_000;
    public const int MaxRenderedLength = 50_000;
    public const int MaxVariablesPerVersion = 100;
    public const int MaxVariableNameLength = 100;
    public const int MaxVariableLabelLength = 150;
    public const int MaxVariableDescriptionLength = 500;
    public const int MaxVariableValueLength = 20_000;
    public const int MaxSelectOptions = 100;
    public const int MaxSelectOptionLength = 200;
    public const int MaxValidationPatternLength = 500;
    public const int ValidationRegexTimeoutMs = 200;
    public const int MaxHistoryInputPreview = 500;
    public const int MaxHistoryOutputPreview = 1000;
    public const int MaxChangeNotesLength = 1000;
    public const int MaxPromptContentLength = MaxTemplateLength;
    public const int MaxPromptCoverImageLength = 2048;
    public const int MaxPromptAiModelLength = 80;
    public const int MaxPromptPackItems = 100;
    public const int MaxPromptRejectionReasonLength = 2000;
    public const int AdminPromptPreviewLength = 280;
}

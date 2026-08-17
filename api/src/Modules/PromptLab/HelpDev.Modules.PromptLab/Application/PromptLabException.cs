namespace HelpDev.Modules.PromptLab.Application;

public sealed class PromptLabException : Exception
{
    public PromptLabException(string message, string code)
        : base(message)
    {
        Code = code;
    }

    public PromptLabException(string message, string code, Exception innerException)
        : base(message, innerException)
    {
        Code = code;
    }

    public string Code { get; }
}

public static class PromptLabApplicationErrorCodes
{
    public const string CategoryNotFound = Domain.PromptLabErrorCodes.CategoryNotFound;
    public const string CategoryNameRequired = Domain.PromptLabErrorCodes.CategoryNameRequired;
    public const string CategoryNameInvalid = Domain.PromptLabErrorCodes.CategoryNameInvalid;
    public const string CategorySlugRequired = Domain.PromptLabErrorCodes.CategorySlugRequired;
    public const string CategorySlugInvalid = Domain.PromptLabErrorCodes.CategorySlugInvalid;
    public const string CategorySlugDuplicate = Domain.PromptLabErrorCodes.CategorySlugDuplicate;
    public const string CategoryInactive = Domain.PromptLabErrorCodes.CategoryInactive;

    public const string PromptNotFound = Domain.PromptLabErrorCodes.PromptNotFound;
    public const string PromptNameRequired = Domain.PromptLabErrorCodes.PromptNameRequired;
    public const string PromptNameInvalid = Domain.PromptLabErrorCodes.PromptNameInvalid;
    public const string PromptSlugRequired = Domain.PromptLabErrorCodes.PromptSlugRequired;
    public const string PromptSlugInvalid = Domain.PromptLabErrorCodes.PromptSlugInvalid;
    public const string PromptSlugDuplicate = Domain.PromptLabErrorCodes.PromptSlugDuplicate;
    public const string PromptSummaryRequired = Domain.PromptLabErrorCodes.PromptSummaryRequired;
    public const string PromptSummaryInvalid = Domain.PromptLabErrorCodes.PromptSummaryInvalid;
    public const string PromptDisabled = Domain.PromptLabErrorCodes.PromptDisabled;
    public const string PromptUnpublished = Domain.PromptLabErrorCodes.PromptUnpublished;
    public const string PromptCategoryInvalid = Domain.PromptLabErrorCodes.PromptCategoryInvalid;
    public const string PromptCannotPublish = Domain.PromptLabErrorCodes.PromptCannotPublish;
    public const string PromptVersionNotFound = Domain.PromptLabErrorCodes.PromptVersionNotFound;
    public const string PromptVersionInvalid = Domain.PromptLabErrorCodes.PromptVersionInvalid;

    public const string TemplateRequired = Domain.PromptLabErrorCodes.TemplateRequired;
    public const string TemplateTooLong = Domain.PromptLabErrorCodes.TemplateTooLong;
    public const string TemplateSyntaxInvalid = Domain.PromptLabErrorCodes.TemplateSyntaxInvalid;
    public const string TemplatePlaceholderInvalid = Domain.PromptLabErrorCodes.TemplatePlaceholderInvalid;
    public const string TemplatePlaceholderDuplicate = Domain.PromptLabErrorCodes.TemplatePlaceholderDuplicate;
    public const string TemplateUnknownPlaceholder = Domain.PromptLabErrorCodes.TemplateUnknownPlaceholder;
    public const string TemplateUnusedVariable = Domain.PromptLabErrorCodes.TemplateUnusedVariable;
    public const string TemplateTooManyVariables = Domain.PromptLabErrorCodes.TemplateTooManyVariables;

    public const string VariableNameRequired = Domain.PromptLabErrorCodes.VariableNameRequired;
    public const string VariableNameInvalid = Domain.PromptLabErrorCodes.VariableNameInvalid;
    public const string VariableNameDuplicate = Domain.PromptLabErrorCodes.VariableNameDuplicate;
    public const string VariableNameReserved = Domain.PromptLabErrorCodes.VariableNameReserved;
    public const string VariableTypeInvalid = Domain.PromptLabErrorCodes.VariableTypeInvalid;
    public const string VariableDefaultInvalid = Domain.PromptLabErrorCodes.VariableDefaultInvalid;
    public const string VariableConstraintsInvalid = Domain.PromptLabErrorCodes.VariableConstraintsInvalid;
    public const string VariablePatternInvalid = Domain.PromptLabErrorCodes.VariablePatternInvalid;
    public const string VariableOptionsInvalid = Domain.PromptLabErrorCodes.VariableOptionsInvalid;

    public const string RenderInputInvalid = Domain.PromptLabErrorCodes.RenderInputInvalid;
    public const string RenderUnknownVariable = Domain.PromptLabErrorCodes.RenderUnknownVariable;
    public const string RenderRequiredVariableMissing = Domain.PromptLabErrorCodes.RenderRequiredVariableMissing;
    public const string RenderValueInvalid = Domain.PromptLabErrorCodes.RenderValueInvalid;
    public const string RenderValueTooLong = Domain.PromptLabErrorCodes.RenderValueTooLong;
    public const string RenderPatternTimeout = Domain.PromptLabErrorCodes.RenderPatternTimeout;
    public const string RenderOutputTooLong = Domain.PromptLabErrorCodes.RenderOutputTooLong;
    public const string RenderRequiresAuthentication = Domain.PromptLabErrorCodes.RenderRequiresAuthentication;
    public const string RenderFailed = Domain.PromptLabErrorCodes.RenderFailed;

    public const string FavoriteRequiresAuthentication = Domain.PromptLabErrorCodes.FavoriteRequiresAuthentication;
    public const string FavoriteInvalid = Domain.PromptLabErrorCodes.FavoriteInvalid;

    public const string HistoryNotFound = Domain.PromptLabErrorCodes.HistoryNotFound;
    public const string HistoryAccessDenied = Domain.PromptLabErrorCodes.HistoryAccessDenied;

    public const string PaginationInvalid = Domain.PromptLabErrorCodes.PaginationInvalid;

    public const string AiModelNotFound = Domain.PromptLabErrorCodes.AiModelNotFound;
    public const string AiModelInactive = Domain.PromptLabErrorCodes.AiModelInactive;
    public const string PromptTitleRequired = Domain.PromptLabErrorCodes.PromptTitleRequired;
    public const string PromptTitleInvalid = Domain.PromptLabErrorCodes.PromptTitleInvalid;
    public const string PromptContentRequired = Domain.PromptLabErrorCodes.PromptContentRequired;
    public const string PromptContentInvalid = Domain.PromptLabErrorCodes.PromptContentInvalid;
    public const string PromptMediaTypeInvalid = Domain.PromptLabErrorCodes.PromptMediaTypeInvalid;
    public const string PromptAiModelInvalid = Domain.PromptLabErrorCodes.PromptAiModelInvalid;
    public const string PromptCoverImageInvalid = Domain.PromptLabErrorCodes.PromptCoverImageInvalid;
    public const string PromptAuthorInvalid = Domain.PromptLabErrorCodes.PromptAuthorInvalid;
    public const string PromptNotDraft = Domain.PromptLabErrorCodes.PromptNotDraft;
    public const string PromptEditForbidden = Domain.PromptLabErrorCodes.PromptEditForbidden;
    public const string PromptStatusInvalid = Domain.PromptLabErrorCodes.PromptStatusInvalid;
    public const string PromptRejectionReasonRequired = Domain.PromptLabErrorCodes.PromptRejectionReasonRequired;
    public const string PromptRejectionReasonInvalid = Domain.PromptLabErrorCodes.PromptRejectionReasonInvalid;
}

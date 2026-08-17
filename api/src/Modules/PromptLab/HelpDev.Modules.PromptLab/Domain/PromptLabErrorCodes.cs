namespace HelpDev.Modules.PromptLab.Domain;

public static class PromptLabErrorCodes
{
    public const string CategoryNotFound = "promptlab_category_not_found";
    public const string CategoryNameRequired = "promptlab_category_name_required";
    public const string CategoryNameInvalid = "promptlab_category_name_invalid";
    public const string CategorySlugRequired = "promptlab_category_slug_required";
    public const string CategorySlugInvalid = "promptlab_category_slug_invalid";
    public const string CategorySlugDuplicate = "promptlab_category_slug_duplicate";
    public const string CategoryInactive = "promptlab_category_inactive";

    public const string AiModelNotFound = "promptlab_ai_model_not_found";
    public const string AiModelNameRequired = "promptlab_ai_model_name_required";
    public const string AiModelNameInvalid = "promptlab_ai_model_name_invalid";
    public const string AiModelSlugRequired = "promptlab_ai_model_slug_required";
    public const string AiModelSlugInvalid = "promptlab_ai_model_slug_invalid";
    public const string AiModelSlugDuplicate = "promptlab_ai_model_slug_duplicate";
    public const string AiModelProviderRequired = "promptlab_ai_model_provider_required";
    public const string AiModelProviderInvalid = "promptlab_ai_model_provider_invalid";
    public const string AiModelLogoInvalid = "promptlab_ai_model_logo_invalid";
    public const string AiModelInactive = "promptlab_ai_model_inactive";

    public const string PromptNotFound = "promptlab_prompt_not_found";
    public const string PromptNameRequired = "promptlab_prompt_name_required";
    public const string PromptNameInvalid = "promptlab_prompt_name_invalid";
    public const string PromptSlugRequired = "promptlab_prompt_slug_required";
    public const string PromptSlugInvalid = "promptlab_prompt_slug_invalid";
    public const string PromptSlugDuplicate = "promptlab_prompt_slug_duplicate";
    public const string PromptSummaryRequired = "promptlab_prompt_summary_required";
    public const string PromptSummaryInvalid = "promptlab_prompt_summary_invalid";
    public const string PromptDisabled = "promptlab_prompt_disabled";
    public const string PromptUnpublished = "promptlab_prompt_unpublished";
    public const string PromptCategoryInvalid = "promptlab_prompt_category_invalid";
    public const string PromptCannotPublish = "promptlab_prompt_cannot_publish";
    public const string PromptVersionNotFound = "promptlab_prompt_version_not_found";
    public const string PromptVersionInvalid = "promptlab_prompt_version_invalid";

    public const string PromptTitleRequired = "promptlab_prompt_title_required";
    public const string PromptTitleInvalid = "promptlab_prompt_title_invalid";
    public const string PromptContentRequired = "promptlab_prompt_content_required";
    public const string PromptContentInvalid = "promptlab_prompt_content_invalid";
    public const string PromptContentNotPublic = "promptlab_prompt_content_not_public";
    public const string PromptNotPublic = "promptlab_prompt_not_public";
    public const string PromptNotDraft = "promptlab_prompt_not_draft";
    public const string PromptEditForbidden = "promptlab_prompt_edit_forbidden";
    public const string PromptAuthorInvalid = "promptlab_prompt_author_invalid";
    public const string PromptMediaTypeInvalid = "promptlab_prompt_media_type_invalid";
    public const string PromptAiModelRequired = "promptlab_prompt_ai_model_required";
    public const string PromptAiModelInvalid = "promptlab_prompt_ai_model_invalid";
    public const string PromptCoverImageInvalid = "promptlab_prompt_cover_image_invalid";
    public const string PromptStatusInvalid = "promptlab_prompt_status_invalid";
    public const string PromptRejectionReasonRequired = "promptlab_prompt_rejection_reason_required";
    public const string PromptRejectionReasonInvalid = "promptlab_prompt_rejection_reason_invalid";

    public const string TemplateRequired = "promptlab_template_required";
    public const string TemplateTooLong = "promptlab_template_too_long";
    public const string TemplateSyntaxInvalid = "promptlab_template_syntax_invalid";
    public const string TemplatePlaceholderInvalid = "promptlab_template_placeholder_invalid";
    public const string TemplatePlaceholderDuplicate = "promptlab_template_placeholder_duplicate";
    public const string TemplateUnknownPlaceholder = "promptlab_template_unknown_placeholder";
    public const string TemplateUnusedVariable = "promptlab_template_unused_variable";
    public const string TemplateTooManyVariables = "promptlab_template_too_many_variables";

    public const string VariableNameRequired = "promptlab_variable_name_required";
    public const string VariableNameInvalid = "promptlab_variable_name_invalid";
    public const string VariableNameDuplicate = "promptlab_variable_name_duplicate";
    public const string VariableNameReserved = "promptlab_variable_name_reserved";
    public const string VariableTypeInvalid = "promptlab_variable_type_invalid";
    public const string VariableDefaultInvalid = "promptlab_variable_default_invalid";
    public const string VariableConstraintsInvalid = "promptlab_variable_constraints_invalid";
    public const string VariablePatternInvalid = "promptlab_variable_pattern_invalid";
    public const string VariableOptionsInvalid = "promptlab_variable_options_invalid";

    public const string RenderInputInvalid = "promptlab_render_input_invalid";
    public const string RenderUnknownVariable = "promptlab_render_unknown_variable";
    public const string RenderRequiredVariableMissing = "promptlab_render_required_variable_missing";
    public const string RenderValueInvalid = "promptlab_render_value_invalid";
    public const string RenderValueTooLong = "promptlab_render_value_too_long";
    public const string RenderPatternTimeout = "promptlab_render_pattern_timeout";
    public const string RenderOutputTooLong = "promptlab_render_output_too_long";
    public const string RenderRequiresAuthentication = "promptlab_render_requires_authentication";
    public const string RenderFailed = "promptlab_render_failed";

    public const string FavoriteRequiresAuthentication = "promptlab_favorite_requires_authentication";
    public const string FavoriteInvalid = "promptlab_favorite_invalid";

    public const string HistoryNotFound = "promptlab_history_not_found";
    public const string HistoryAccessDenied = "promptlab_history_access_denied";

    public const string PaginationInvalid = "promptlab_pagination_invalid";

    public const string PackNotFound = "promptlab_pack_not_found";
    public const string PackTitleRequired = "promptlab_pack_title_required";
    public const string PackTitleInvalid = "promptlab_pack_title_invalid";
    public const string PackSlugRequired = "promptlab_pack_slug_required";
    public const string PackSlugInvalid = "promptlab_pack_slug_invalid";
    public const string PackAuthorInvalid = "promptlab_pack_author_invalid";
    public const string PackCoverImageInvalid = "promptlab_pack_cover_image_invalid";
    public const string PackNotDraft = "promptlab_pack_not_draft";
    public const string PackEditForbidden = "promptlab_pack_edit_forbidden";
    public const string PackNotPublic = "promptlab_pack_not_public";
    public const string PackStatusInvalid = "promptlab_pack_status_invalid";
    public const string PackEmpty = "promptlab_pack_empty";
    public const string PackItemInvalid = "promptlab_pack_item_invalid";
    public const string PackItemDuplicate = "promptlab_pack_item_duplicate";
    public const string PackItemNotFound = "promptlab_pack_item_not_found";
    public const string PackItemOrderInvalid = "promptlab_pack_item_order_invalid";
    public const string PackItemPromptNotPublic = "promptlab_pack_item_prompt_not_public";
}

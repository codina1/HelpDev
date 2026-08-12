namespace HelpDev.Modules.Toolbox.Domain;

public static class ToolboxErrorCodes
{
    public const string CategoryNotFound = "toolbox_category_not_found";
    public const string CategoryNameRequired = "toolbox_category_name_required";
    public const string CategoryNameInvalid = "toolbox_category_name_invalid";
    public const string CategorySlugRequired = "toolbox_category_slug_required";
    public const string CategorySlugInvalid = "toolbox_category_slug_invalid";
    public const string CategorySlugDuplicate = "toolbox_category_slug_duplicate";
    public const string CategoryInactive = "toolbox_category_inactive";

    public const string ToolNotFound = "toolbox_tool_not_found";
    public const string ToolNameRequired = "toolbox_tool_name_required";
    public const string ToolNameInvalid = "toolbox_tool_name_invalid";
    public const string ToolSlugRequired = "toolbox_tool_slug_required";
    public const string ToolSlugInvalid = "toolbox_tool_slug_invalid";
    public const string ToolSlugDuplicate = "toolbox_tool_slug_duplicate";
    public const string ToolSummaryRequired = "toolbox_tool_summary_required";
    public const string ToolSummaryInvalid = "toolbox_tool_summary_invalid";
    public const string ToolTypeInvalid = "toolbox_tool_type_invalid";
    public const string ToolSchemaInvalid = "toolbox_tool_schema_invalid";
    public const string ToolDisabled = "toolbox_tool_disabled";
    public const string ToolUnpublished = "toolbox_tool_unpublished";
    public const string ToolCategoryInvalid = "toolbox_tool_category_invalid";
    public const string ToolCannotPublish = "toolbox_tool_cannot_publish";
    public const string ToolRequiresAuthentication = "toolbox_tool_requires_authentication";

    public const string ExecutionInputInvalid = "toolbox_execution_input_invalid";
    public const string ExecutionInputTooLarge = "toolbox_execution_input_too_large";
    public const string ExecutionOutputTooLarge = "toolbox_execution_output_too_large";
    public const string ExecutionTypeUnsupported = "toolbox_execution_type_unsupported";
    public const string ExecutionFailed = "toolbox_execution_failed";
    public const string JsonInvalid = "toolbox_json_invalid";
    public const string Base64Invalid = "toolbox_base64_invalid";
    public const string Utf8Invalid = "toolbox_utf8_invalid";
    public const string UuidCountInvalid = "toolbox_uuid_count_invalid";
    public const string HashAlgorithmInvalid = "toolbox_hash_algorithm_invalid";
    public const string TimestampInvalid = "toolbox_timestamp_invalid";
    public const string RegexPatternInvalid = "toolbox_regex_pattern_invalid";
    public const string RegexTimeout = "toolbox_regex_timeout";
    public const string RegexOptionsInvalid = "toolbox_regex_options_invalid";

    public const string FavoriteNotFound = "toolbox_favorite_not_found";
    public const string FavoriteInvalid = "toolbox_favorite_invalid";
    public const string FavoriteRequiresAuthentication = "toolbox_favorite_requires_authentication";

    public const string HistoryNotFound = "toolbox_history_not_found";
    public const string HistoryAccessDenied = "toolbox_history_access_denied";

    public const string PaginationInvalid = "toolbox_pagination_invalid";
}

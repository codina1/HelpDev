namespace HelpDev.Modules.Analytics.Domain;

public static class AnalyticsErrorCodes
{
    public const string EventIdRequired = "analytics_event_id_required";
    public const string EventTypeRequired = "analytics_event_type_required";
    public const string EventTypeUnsupported = "analytics_event_type_unsupported";
    public const string EventTimestampInvalid = "analytics_event_timestamp_invalid";
    public const string EventQuantityInvalid = "analytics_event_quantity_invalid";
    public const string EventDimensionsInvalid = "analytics_event_dimensions_invalid";
    public const string EventDimensionNotAllowed = "analytics_event_dimension_not_allowed";
    public const string EventSchemaVersionUnsupported = "analytics_event_schema_version_unsupported";

    public const string EventProcessingFailed = "analytics_event_processing_failed";
    public const string EventAlreadyProcessed = "analytics_event_already_processed";
    public const string MetricMappingNotFound = "analytics_metric_mapping_not_found";
    public const string ConcurrencyConflict = "analytics_concurrency_conflict";

    public const string DateRangeInvalid = "analytics_date_range_invalid";
    public const string DateRangeTooLarge = "analytics_date_range_too_large";
    public const string MetricKeyInvalid = "analytics_metric_key_invalid";
    public const string DimensionInvalid = "analytics_dimension_invalid";
    public const string LimitInvalid = "analytics_limit_invalid";
    public const string SubjectTypeInvalid = "analytics_subject_type_invalid";
}

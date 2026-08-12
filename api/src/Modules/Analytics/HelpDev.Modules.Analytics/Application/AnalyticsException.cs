namespace HelpDev.Modules.Analytics.Application;

public sealed class AnalyticsException : Exception
{
    public AnalyticsException(string message, string code)
        : base(message)
    {
        Code = code;
    }

    public string Code { get; }
}

public static class AnalyticsApplicationErrorCodes
{
    public const string EventIdRequired = Domain.AnalyticsErrorCodes.EventIdRequired;
    public const string EventTypeRequired = Domain.AnalyticsErrorCodes.EventTypeRequired;
    public const string EventTypeUnsupported = Domain.AnalyticsErrorCodes.EventTypeUnsupported;
    public const string EventTimestampInvalid = Domain.AnalyticsErrorCodes.EventTimestampInvalid;
    public const string EventQuantityInvalid = Domain.AnalyticsErrorCodes.EventQuantityInvalid;
    public const string EventDimensionsInvalid = Domain.AnalyticsErrorCodes.EventDimensionsInvalid;
    public const string EventDimensionNotAllowed = Domain.AnalyticsErrorCodes.EventDimensionNotAllowed;
    public const string EventSchemaVersionUnsupported = Domain.AnalyticsErrorCodes.EventSchemaVersionUnsupported;
    public const string EventProcessingFailed = Domain.AnalyticsErrorCodes.EventProcessingFailed;
    public const string MetricMappingNotFound = Domain.AnalyticsErrorCodes.MetricMappingNotFound;
    public const string ConcurrencyConflict = Domain.AnalyticsErrorCodes.ConcurrencyConflict;
    public const string DateRangeInvalid = Domain.AnalyticsErrorCodes.DateRangeInvalid;
    public const string DateRangeTooLarge = Domain.AnalyticsErrorCodes.DateRangeTooLarge;
    public const string MetricKeyInvalid = Domain.AnalyticsErrorCodes.MetricKeyInvalid;
    public const string DimensionInvalid = Domain.AnalyticsErrorCodes.DimensionInvalid;
    public const string LimitInvalid = Domain.AnalyticsErrorCodes.LimitInvalid;
    public const string SubjectTypeInvalid = Domain.AnalyticsErrorCodes.SubjectTypeInvalid;
}

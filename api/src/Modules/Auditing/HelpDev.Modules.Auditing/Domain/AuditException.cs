using HelpDev.SharedContracts.Auditing;
using HelpDev.SharedKernel.Exceptions;

namespace HelpDev.Modules.Auditing.Domain;

public sealed class AuditException : DomainException
{
    public AuditException(string message, string code)
        : base(message, code)
    {
    }
}

public static class AuditErrorCodes
{
    public const string RecordInvalid = "audit_record_invalid";
    public const string ActionUnsupported = "audit_action_unsupported";
    public const string CategoryInvalid = "audit_category_invalid";
    public const string OutcomeInvalid = "audit_outcome_invalid";
    public const string MetadataInvalid = "audit_metadata_invalid";
    public const string MetadataSensitive = "audit_metadata_sensitive";
    public const string RecordNotFound = "audit_record_not_found";
    public const string DateRangeInvalid = "audit_date_range_invalid";
    public const string DateRangeTooLarge = "audit_date_range_too_large";
    public const string PageInvalid = "audit_page_invalid";
}

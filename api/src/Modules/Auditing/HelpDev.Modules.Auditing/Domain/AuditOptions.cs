namespace HelpDev.Modules.Auditing.Domain;

public sealed class AuditOptions
{
    public const string SectionName = "Audit";

    public bool Enabled { get; set; } = true;

    public int RetentionDays { get; set; } = 365;

    public int MaxMetadataEntries { get; set; } = 10;

    public int MaxMetadataKeyLength { get; set; } = 50;

    public int MaxMetadataValueLength { get; set; } = 200;

    public int MaxReasonLength { get; set; } = 150;
}

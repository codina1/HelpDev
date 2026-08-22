namespace HelpDev.Modules.Media.Domain.Enums;

/// <summary>Lifecycle status for a media asset. V1 supports Active only (no soft-delete).</summary>
public enum MediaAssetStatus
{
    Active = 0,
    Archived = 1,
}

namespace HelpDev.Modules.Media.Application.Assets;

/// <summary>Authenticated actor for Media management. Writers own their uploads; Admins see all.</summary>
public sealed class MediaManagementActor
{
    public MediaManagementActor(Guid userId, bool canManageAllAssets)
    {
        if (userId == Guid.Empty)
        {
            throw new ArgumentException("User id is required.", nameof(userId));
        }

        UserId = userId;
        CanManageAllAssets = canManageAllAssets;
    }

    public Guid UserId { get; }

    public bool CanManageAllAssets { get; }
}

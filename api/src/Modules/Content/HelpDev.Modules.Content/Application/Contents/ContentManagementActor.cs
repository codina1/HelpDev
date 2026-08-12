namespace HelpDev.Modules.Content.Application.Contents;

/// <summary>
/// Framework-neutral management actor constructed by the API from authenticated claims.
/// </summary>
public sealed record ContentManagementActor
{
    public ContentManagementActor(Guid userId, bool canManageAllContent)
    {
        if (userId == Guid.Empty)
        {
            throw new ArgumentException("User id must not be empty.", nameof(userId));
        }

        UserId = userId;
        CanManageAllContent = canManageAllContent;
    }

    public Guid UserId { get; init; }

    public bool CanManageAllContent { get; init; }
}

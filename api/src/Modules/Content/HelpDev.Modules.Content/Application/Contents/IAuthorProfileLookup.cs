namespace HelpDev.Modules.Content.Application.Contents;

/// <summary>
/// Public author fields used on article listing/detail cards.
/// </summary>
public sealed record AuthorPublicProfile(
    string DisplayName,
    string? Role,
    string? Bio,
    string? AvatarUrl);

/// <summary>
/// Resolves author display data for published content without leaking private identity fields.
/// </summary>
public interface IAuthorProfileLookup
{
    Task<AuthorPublicProfile?> GetAsync(Guid authorId, CancellationToken cancellationToken = default);

    Task<IReadOnlyDictionary<Guid, AuthorPublicProfile>> GetManyAsync(
        IEnumerable<Guid> authorIds,
        CancellationToken cancellationToken = default);
}

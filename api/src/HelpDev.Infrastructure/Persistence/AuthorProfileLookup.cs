using HelpDev.Infrastructure.Persistence;
using HelpDev.Modules.Content.Application.Contents;
using Microsoft.EntityFrameworkCore;

namespace HelpDev.Infrastructure.Persistence;

/// <summary>
/// Reads public author profile fields from Identity users for Content DTOs.
/// </summary>
public sealed class AuthorProfileLookup : IAuthorProfileLookup
{
    private readonly ApplicationDbContext _db;

    public AuthorProfileLookup(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<AuthorPublicProfile?> GetAsync(
        Guid authorId,
        CancellationToken cancellationToken = default)
    {
        var map = await GetManyAsync([authorId], cancellationToken);
        return map.TryGetValue(authorId, out var profile) ? profile : null;
    }

    public async Task<IReadOnlyDictionary<Guid, AuthorPublicProfile>> GetManyAsync(
        IEnumerable<Guid> authorIds,
        CancellationToken cancellationToken = default)
    {
        var ids = authorIds.Where(id => id != Guid.Empty).Distinct().ToArray();
        if (ids.Length == 0)
        {
            return new Dictionary<Guid, AuthorPublicProfile>();
        }

        var rows = await _db.Users
            .AsNoTracking()
            .Where(user => ids.Contains(user.Id))
            .Select(user => new
            {
                user.Id,
                user.FirstName,
                user.LastName,
                user.FullName,
                user.Stack,
                user.Expertise,
                user.ProfileImageUrl,
            })
            .ToListAsync(cancellationToken);

        var result = new Dictionary<Guid, AuthorPublicProfile>(rows.Count);
        foreach (var row in rows)
        {
            var displayName = $"{row.FirstName} {row.LastName}".Trim();
            if (string.IsNullOrWhiteSpace(displayName))
            {
                displayName = row.FullName?.Trim() ?? string.Empty;
            }

            if (string.IsNullOrWhiteSpace(displayName))
            {
                displayName = "نویسنده HelpDev";
            }

            var role = string.IsNullOrWhiteSpace(row.Stack) ? null : row.Stack.Trim();
            var bio = string.IsNullOrWhiteSpace(row.Expertise) ? null : row.Expertise.Trim();
            var avatar = string.IsNullOrWhiteSpace(row.ProfileImageUrl) ? null : row.ProfileImageUrl.Trim();

            result[row.Id] = new AuthorPublicProfile(displayName, role, bio, avatar);
        }

        return result;
    }
}

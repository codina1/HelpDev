using HelpDev.Modules.Identity.Domain.Entities;

namespace HelpDev.Modules.Identity.Application.Profiles;

public static class UserProfileMapper
{
    public static string GetDisplayName(User user)
    {
        var displayName = $"{user.FirstName} {user.LastName}".Trim();
        return string.IsNullOrWhiteSpace(displayName) ? user.Mobile : displayName;
    }

    public static ProfileDto ToDto(User user) =>
        new(
            user.Id,
            user.Mobile,
            user.Role.ToString(),
            user.FirstName,
            user.LastName,
            GetDisplayName(user),
            user.Email,
            user.ProfileImageUrl,
            user.Expertise,
            user.Interests,
            GetCompletionPercent(user));

    public static void ApplyProfileUpdate(User user, UpdateProfileRequest request)
    {
        user.FirstName = request.FirstName.Trim();
        user.LastName = request.LastName.Trim();
        user.Email = request.Email.Trim();
        user.ProfileImageUrl = request.ProfileImageUrl.Trim();
        user.Expertise = request.Expertise.Trim();
        user.Interests = request.Interests.Trim();
        user.FullName = $"{user.FirstName} {user.LastName}".Trim();
    }

    private static int GetCompletionPercent(User user)
    {
        var fields = new[]
        {
            user.FirstName,
            user.LastName,
            user.Email,
            user.ProfileImageUrl,
            user.Expertise,
            user.Interests,
        };

        var completed = fields.Count(field => !string.IsNullOrWhiteSpace(field));
        return (int)Math.Round(completed * 100d / fields.Length);
    }
}

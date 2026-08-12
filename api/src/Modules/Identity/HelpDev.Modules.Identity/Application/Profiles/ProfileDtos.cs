namespace HelpDev.Modules.Identity.Application.Profiles;

public sealed record ProfileDto(
    Guid Id,
    string Mobile,
    string Role,
    string FirstName,
    string LastName,
    string DisplayName,
    string Email,
    string ProfileImageUrl,
    string Expertise,
    string Interests,
    int ProfileCompletionPercent);

public sealed class UpdateProfileRequest
{
    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string ProfileImageUrl { get; set; } = string.Empty;

    public string Expertise { get; set; } = string.Empty;

    public string Interests { get; set; } = string.Empty;
}

using HelpDev.Modules.Identity.Domain.Enums;

namespace HelpDev.Application.Admin;

public sealed record AdminUserListItemDto(
    Guid Id,
    string Mobile,
    string FirstName,
    string LastName,
    string DisplayName,
    string Email,
    string Role,
    DateTime CreatedAt,
    DateTime? LastLogin);

public sealed record AdminUserDetailDto(
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
    int ProfileCompletionPercent,
    DateTime CreatedAt,
    DateTime? LastLogin);

public sealed class UpdateAdminUserRequest
{
    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string ProfileImageUrl { get; set; } = string.Empty;

    public string Expertise { get; set; } = string.Empty;

    public string Interests { get; set; } = string.Empty;

    public string Role { get; set; } = UserRole.User.ToString();
}

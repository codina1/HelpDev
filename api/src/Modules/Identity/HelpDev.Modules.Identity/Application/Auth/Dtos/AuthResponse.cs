namespace HelpDev.Modules.Identity.Application.Auth.Dtos;

public sealed record AuthResponse(
    string AccessToken,
    int ExpiresIn,
    AuthUserDto User);

public sealed record AuthUserDto(
    Guid Id,
    string Mobile,
    string Role,
    string FirstName,
    string LastName,
    string DisplayName,
    string Email,
    string ProfileImageUrl,
    string Expertise,
    string Interests);

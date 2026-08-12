using HelpDev.Modules.Identity.Domain.Enums;

namespace HelpDev.Modules.Identity.Domain.Entities;

public class User
{
    public Guid Id { get; set; }

    public string Mobile { get; set; } = string.Empty;

    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public string FullName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string ProfileImageUrl { get; set; } = string.Empty;

    public string Expertise { get; set; } = string.Empty;

    public string Interests { get; set; } = string.Empty;

    public UserRole Role { get; set; } = UserRole.User;

    public string Stack { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public DateTime? LastLogin { get; set; }
}

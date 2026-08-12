using HelpDev.Modules.Identity.Application.Profiles;
using HelpDev.Modules.Identity.Domain.Entities;
using HelpDev.Modules.Identity.Domain.Enums;

namespace HelpDev.Identity.Tests;

public sealed class UserTests
{
    [Fact]
    public void New_user_defaults_role_to_User()
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            Mobile = "09123456789",
        };

        Assert.Equal(UserRole.User, user.Role);
        Assert.Equal(string.Empty, user.FirstName);
        Assert.Equal(string.Empty, user.LastName);
        Assert.Equal(string.Empty, user.FullName);
        Assert.Equal(string.Empty, user.Email);
    }

    [Fact]
    public void Mobile_is_preserved_exactly_as_assigned()
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            Mobile = "09123456789",
        };

        Assert.Equal("09123456789", user.Mobile);
    }

    [Fact]
    public void ApplyProfileUpdate_updates_supported_fields_and_full_name()
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            Mobile = "09123456789",
            Role = UserRole.User,
        };

        UserProfileMapper.ApplyProfileUpdate(user, new UpdateProfileRequest
        {
            FirstName = "  Sara ",
            LastName = " Ahmadi ",
            Email = " sara@example.com ",
            ProfileImageUrl = " https://cdn.example/a.png ",
            Expertise = " Backend ",
            Interests = " DDD ",
        });

        Assert.Equal("Sara", user.FirstName);
        Assert.Equal("Ahmadi", user.LastName);
        Assert.Equal("Sara Ahmadi", user.FullName);
        Assert.Equal("sara@example.com", user.Email);
        Assert.Equal("https://cdn.example/a.png", user.ProfileImageUrl);
        Assert.Equal("Backend", user.Expertise);
        Assert.Equal("DDD", user.Interests);
        Assert.Equal(UserRole.User, user.Role);
        Assert.Equal("09123456789", user.Mobile);
    }

    [Fact]
    public void ApplyProfileUpdate_with_same_values_is_effectively_noop_for_fields()
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            Mobile = "09123456789",
            FirstName = "Sara",
            LastName = "Ahmadi",
            FullName = "Sara Ahmadi",
            Email = "sara@example.com",
            ProfileImageUrl = "https://cdn.example/a.png",
            Expertise = "Backend",
            Interests = "DDD",
        };

        UserProfileMapper.ApplyProfileUpdate(user, new UpdateProfileRequest
        {
            FirstName = "Sara",
            LastName = "Ahmadi",
            Email = "sara@example.com",
            ProfileImageUrl = "https://cdn.example/a.png",
            Expertise = "Backend",
            Interests = "DDD",
        });

        Assert.Equal("Sara", user.FirstName);
        Assert.Equal("Ahmadi", user.LastName);
        Assert.Equal("Sara Ahmadi", user.FullName);
        Assert.Equal("sara@example.com", user.Email);
    }

    [Fact]
    public void ToDto_maps_display_name_and_completion_percent()
    {
        var user = new User
        {
            Id = Guid.Parse("11111111-1111-1111-1111-111111111101"),
            Mobile = "09123456789",
            FirstName = "Sara",
            LastName = "Ahmadi",
            Email = "sara@example.com",
            Role = UserRole.Writer,
        };

        var dto = UserProfileMapper.ToDto(user);

        Assert.Equal(user.Id, dto.Id);
        Assert.Equal("09123456789", dto.Mobile);
        Assert.Equal("Writer", dto.Role);
        Assert.Equal("Sara Ahmadi", dto.DisplayName);
        Assert.Equal(50, dto.ProfileCompletionPercent);
    }
}

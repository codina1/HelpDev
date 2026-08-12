using HelpDev.Identity.Tests.Fakes;
using HelpDev.Modules.Identity.Application.Profiles;
using HelpDev.Modules.Identity.Domain.Entities;
using HelpDev.Modules.Identity.Domain.Enums;

namespace HelpDev.Identity.Tests;

public sealed class ProfileServiceTests
{
    [Fact]
    public async Task GetMyProfile_returns_mapped_dto_for_existing_user()
    {
        var userId = Guid.NewGuid();
        var users = new FakeUserRepository();
        users.Seed(new User
        {
            Id = userId,
            Mobile = "09123456789",
            FirstName = "Ali",
            LastName = "Rezaei",
            Email = "ali@example.com",
            Role = UserRole.User,
        });
        var service = new ProfileService(users);

        var profile = await service.GetMyProfileAsync(userId);

        Assert.Equal(userId, profile.Id);
        Assert.Equal("09123456789", profile.Mobile);
        Assert.Equal("Ali Rezaei", profile.DisplayName);
        Assert.Equal("ali@example.com", profile.Email);
        Assert.Equal("User", profile.Role);
        Assert.Equal(50, profile.ProfileCompletionPercent);
    }

    [Fact]
    public async Task GetMyProfile_throws_when_user_is_missing()
    {
        var service = new ProfileService(new FakeUserRepository());

        var ex = await Assert.ThrowsAsync<ProfileException>(() =>
            service.GetMyProfileAsync(Guid.NewGuid()));

        Assert.Equal("کاربر یافت نشد.", ex.Message);
    }

    [Fact]
    public async Task UpdateMyProfile_persists_supported_fields()
    {
        var userId = Guid.NewGuid();
        var users = new FakeUserRepository();
        users.Seed(new User
        {
            Id = userId,
            Mobile = "09123456789",
            Role = UserRole.User,
        });
        var service = new ProfileService(users);

        var updated = await service.UpdateMyProfileAsync(userId, new UpdateProfileRequest
        {
            FirstName = "Neda",
            LastName = "Karimi",
            Email = "neda@example.com",
            ProfileImageUrl = "https://cdn.example/neda.png",
            Expertise = "Frontend",
            Interests = "React",
        });

        Assert.Equal(1, users.UpdateCount);
        Assert.Equal("Neda", updated.FirstName);
        Assert.Equal("Karimi", updated.LastName);
        Assert.Equal("Neda Karimi", updated.DisplayName);
        Assert.Equal("neda@example.com", updated.Email);
        Assert.Equal("Frontend", updated.Expertise);
        Assert.Equal("React", updated.Interests);
        Assert.Equal(100, updated.ProfileCompletionPercent);

        var stored = users.Users.Single();
        Assert.Equal("Neda Karimi", stored.FullName);
        Assert.Equal("09123456789", stored.Mobile);
    }

    [Fact]
    public async Task UpdateMyProfile_throws_for_invalid_first_name()
    {
        var userId = Guid.NewGuid();
        var users = new FakeUserRepository();
        users.Seed(new User { Id = userId, Mobile = "09123456789" });
        var service = new ProfileService(users);

        var ex = await Assert.ThrowsAsync<ProfileException>(() =>
            service.UpdateMyProfileAsync(userId, new UpdateProfileRequest
            {
                FirstName = " ",
                LastName = "Karimi",
            }));

        Assert.Equal("نام معتبر نیست.", ex.Message);
        Assert.Equal(0, users.UpdateCount);
    }

    [Fact]
    public async Task UpdateMyProfile_throws_when_user_is_missing()
    {
        var service = new ProfileService(new FakeUserRepository());

        var ex = await Assert.ThrowsAsync<ProfileException>(() =>
            service.UpdateMyProfileAsync(Guid.NewGuid(), new UpdateProfileRequest
            {
                FirstName = "Neda",
                LastName = "Karimi",
            }));

        Assert.Equal("کاربر یافت نشد.", ex.Message);
    }
}

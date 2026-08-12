namespace HelpDev.Modules.Identity.Application.Profiles;

public interface IProfileService
{
    Task<ProfileDto> GetMyProfileAsync(Guid userId, CancellationToken cancellationToken = default);

    Task<ProfileDto> UpdateMyProfileAsync(
        Guid userId,
        UpdateProfileRequest request,
        CancellationToken cancellationToken = default);
}

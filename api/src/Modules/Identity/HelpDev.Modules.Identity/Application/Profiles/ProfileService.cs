using System.Net.Mail;
using HelpDev.Modules.Identity.Application.Persistence;

namespace HelpDev.Modules.Identity.Application.Profiles;

public sealed class ProfileService : IProfileService
{
    private readonly IUserRepository _userRepository;

    public ProfileService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<ProfileDto> GetMyProfileAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken)
            ?? throw new ProfileException("کاربر یافت نشد.");

        return UserProfileMapper.ToDto(user);
    }

    public async Task<ProfileDto> UpdateMyProfileAsync(
        Guid userId,
        UpdateProfileRequest request,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.FirstName) || request.FirstName.Length > 100)
        {
            throw new ProfileException("نام معتبر نیست.");
        }

        if (string.IsNullOrWhiteSpace(request.LastName) || request.LastName.Length > 100)
        {
            throw new ProfileException("نام خانوادگی معتبر نیست.");
        }

        if (!string.IsNullOrWhiteSpace(request.Email))
        {
            if (request.Email.Length > 200)
            {
                throw new ProfileException("ایمیل معتبر نیست.");
            }

            try
            {
                _ = new MailAddress(request.Email);
            }
            catch
            {
                throw new ProfileException("فرمت ایمیل معتبر نیست.");
            }
        }

        if (request.ProfileImageUrl.Length > 500)
        {
            throw new ProfileException("آدرس تصویر پروفایل معتبر نیست.");
        }

        if (request.Expertise.Length > 200)
        {
            throw new ProfileException("تخصص نمی‌تواند بیش از ۲۰۰ کاراکتر باشد.");
        }

        if (request.Interests.Length > 500)
        {
            throw new ProfileException("علاقه‌مندی‌ها نمی‌تواند بیش از ۵۰۰ کاراکتر باشد.");
        }

        var user = await _userRepository.GetByIdAsync(userId, cancellationToken)
            ?? throw new ProfileException("کاربر یافت نشد.");

        UserProfileMapper.ApplyProfileUpdate(user, request);
        await _userRepository.UpdateAsync(user, cancellationToken);

        return UserProfileMapper.ToDto(user);
    }
}

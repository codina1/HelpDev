using System.Net.Mail;
using HelpDev.Modules.Identity.Application.Persistence;
using HelpDev.Modules.Identity.Application.Profiles;
using HelpDev.Modules.Identity.Domain.Enums;

namespace HelpDev.Application.Admin;

public sealed class AdminUserService : IAdminUserService
{
    private readonly IUserRepository _userRepository;

    public AdminUserService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<IReadOnlyList<AdminUserListItemDto>> ListUsersAsync(
        CancellationToken cancellationToken = default)
    {
        var users = await _userRepository.ListAllAsync(cancellationToken);

        return users
            .Select(user => new AdminUserListItemDto(
                user.Id,
                user.Mobile,
                user.FirstName,
                user.LastName,
                UserProfileMapper.GetDisplayName(user),
                user.Email,
                user.Role.ToString(),
                user.CreatedAt,
                user.LastLogin))
            .ToList();
    }

    public async Task<AdminUserDetailDto> GetUserAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken)
            ?? throw new ProfileException("کاربر یافت نشد.");

        return ToDetail(user);
    }

    public async Task<AdminUserDetailDto> UpdateUserAsync(
        Guid actorUserId,
        Guid targetUserId,
        UpdateAdminUserRequest request,
        CancellationToken cancellationToken = default)
    {
        ValidateRequest(request);

        if (!Enum.TryParse<UserRole>(request.Role, ignoreCase: true, out var role))
        {
            throw new ProfileException("نقش کاربر معتبر نیست.");
        }

        var user = await _userRepository.GetByIdAsync(targetUserId, cancellationToken)
            ?? throw new ProfileException("کاربر یافت نشد.");

        if (actorUserId == targetUserId && role != UserRole.Admin)
        {
            throw new ProfileException("نمی‌توانید نقش ادمین خودتان را حذف کنید.");
        }

        var previousFullName = user.FullName;

        UserProfileMapper.ApplyProfileUpdate(user, new UpdateProfileRequest
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            ProfileImageUrl = request.ProfileImageUrl,
            Expertise = request.Expertise,
            Interests = request.Interests,
        });

        if (string.IsNullOrWhiteSpace(user.FullName) && !string.IsNullOrWhiteSpace(previousFullName))
        {
            user.FullName = previousFullName;
        }

        user.Role = role;
        await _userRepository.UpdateAsync(user, cancellationToken);

        return ToDetail(user);
    }

    private static void ValidateRequest(UpdateAdminUserRequest request)
    {
        if (request.FirstName.Length > 100)
        {
            throw new ProfileException("نام معتبر نیست.");
        }

        if (request.LastName.Length > 100)
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
    }

    private static AdminUserDetailDto ToDetail(HelpDev.Modules.Identity.Domain.Entities.User user)
    {
        var profile = UserProfileMapper.ToDto(user);

        return new AdminUserDetailDto(
            profile.Id,
            profile.Mobile,
            profile.Role,
            profile.FirstName,
            profile.LastName,
            profile.DisplayName,
            profile.Email,
            profile.ProfileImageUrl,
            profile.Expertise,
            profile.Interests,
            profile.ProfileCompletionPercent,
            user.CreatedAt,
            user.LastLogin);
    }
}

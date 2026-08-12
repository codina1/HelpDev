namespace HelpDev.Application.Admin;

public interface IAdminUserService
{
    Task<IReadOnlyList<AdminUserListItemDto>> ListUsersAsync(CancellationToken cancellationToken = default);

    Task<AdminUserDetailDto> GetUserAsync(Guid userId, CancellationToken cancellationToken = default);

    Task<AdminUserDetailDto> UpdateUserAsync(
        Guid actorUserId,
        Guid targetUserId,
        UpdateAdminUserRequest request,
        CancellationToken cancellationToken = default);
}

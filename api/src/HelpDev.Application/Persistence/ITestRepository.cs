using HelpDev.Application.Test.Dtos;

namespace HelpDev.Application.Persistence;

public interface ITestRepository
{
    Task<IReadOnlyList<TestUserDto>> ListUsersAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<TestContentTypeCountDto>> GetPublishedContentCountsByTypeAsync(
        CancellationToken cancellationToken = default);
}

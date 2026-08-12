using HelpDev.Application.Persistence;
using HelpDev.Application.Test.Dtos;

namespace HelpDev.Application.Test;

public sealed class TestService : ITestService
{
    private readonly ITestRepository _testRepository;

    public TestService(ITestRepository testRepository)
    {
        _testRepository = testRepository;
    }

    public async Task<TestContentResponse> GetContentSummaryAsync(
        TestAuthInfoDto authentication,
        bool databaseConnected,
        CancellationToken cancellationToken = default)
    {
        if (!databaseConnected)
        {
            return new TestContentResponse(
                Status: "Degraded",
                Database: new TestDatabaseInfoDto(false),
                Authentication: authentication,
                TotalPublished: 0,
                ByType: []);
        }

        var byType = await _testRepository.GetPublishedContentCountsByTypeAsync(cancellationToken);
        var total = byType.Sum(item => item.Count);

        return new TestContentResponse(
            Status: "Healthy",
            Database: new TestDatabaseInfoDto(true),
            Authentication: authentication,
            TotalPublished: total,
            ByType: byType);
    }

    public async Task<TestUsersResponse> GetUsersSummaryAsync(
        TestAuthInfoDto authentication,
        bool databaseConnected,
        CancellationToken cancellationToken = default)
    {
        if (!databaseConnected)
        {
            return new TestUsersResponse(
                Status: "Degraded",
                Database: new TestDatabaseInfoDto(false),
                Authentication: authentication,
                Total: 0,
                Users: []);
        }

        var users = await _testRepository.ListUsersAsync(cancellationToken);

        return new TestUsersResponse(
            Status: "Healthy",
            Database: new TestDatabaseInfoDto(true),
            Authentication: authentication,
            Total: users.Count,
            Users: users);
    }
}

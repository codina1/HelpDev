using HelpDev.Application.Test.Dtos;

namespace HelpDev.Application.Test;

public interface ITestService
{
    Task<TestContentResponse> GetContentSummaryAsync(
        TestAuthInfoDto authentication,
        bool databaseConnected,
        CancellationToken cancellationToken = default);

    Task<TestUsersResponse> GetUsersSummaryAsync(
        TestAuthInfoDto authentication,
        bool databaseConnected,
        CancellationToken cancellationToken = default);
}

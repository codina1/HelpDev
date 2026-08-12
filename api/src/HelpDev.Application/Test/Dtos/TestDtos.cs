namespace HelpDev.Application.Test.Dtos;

public sealed record TestUserDto(
    Guid Id,
    string Mobile,
    string FullName,
    string Role);

public sealed record TestContentTypeCountDto(string Type, int Count);

public sealed record TestAuthInfoDto(
    Guid UserId,
    string Role,
    string Mobile);

public sealed record TestDatabaseInfoDto(bool Connected);

public sealed record TestContentResponse(
    string Status,
    TestDatabaseInfoDto Database,
    TestAuthInfoDto Authentication,
    int TotalPublished,
    IReadOnlyList<TestContentTypeCountDto> ByType);

public sealed record TestUsersResponse(
    string Status,
    TestDatabaseInfoDto Database,
    TestAuthInfoDto Authentication,
    int Total,
    IReadOnlyList<TestUserDto> Users);

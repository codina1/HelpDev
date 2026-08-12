namespace HelpDev.Modules.Administration.Application.FeatureFlags;

public sealed record FeatureFlagDto(
    Guid Id,
    string Key,
    bool IsEnabled,
    string? Description,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc);

public sealed record CreateFeatureFlagRequest(
    string Key,
    bool IsEnabled,
    string? Description);

public sealed record UpdateFeatureFlagRequest(
    string? Description);

public sealed record SetFeatureFlagStateRequest(bool IsEnabled);

public interface IFeatureFlagQueries
{
    Task<IReadOnlyList<FeatureFlagDto>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<FeatureFlagDto?> GetByKeyAsync(string key, CancellationToken cancellationToken = default);
}

public interface IFeatureFlagService
{
    Task<IReadOnlyList<FeatureFlagDto>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<FeatureFlagDto> GetByKeyAsync(string key, CancellationToken cancellationToken = default);

    Task<FeatureFlagDto> CreateAsync(
        CreateFeatureFlagRequest request,
        Guid? administratorId = null,
        CancellationToken cancellationToken = default);

    Task<FeatureFlagDto> UpdateAsync(
        string key,
        UpdateFeatureFlagRequest request,
        Guid? administratorId = null,
        CancellationToken cancellationToken = default);

    Task<FeatureFlagDto> SetEnabledAsync(
        string key,
        bool isEnabled,
        Guid? administratorId = null,
        CancellationToken cancellationToken = default);
}

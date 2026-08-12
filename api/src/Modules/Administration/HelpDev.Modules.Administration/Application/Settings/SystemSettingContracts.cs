using HelpDev.Modules.Administration.Domain.Settings;

namespace HelpDev.Modules.Administration.Application.Settings;

public sealed record SystemSettingDto(
    Guid Id,
    string Key,
    string Value,
    string ValueType,
    string? Description,
    bool IsPublic,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc);

public sealed record PublicSystemSettingDto(
    string Key,
    string Value,
    string ValueType);

public sealed record CreateSystemSettingRequest(
    string Key,
    string Value,
    string ValueType,
    string? Description,
    bool IsPublic);

public sealed record UpdateSystemSettingRequest(
    string Value,
    string? Description,
    bool? IsPublic);

public interface ISystemSettingQueries
{
    Task<IReadOnlyList<SystemSettingDto>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<SystemSettingDto?> GetByKeyAsync(string key, CancellationToken cancellationToken = default);
}

public interface IPublicSystemSettingQueries
{
    Task<IReadOnlyList<PublicSystemSettingDto>> GetPublicAsync(CancellationToken cancellationToken = default);
}

public interface ISystemSettingService
{
    Task<IReadOnlyList<SystemSettingDto>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<SystemSettingDto> GetByKeyAsync(string key, CancellationToken cancellationToken = default);

    Task<SystemSettingDto> CreateAsync(
        CreateSystemSettingRequest request,
        Guid? administratorId = null,
        CancellationToken cancellationToken = default);

    Task<SystemSettingDto> UpdateAsync(
        string key,
        UpdateSystemSettingRequest request,
        Guid? administratorId = null,
        CancellationToken cancellationToken = default);
}

public static class SystemSettingValueTypeParser
{
    public static SystemSettingValueType Parse(string valueType)
    {
        if (string.IsNullOrWhiteSpace(valueType))
        {
            throw new AdministrationException(
                "System setting value type is required.",
                AdministrationApplicationErrorCodes.SettingValueInvalid);
        }

        if (Enum.TryParse<SystemSettingValueType>(valueType.Trim(), ignoreCase: true, out var parsed)
            && Enum.IsDefined(parsed))
        {
            return parsed;
        }

        throw new AdministrationException(
            "System setting value type is invalid.",
            AdministrationApplicationErrorCodes.SettingValueInvalid);
    }
}

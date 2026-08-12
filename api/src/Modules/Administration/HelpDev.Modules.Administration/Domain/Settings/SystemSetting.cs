using System.Globalization;
using System.Text.Json;
using HelpDev.SharedKernel.Common;
using HelpDev.SharedKernel.Exceptions;

namespace HelpDev.Modules.Administration.Domain.Settings;

public sealed class SystemSetting : AggregateRoot<Guid>
{
    public const int KeyMaxLength = 100;
    public const int ValueMaxLength = 4000;
    public const int DescriptionMaxLength = 500;

    private static readonly string[] SensitiveKeyFragments =
    [
        "ConnectionString",
        "JwtSecret",
        "Password",
        "ApiKey",
        "PrivateKey",
        "EncryptionKey",
        "Secret",
        "Credential",
    ];

    /// <summary>Required for EF Core materialization.</summary>
    private SystemSetting()
    {
    }

    private SystemSetting(Guid id)
        : base(id)
    {
    }

    public string Key { get; private set; } = string.Empty;

    public string Value { get; private set; } = string.Empty;

    public string? Description { get; private set; }

    public SystemSettingValueType ValueType { get; private set; }

    public bool IsPublic { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    public DateTime UpdatedAtUtc { get; private set; }

    public static SystemSetting Create(
        Guid id,
        string key,
        string value,
        SystemSettingValueType valueType,
        string? description,
        bool isPublic,
        DateTime utcNow)
    {
        if (id == Guid.Empty)
        {
            throw new DomainException("System setting id must not be empty.", AdministrationErrorCodes.SettingKeyInvalid);
        }

        var normalizedKey = NormalizeKey(key);
        EnsureNotSensitive(normalizedKey);
        var normalizedValue = NormalizeAndValidateValue(value, valueType);
        var normalizedDescription = NormalizeDescription(description);

        return new SystemSetting(id)
        {
            Key = normalizedKey,
            Value = normalizedValue,
            ValueType = valueType,
            Description = normalizedDescription,
            IsPublic = isPublic,
            CreatedAtUtc = utcNow,
            UpdatedAtUtc = utcNow,
        };
    }

    public bool UpdateValue(string value, DateTime utcNow)
    {
        var normalized = NormalizeAndValidateValue(value, ValueType);
        if (string.Equals(Value, normalized, StringComparison.Ordinal))
        {
            return false;
        }

        Value = normalized;
        UpdatedAtUtc = utcNow;
        return true;
    }

    public bool UpdateDescription(string? description, DateTime utcNow)
    {
        var normalized = NormalizeDescription(description);
        if (string.Equals(Description, normalized, StringComparison.Ordinal))
        {
            return false;
        }

        Description = normalized;
        UpdatedAtUtc = utcNow;
        return true;
    }

    public bool ChangeVisibility(bool isPublic, DateTime utcNow)
    {
        if (IsPublic == isPublic)
        {
            return false;
        }

        IsPublic = isPublic;
        UpdatedAtUtc = utcNow;
        return true;
    }

    public static string NormalizeKey(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            throw new DomainException("System setting key is required.", AdministrationErrorCodes.SettingKeyRequired);
        }

        var trimmed = key.Trim();
        if (trimmed.Length > KeyMaxLength)
        {
            throw new DomainException(
                $"System setting key must be at most {KeyMaxLength} characters.",
                AdministrationErrorCodes.SettingKeyInvalid);
        }

        if (trimmed.Any(ch => !(char.IsLetterOrDigit(ch) || ch is '_' or '-' or '.')))
        {
            throw new DomainException(
                "System setting key may contain only letters, digits, underscore, hyphen, or period.",
                AdministrationErrorCodes.SettingKeyInvalid);
        }

        EnsureNotSensitive(trimmed);
        return trimmed;
    }

    public static void EnsureNotSensitive(string key)
    {
        foreach (var fragment in SensitiveKeyFragments)
        {
            if (key.Contains(fragment, StringComparison.OrdinalIgnoreCase))
            {
                throw new DomainException(
                    "System setting key must not store secrets or infrastructure credentials.",
                    AdministrationErrorCodes.SettingSensitiveKeyForbidden);
            }
        }
    }

    private static string NormalizeAndValidateValue(string value, SystemSettingValueType valueType)
    {
        if (value is null)
        {
            throw new DomainException("System setting value is required.", AdministrationErrorCodes.SettingValueInvalid);
        }

        var trimmed = value.Trim();
        if (trimmed.Length == 0)
        {
            throw new DomainException("System setting value is required.", AdministrationErrorCodes.SettingValueInvalid);
        }

        if (trimmed.Length > ValueMaxLength)
        {
            throw new DomainException(
                $"System setting value must be at most {ValueMaxLength} characters.",
                AdministrationErrorCodes.SettingValueTooLong);
        }

        return valueType switch
        {
            SystemSettingValueType.String => trimmed,
            SystemSettingValueType.Boolean => NormalizeBoolean(trimmed),
            SystemSettingValueType.Integer => NormalizeInteger(trimmed),
            SystemSettingValueType.Decimal => NormalizeDecimal(trimmed),
            SystemSettingValueType.Json => NormalizeJson(trimmed),
            _ => throw new DomainException(
                "System setting value type is invalid.",
                AdministrationErrorCodes.SettingValueInvalid),
        };
    }

    private static string NormalizeBoolean(string value)
    {
        if (bool.TryParse(value, out var parsed))
        {
            return parsed ? "true" : "false";
        }

        if (string.Equals(value, "1", StringComparison.Ordinal)
            || string.Equals(value, "yes", StringComparison.OrdinalIgnoreCase)
            || string.Equals(value, "on", StringComparison.OrdinalIgnoreCase))
        {
            return "true";
        }

        if (string.Equals(value, "0", StringComparison.Ordinal)
            || string.Equals(value, "no", StringComparison.OrdinalIgnoreCase)
            || string.Equals(value, "off", StringComparison.OrdinalIgnoreCase))
        {
            return "false";
        }

        throw new DomainException(
            "Boolean setting value must be true/false (or 1/0, yes/no, on/off).",
            AdministrationErrorCodes.SettingValueInvalid);
    }

    private static string NormalizeInteger(string value)
    {
        if (!int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed))
        {
            throw new DomainException(
                "Integer setting value must be a valid invariant-culture integer.",
                AdministrationErrorCodes.SettingValueInvalid);
        }

        return parsed.ToString(CultureInfo.InvariantCulture);
    }

    private static string NormalizeDecimal(string value)
    {
        if (!decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out var parsed))
        {
            throw new DomainException(
                "Decimal setting value must be a valid invariant-culture decimal.",
                AdministrationErrorCodes.SettingValueInvalid);
        }

        return parsed.ToString(CultureInfo.InvariantCulture);
    }

    private static string NormalizeJson(string value)
    {
        try
        {
            using var document = JsonDocument.Parse(value);
            return document.RootElement.GetRawText();
        }
        catch (JsonException)
        {
            throw new DomainException(
                "Json setting value must be valid JSON.",
                AdministrationErrorCodes.SettingValueInvalid);
        }
    }

    private static string? NormalizeDescription(string? description)
    {
        if (description is null)
        {
            return null;
        }

        var trimmed = description.Trim();
        if (trimmed.Length == 0)
        {
            return null;
        }

        if (trimmed.Length > DescriptionMaxLength)
        {
            throw new DomainException(
                $"System setting description must be at most {DescriptionMaxLength} characters.",
                AdministrationErrorCodes.SettingKeyInvalid);
        }

        return trimmed;
    }
}

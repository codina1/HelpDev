using HelpDev.Modules.Administration.Domain;
using HelpDev.Modules.Administration.Domain.Settings;
using HelpDev.SharedKernel.Exceptions;

namespace HelpDev.Administration.Tests;

public sealed class SystemSettingTests
{
    private static readonly DateTime Now = new(2026, 7, 19, 12, 0, 0, DateTimeKind.Utc);

    [Theory]
    [InlineData(SystemSettingValueType.String, "HelpDev", "HelpDev")]
    [InlineData(SystemSettingValueType.Integer, "42", "42")]
    [InlineData(SystemSettingValueType.Boolean, "true", "true")]
    [InlineData(SystemSettingValueType.Boolean, "YES", "true")]
    [InlineData(SystemSettingValueType.Boolean, "0", "false")]
    [InlineData(SystemSettingValueType.Decimal, "3.14", "3.14")]
    [InlineData(SystemSettingValueType.Json, "{\"a\":1}", "{\"a\":1}")]
    public void Create_accepts_supported_value_types(
        SystemSettingValueType valueType,
        string value,
        string expected)
    {
        var setting = SystemSetting.Create(
            Guid.NewGuid(),
            "SiteName",
            value,
            valueType,
            null,
            isPublic: true,
            Now);

        Assert.Equal(expected, setting.Value);
        Assert.Equal(valueType, setting.ValueType);
    }

    [Theory]
    [InlineData(SystemSettingValueType.Boolean, "maybe")]
    [InlineData(SystemSettingValueType.Integer, "1.5")]
    [InlineData(SystemSettingValueType.Decimal, "abc")]
    [InlineData(SystemSettingValueType.Json, "{broken")]
    public void Create_rejects_invalid_typed_values(SystemSettingValueType valueType, string value)
    {
        var ex = Assert.Throws<DomainException>(() =>
            SystemSetting.Create(Guid.NewGuid(), "DefaultPageSize", value, valueType, null, false, Now));

        Assert.Equal(AdministrationErrorCodes.SettingValueInvalid, ex.Code);
    }

    [Theory]
    [InlineData("JwtSecret")]
    [InlineData("DbConnectionString")]
    [InlineData("ApiKeyPrimary")]
    [InlineData("PasswordHash")]
    public void Create_rejects_sensitive_keys(string key)
    {
        var ex = Assert.Throws<DomainException>(() =>
            SystemSetting.Create(Guid.NewGuid(), key, "value", SystemSettingValueType.String, null, false, Now));

        Assert.Equal(AdministrationErrorCodes.SettingSensitiveKeyForbidden, ex.Code);
    }

    [Fact]
    public void Create_rejects_oversized_value()
    {
        var ex = Assert.Throws<DomainException>(() =>
            SystemSetting.Create(
                Guid.NewGuid(),
                "SiteDescription",
                new string('x', SystemSetting.ValueMaxLength + 1),
                SystemSettingValueType.String,
                null,
                false,
                Now));

        Assert.Equal(AdministrationErrorCodes.SettingValueTooLong, ex.Code);
    }

    [Fact]
    public void UpdateValue_noop_does_not_change_timestamp()
    {
        var setting = SystemSetting.Create(
            Guid.NewGuid(),
            "DefaultLanguage",
            "en",
            SystemSettingValueType.String,
            null,
            true,
            Now);

        Assert.False(setting.UpdateValue("en", Now.AddMinutes(1)));
        Assert.Equal(Now, setting.UpdatedAtUtc);

        Assert.True(setting.UpdateValue("fa", Now.AddMinutes(1)));
        Assert.Equal("fa", setting.Value);
        Assert.Equal(Now.AddMinutes(1), setting.UpdatedAtUtc);
    }
}

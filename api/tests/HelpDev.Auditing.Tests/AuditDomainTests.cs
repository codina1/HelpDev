using HelpDev.Modules.Auditing.Application.Recording;
using HelpDev.Modules.Auditing.Domain;
using HelpDev.Modules.Auditing.Domain.Records;
using HelpDev.SharedContracts.Auditing;
using Microsoft.Extensions.Options;

namespace HelpDev.Auditing.Tests;

public sealed class AuditRecordTests
{
    private static readonly DateTime Now = new(2026, 7, 20, 10, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Create_with_valid_input_produces_record()
    {
        var id = Guid.NewGuid();

        var record = AuditRecord.Create(
            id,
            Now,
            AuditCategories.Administration,
            AuditActions.AdministrationFeatureFlagCreated,
            AuditOutcomes.Success,
            Guid.NewGuid(),
            AuditActorTypes.User,
            id,
            "FeatureFlag",
            "feature.test",
            null,
            "corr-1",
            "POST",
            "/api/admin/feature-flags",
            new Dictionary<string, string> { ["key"] = "feature.test" },
            Now,
            maxReasonLength: 200,
            maxSubjectDisplayLength: 200,
            maxPathTemplateLength: 300,
            maxCorrelationIdLength: 100);

        Assert.Equal(id, record.Id);
        Assert.Equal(AuditActions.AdministrationFeatureFlagCreated, record.Action);
        Assert.Equal("feature.test", record.SubjectDisplay);
    }

    [Fact]
    public void Create_rejects_unsupported_action()
    {
        var ex = Assert.Throws<AuditException>(() =>
            AuditRecord.Create(
                Guid.NewGuid(),
                Now,
                AuditCategories.Administration,
                "administration.unsupported",
                AuditOutcomes.Success,
                null,
                AuditActorTypes.System,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                Now,
                200,
                200,
                300,
                100));

        Assert.Equal(AuditErrorCodes.ActionUnsupported, ex.Code);
    }
}

public sealed class AuditMetadataSanitizerTests
{
    private readonly AuditMetadataSanitizer _sanitizer = new(Options.Create(new AuditOptions()));

    [Fact]
    public void Sanitize_allows_feature_flag_metadata_keys()
    {
        var result = _sanitizer.Sanitize(
            AuditActions.AdministrationFeatureFlagCreated,
            new Dictionary<string, string>
            {
                ["key"] = "feature.test",
                ["previousState"] = "none",
                ["newState"] = "enabled",
            });

        Assert.NotNull(result);
        Assert.Equal("enabled", result!["newState"]);
    }

    [Fact]
    public void Sanitize_rejects_setting_value_key()
    {
        var ex = Assert.Throws<AuditException>(() =>
            _sanitizer.Sanitize(
                AuditActions.AdministrationSettingUpdated,
                new Dictionary<string, string> { ["value"] = "secret" }));

        Assert.Equal(AuditErrorCodes.MetadataInvalid, ex.Code);
    }

    [Fact]
    public void Sanitize_rejects_disallowed_key_for_action()
    {
        var ex = Assert.Throws<AuditException>(() =>
            _sanitizer.Sanitize(
                AuditActions.AuthenticationOtpRequested,
                new Dictionary<string, string> { ["phone"] = "09123456789" }));

        Assert.Equal(AuditErrorCodes.MetadataInvalid, ex.Code);
    }

    [Fact]
    public void Sanitize_allows_setting_metadata_without_value()
    {
        var result = _sanitizer.Sanitize(
            AuditActions.AdministrationSettingUpdated,
            new Dictionary<string, string>
            {
                ["key"] = "site.name",
                ["isPublic"] = "true",
                ["valueChanged"] = "true",
            });

        Assert.NotNull(result);
        Assert.False(result!.ContainsKey("value"));
    }
}

using HelpDev.Modules.Administration.Domain;
using HelpDev.Modules.Administration.Domain.FeatureFlags;
using HelpDev.SharedKernel.Exceptions;

namespace HelpDev.Administration.Tests;

public sealed class FeatureFlagTests
{
    private static readonly DateTime Now = new(2026, 7, 19, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Create_valid_flag_trims_key_and_description()
    {
        var flag = FeatureFlag.Create(Guid.NewGuid(), "  SearchEnabled  ", true, "  desc  ", Now);

        Assert.Equal("SearchEnabled", flag.Key);
        Assert.True(flag.IsEnabled);
        Assert.Equal("desc", flag.Description);
        Assert.Equal(Now, flag.CreatedAtUtc);
        Assert.Equal(Now, flag.UpdatedAtUtc);
    }

    [Fact]
    public void Create_rejects_empty_key()
    {
        var ex = Assert.Throws<DomainException>(() =>
            FeatureFlag.Create(Guid.NewGuid(), "  ", false, null, Now));

        Assert.Equal(AdministrationErrorCodes.FeatureKeyRequired, ex.Code);
    }

    [Fact]
    public void Create_rejects_oversized_key()
    {
        var ex = Assert.Throws<DomainException>(() =>
            FeatureFlag.Create(Guid.NewGuid(), new string('a', FeatureFlag.KeyMaxLength + 1), false, null, Now));

        Assert.Equal(AdministrationErrorCodes.FeatureKeyInvalid, ex.Code);
    }

    [Fact]
    public void Create_rejects_oversized_description()
    {
        var ex = Assert.Throws<DomainException>(() =>
            FeatureFlag.Create(
                Guid.NewGuid(),
                "ValidKey",
                false,
                new string('d', FeatureFlag.DescriptionMaxLength + 1),
                Now));

        Assert.Equal(AdministrationErrorCodes.FeatureDescriptionInvalid, ex.Code);
    }

    [Fact]
    public void Enable_and_disable_update_timestamp_only_on_mutation()
    {
        var flag = FeatureFlag.Create(Guid.NewGuid(), "LearningEnabled", false, null, Now);
        var later = Now.AddMinutes(5);

        Assert.False(flag.Disable(later));
        Assert.Equal(Now, flag.UpdatedAtUtc);

        Assert.True(flag.Enable(later));
        Assert.True(flag.IsEnabled);
        Assert.Equal(later, flag.UpdatedAtUtc);

        Assert.False(flag.Enable(later.AddMinutes(1)));
        Assert.Equal(later, flag.UpdatedAtUtc);

        var disabledAt = later.AddMinutes(2);
        Assert.True(flag.Disable(disabledAt));
        Assert.False(flag.IsEnabled);
        Assert.Equal(disabledAt, flag.UpdatedAtUtc);
    }

    [Fact]
    public void UpdateDescription_noop_does_not_change_timestamp()
    {
        var flag = FeatureFlag.Create(Guid.NewGuid(), "PromptLabEnabled", true, "same", Now);

        Assert.False(flag.UpdateDescription("same", Now.AddHours(1)));
        Assert.Equal(Now, flag.UpdatedAtUtc);

        Assert.True(flag.UpdateDescription("changed", Now.AddHours(1)));
        Assert.Equal("changed", flag.Description);
        Assert.Equal(Now.AddHours(1), flag.UpdatedAtUtc);
    }
}

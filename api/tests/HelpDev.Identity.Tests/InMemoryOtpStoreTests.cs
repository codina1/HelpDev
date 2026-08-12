using HelpDev.Identity.Tests.Fakes;
using HelpDev.Modules.Identity.Application.Auth;
using HelpDev.Modules.Identity.Infrastructure.Auth;
using Microsoft.Extensions.Options;

namespace HelpDev.Identity.Tests;

public sealed class InMemoryOtpStoreTests
{
    private const string Mobile = "09123456789";
    private const string Code = "123456";

    [Fact]
    public async Task Store_and_validate_succeeds_for_correct_code()
    {
        var store = CreateStore();

        await store.StoreAsync(Mobile, Code, TimeSpan.FromMinutes(5));

        Assert.True(await store.ValidateAndRemoveAsync(Mobile, Code));
    }

    [Fact]
    public async Task Validate_fails_for_incorrect_code_and_keeps_entry()
    {
        var store = CreateStore();
        await store.StoreAsync(Mobile, Code, TimeSpan.FromMinutes(5));

        var firstAttempt = await store.ValidateAndRemoveAsync(Mobile, "000000");
        var secondAttempt = await store.ValidateAndRemoveAsync(Mobile, Code);

        Assert.False(firstAttempt);
        Assert.True(secondAttempt);
    }

    [Fact]
    public async Task Successful_validation_removes_otp_so_replay_fails()
    {
        var store = CreateStore();
        await store.StoreAsync(Mobile, Code, TimeSpan.FromMinutes(5));

        var first = await store.ValidateAndRemoveAsync(Mobile, Code);
        var replay = await store.ValidateAndRemoveAsync(Mobile, Code);

        Assert.True(first);
        Assert.False(replay);
    }

    [Fact]
    public async Task Otp_is_valid_before_expiration()
    {
        var clock = new FakeDateTimeProvider(new DateTime(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc));
        var store = CreateStore(clock);
        await store.StoreAsync(Mobile, Code, TimeSpan.FromMinutes(5));

        clock.Advance(TimeSpan.FromMinutes(4));

        Assert.True(await store.ValidateAndRemoveAsync(Mobile, Code));
    }

    [Fact]
    public async Task Advancing_fake_time_expires_otp()
    {
        var clock = new FakeDateTimeProvider(new DateTime(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc));
        var store = CreateStore(clock);
        await store.StoreAsync(Mobile, Code, TimeSpan.FromMinutes(5));

        clock.Advance(TimeSpan.FromMinutes(5));

        Assert.False(await store.ValidateAndRemoveAsync(Mobile, Code));
    }

    [Fact]
    public async Task Expired_otp_is_removed_and_cannot_be_used_later()
    {
        var clock = new FakeDateTimeProvider(new DateTime(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc));
        var store = CreateStore(clock);
        await store.StoreAsync(Mobile, Code, TimeSpan.FromMinutes(5));

        clock.Advance(TimeSpan.FromMinutes(5));
        Assert.False(await store.ValidateAndRemoveAsync(Mobile, Code));

        clock.SetUtcNow(new DateTime(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc));
        Assert.False(await store.ValidateAndRemoveAsync(Mobile, Code));
    }

    [Fact]
    public async Task One_incorrect_attempt_does_not_invalidate_otp()
    {
        var store = CreateStore(maxFailedAttempts: 5);
        await store.StoreAsync(Mobile, Code, TimeSpan.FromMinutes(5));

        Assert.False(await store.ValidateAndRemoveAsync(Mobile, "000000"));
        Assert.True(await store.ValidateAndRemoveAsync(Mobile, Code));
    }

    [Fact]
    public async Task Four_incorrect_attempts_still_allow_correct_code_when_max_is_five()
    {
        var store = CreateStore(maxFailedAttempts: 5);
        await store.StoreAsync(Mobile, Code, TimeSpan.FromMinutes(5));

        for (var i = 0; i < 4; i++)
        {
            Assert.False(await store.ValidateAndRemoveAsync(Mobile, "000000"));
        }

        Assert.True(await store.ValidateAndRemoveAsync(Mobile, Code));
    }

    [Fact]
    public async Task Fifth_incorrect_attempt_invalidates_otp()
    {
        var store = CreateStore(maxFailedAttempts: 5);
        await store.StoreAsync(Mobile, Code, TimeSpan.FromMinutes(5));

        for (var i = 0; i < 5; i++)
        {
            Assert.False(await store.ValidateAndRemoveAsync(Mobile, "000000"));
        }

        Assert.False(await store.ValidateAndRemoveAsync(Mobile, Code));
    }

    [Fact]
    public async Task Correct_code_after_max_attempts_fails()
    {
        var store = CreateStore(maxFailedAttempts: 5);
        await store.StoreAsync(Mobile, Code, TimeSpan.FromMinutes(5));

        for (var i = 0; i < 5; i++)
        {
            await store.ValidateAndRemoveAsync(Mobile, "111111");
        }

        Assert.False(await store.ValidateAndRemoveAsync(Mobile, Code));
    }

    [Fact]
    public async Task Storing_replacement_otp_resets_failed_attempt_count()
    {
        var store = CreateStore(maxFailedAttempts: 5);
        await store.StoreAsync(Mobile, "111111", TimeSpan.FromMinutes(5));

        for (var i = 0; i < 4; i++)
        {
            Assert.False(await store.ValidateAndRemoveAsync(Mobile, "000000"));
        }

        await store.StoreAsync(Mobile, "222222", TimeSpan.FromMinutes(5));

        for (var i = 0; i < 4; i++)
        {
            Assert.False(await store.ValidateAndRemoveAsync(Mobile, "000000"));
        }

        Assert.True(await store.ValidateAndRemoveAsync(Mobile, "222222"));
    }

    [Fact]
    public async Task Failed_attempt_counts_are_isolated_between_mobile_numbers()
    {
        var store = CreateStore(maxFailedAttempts: 5);
        await store.StoreAsync("09120000001", "111111", TimeSpan.FromMinutes(5));
        await store.StoreAsync("09120000002", "222222", TimeSpan.FromMinutes(5));

        for (var i = 0; i < 5; i++)
        {
            Assert.False(await store.ValidateAndRemoveAsync("09120000001", "000000"));
        }

        Assert.False(await store.ValidateAndRemoveAsync("09120000001", "111111"));
        Assert.True(await store.ValidateAndRemoveAsync("09120000002", "222222"));
    }

    [Fact]
    public async Task Store_replaces_previous_otp_for_same_mobile()
    {
        var store = CreateStore();
        await store.StoreAsync(Mobile, "111111", TimeSpan.FromMinutes(5));
        await store.StoreAsync(Mobile, "222222", TimeSpan.FromMinutes(5));

        Assert.False(await store.ValidateAndRemoveAsync(Mobile, "111111"));
        Assert.True(await store.ValidateAndRemoveAsync(Mobile, "222222"));
    }

    [Fact]
    public async Task Otps_are_isolated_per_mobile_number()
    {
        var store = CreateStore();
        await store.StoreAsync("09120000001", "111111", TimeSpan.FromMinutes(5));
        await store.StoreAsync("09120000002", "222222", TimeSpan.FromMinutes(5));

        Assert.False(await store.ValidateAndRemoveAsync("09120000001", "222222"));
        Assert.True(await store.ValidateAndRemoveAsync("09120000001", "111111"));
        Assert.True(await store.ValidateAndRemoveAsync("09120000002", "222222"));
    }

    [Fact]
    public async Task Store_uses_injected_date_time_provider_for_expiration()
    {
        var clock = new FakeDateTimeProvider(new DateTime(2026, 6, 1, 8, 0, 0, DateTimeKind.Utc));
        var store = CreateStore(clock);
        await store.StoreAsync(Mobile, Code, TimeSpan.FromMinutes(10));

        clock.Advance(TimeSpan.FromMinutes(9));
        Assert.True(await store.ValidateAndRemoveAsync(Mobile, Code));
    }

    private static InMemoryOtpStore CreateStore(
        FakeDateTimeProvider? clock = null,
        int maxFailedAttempts = 5)
    {
        clock ??= new FakeDateTimeProvider(new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc));
        var options = Options.Create(new OtpSettings { MaxFailedAttempts = maxFailedAttempts });
        return new InMemoryOtpStore(clock, options);
    }
}

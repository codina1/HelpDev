using HelpDev.Identity.Tests.Fakes;
using HelpDev.Modules.Identity.Application.Auth;
using HelpDev.Modules.Identity.Application.Auth.Dtos;
using HelpDev.Modules.Identity.Domain.Entities;
using HelpDev.Modules.Identity.Domain.Enums;
using HelpDev.SharedContracts.Auditing;
using HelpDev.Testing.Auditing;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace HelpDev.Identity.Tests;

public sealed class AuthServiceTests
{
    [Fact]
    public async Task SendOtp_with_valid_mobile_stores_otp_and_returns_response()
    {
        var otpStore = new FakeOtpStore();
        var service = CreateService(otpStore: otpStore, exposeOtp: true);

        var response = await service.SendOtpAsync(new SendOtpRequest { Mobile = "+989123456789" });

        Assert.Equal("کد تأیید ارسال شد.", response.Message);
        Assert.Equal(300, response.ExpiresInSeconds);
        Assert.False(string.IsNullOrWhiteSpace(response.Otp));
        Assert.Equal(6, response.Otp!.Length);
        Assert.True(otpStore.Entries.ContainsKey("09123456789"));
    }

    [Fact]
    public async Task SendOtp_with_invalid_mobile_throws_AuthException()
    {
        var service = CreateService();

        var ex = await Assert.ThrowsAsync<AuthException>(() =>
            service.SendOtpAsync(new SendOtpRequest { Mobile = "12345" }));

        Assert.Equal("شماره موبایل معتبر نیست.", ex.Message);
    }

    [Fact]
    public async Task VerifyOtp_with_incorrect_code_throws_AuthException()
    {
        var otpStore = new FakeOtpStore();
        otpStore.Seed("09123456789", "123456", TimeSpan.FromMinutes(5));
        var service = CreateService(otpStore: otpStore);

        var ex = await Assert.ThrowsAsync<AuthException>(() =>
            service.VerifyOtpAsync(new VerifyOtpRequest
            {
                Mobile = "09123456789",
                Code = "000000",
            }));

        Assert.Equal("کد تأیید نامعتبر یا منقضی شده است.", ex.Message);
    }

    [Fact]
    public async Task VerifyOtp_with_invalid_code_length_throws_AuthException()
    {
        var service = CreateService();

        var ex = await Assert.ThrowsAsync<AuthException>(() =>
            service.VerifyOtpAsync(new VerifyOtpRequest
            {
                Mobile = "09123456789",
                Code = "123",
            }));

        Assert.Equal("کد تأیید معتبر نیست.", ex.Message);
    }

    [Fact]
    public async Task VerifyOtp_for_new_user_persists_once_with_default_role()
    {
        var users = new FakeUserRepository();
        var otpStore = new FakeOtpStore();
        var jwt = new FakeJwtTokenService();
        otpStore.Seed("09123456789", "123456", TimeSpan.FromMinutes(5));
        var service = CreateService(users, otpStore, jwt);

        var response = await service.VerifyOtpAsync(new VerifyOtpRequest
        {
            Mobile = "09123456789",
            Code = "123456",
        });

        Assert.Equal(1, users.AddCount);
        Assert.Equal(0, users.UpdateCount);
        Assert.Single(users.Users);
        var user = users.Users.Single();
        Assert.Equal("09123456789", user.Mobile);
        Assert.Equal(UserRole.User, user.Role);
        Assert.Equal("test-access-token", response.AccessToken);
        Assert.Equal(3600, response.ExpiresIn);
        Assert.Equal(user.Id, response.User.Id);
        Assert.Equal("User", response.User.Role);
        Assert.Equal("09123456789", response.User.Mobile);
        Assert.Equal(user.Id, jwt.LastUserId);
        Assert.Equal(UserRole.User, jwt.LastRole);
        Assert.Equal("09123456789", jwt.LastMobile);
    }

    [Fact]
    public async Task VerifyOtp_for_existing_user_updates_last_login_and_does_not_add()
    {
        var existing = new User
        {
            Id = Guid.Parse("aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee"),
            Mobile = "09123456789",
            Role = UserRole.Writer,
            FirstName = "Sara",
            LastName = "Ahmadi",
            CreatedAt = DateTime.UtcNow.AddDays(-10),
        };
        var users = new FakeUserRepository();
        users.Seed(existing);
        var otpStore = new FakeOtpStore();
        otpStore.Seed("09123456789", "654321", TimeSpan.FromMinutes(5));
        var jwt = new FakeJwtTokenService();
        var service = CreateService(users, otpStore, jwt);

        var response = await service.VerifyOtpAsync(new VerifyOtpRequest
        {
            Mobile = "+989123456789",
            Code = "654321",
        });

        Assert.Equal(0, users.AddCount);
        Assert.Equal(1, users.UpdateCount);
        Assert.Equal(existing.Id, response.User.Id);
        Assert.Equal("Writer", response.User.Role);
        Assert.Equal("Sara", response.User.FirstName);
        Assert.Equal(UserRole.Writer, jwt.LastRole);
        Assert.NotNull(existing.LastLogin);
    }

    [Fact]
    public async Task SendOtp_records_otp_requested_audit_without_phone()
    {
        var audit = new FakeAuditRecorder();
        var service = CreateService(auditRecorder: audit);

        await service.SendOtpAsync(new SendOtpRequest { Mobile = "+989123456789" });

        var record = Assert.Single(audit.Recorded);
        Assert.Equal(AuditActions.AuthenticationOtpRequested, record.Action);
        Assert.Equal("otp", record.Metadata!["method"]);
        Assert.DoesNotContain(audit.Recorded, r => r.Metadata?.ContainsKey("phone") == true);
    }

    [Fact]
    public async Task VerifyOtp_failure_records_audit_before_throw()
    {
        var audit = new FakeAuditRecorder();
        var otpStore = new FakeOtpStore();
        otpStore.Seed("09123456789", "123456", TimeSpan.FromMinutes(5));
        var service = CreateService(otpStore: otpStore, auditRecorder: audit);

        await Assert.ThrowsAsync<AuthException>(() =>
            service.VerifyOtpAsync(new VerifyOtpRequest { Mobile = "09123456789", Code = "000000" }));

        Assert.Contains(
            audit.Recorded,
            record => record.Action == AuditActions.AuthenticationOtpVerificationFailed
                && record.Outcome == AuditOutcomes.Failure);
    }

    private static AuthService CreateService(
        FakeUserRepository? users = null,
        FakeOtpStore? otpStore = null,
        FakeJwtTokenService? jwt = null,
        bool exposeOtp = false,
        IAuditRecorder? auditRecorder = null)
    {
        var authSettings = Options.Create(new AuthSettings
        {
            OtpExpirationMinutes = 5,
            ExposeOtpInResponse = exposeOtp,
        });

        return new AuthService(
            otpStore ?? new FakeOtpStore(),
            users ?? new FakeUserRepository(),
            jwt ?? new FakeJwtTokenService(),
            new HelpDev.Testing.Analytics.NoOpAnalyticsEventIngestor(),
            auditRecorder ?? new NoOpAuditRecorder(),
            new FakeAuditRequestContext(),
            authSettings,
            NullLogger<AuthService>.Instance);
    }
}

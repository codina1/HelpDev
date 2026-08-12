using HelpDev.Modules.Identity.Application.Auth.Dtos;

using HelpDev.Modules.Identity.Application.Common;

using HelpDev.Modules.Identity.Application.Persistence;

using HelpDev.Modules.Identity.Application.Profiles;

using HelpDev.Modules.Identity.Domain.Entities;

using HelpDev.Modules.Identity.Domain.Enums;

using HelpDev.SharedContracts.Analytics;

using HelpDev.SharedContracts.Auditing;

using Microsoft.Extensions.Logging;

using Microsoft.Extensions.Options;



namespace HelpDev.Modules.Identity.Application.Auth;



public sealed class AuthService : IAuthService

{

    private readonly IOtpStore _otpStore;

    private readonly IUserRepository _userRepository;

    private readonly IJwtTokenService _jwtTokenService;

    private readonly IAnalyticsEventIngestor _analyticsIngestor;

    private readonly IAuditRecorder _auditRecorder;

    private readonly IAuditRequestContext _auditRequestContext;

    private readonly AuthSettings _authSettings;

    private readonly ILogger<AuthService> _logger;



    public AuthService(

        IOtpStore otpStore,

        IUserRepository userRepository,

        IJwtTokenService jwtTokenService,

        IAnalyticsEventIngestor analyticsIngestor,

        IAuditRecorder auditRecorder,

        IAuditRequestContext auditRequestContext,

        IOptions<AuthSettings> authSettings,

        ILogger<AuthService> logger)

    {

        _otpStore = otpStore;

        _userRepository = userRepository;

        _jwtTokenService = jwtTokenService;

        _analyticsIngestor = analyticsIngestor;

        _auditRecorder = auditRecorder;

        _auditRequestContext = auditRequestContext;

        _authSettings = authSettings.Value;

        _logger = logger;

    }



    public async Task<SendOtpResponse> SendOtpAsync(

        SendOtpRequest request,

        CancellationToken cancellationToken = default)

    {

        if (!MobileNormalizer.TryNormalize(request.Mobile, out var mobile))

        {

            throw new AuthException("شماره موبایل معتبر نیست.");

        }



        var code = GenerateOtpCode();

        var expiration = TimeSpan.FromMinutes(_authSettings.OtpExpirationMinutes);



        await _otpStore.StoreAsync(mobile, code, expiration, cancellationToken);



        _logger.LogInformation("OTP requested. Operation={Operation}", "otp_requested");



        await _auditRecorder.RecordAsync(new AuditRecordInput(

            Category: AuditCategories.Authentication,

            Action: AuditActions.AuthenticationOtpRequested,

            Outcome: AuditOutcomes.Success,

            ActorUserId: null,

            ActorType: AuditActorTypes.Anonymous,

            CorrelationId: _auditRequestContext.CorrelationId,

            RequestMethod: _auditRequestContext.RequestMethod,

            RequestPathTemplate: _auditRequestContext.RequestPathTemplate,

            Metadata: new Dictionary<string, string> { ["method"] = "otp" }), cancellationToken);



        return new SendOtpResponse(

            Message: "کد تأیید ارسال شد.",

            ExpiresInSeconds: (int)expiration.TotalSeconds,

            Otp: _authSettings.ExposeOtpInResponse ? code : null);

    }



    public async Task<AuthResponse> VerifyOtpAsync(

        VerifyOtpRequest request,

        CancellationToken cancellationToken = default)

    {

        if (!MobileNormalizer.TryNormalize(request.Mobile, out var mobile))

        {

            throw new AuthException("شماره موبایل معتبر نیست.");

        }



        if (string.IsNullOrWhiteSpace(request.Code) || request.Code.Length != 6)

        {

            await RecordOtpVerificationFailedAsync("invalid_code_format", cancellationToken);

            throw new AuthException("کد تأیید معتبر نیست.");

        }



        var isValid = await _otpStore.ValidateAndRemoveAsync(mobile, request.Code.Trim(), cancellationToken);

        if (!isValid)

        {

            await RecordOtpVerificationFailedAsync("invalid_or_expired", cancellationToken);

            throw new AuthException("کد تأیید نامعتبر یا منقضی شده است.");

        }



        var user = await _userRepository.GetByMobileAsync(mobile, cancellationToken);

        var isNewUser = user is null;

        if (isNewUser)

        {

            user = new User

            {

                Id = Guid.NewGuid(),

                Mobile = mobile,

                FullName = string.Empty,

                Role = UserRole.User,

                Stack = string.Empty,

                CreatedAt = DateTime.UtcNow,

                LastLogin = DateTime.UtcNow,

            };



            await _userRepository.AddAsync(user, cancellationToken);

        }

        else if (user is not null)
        {
            user.LastLogin = DateTime.UtcNow;
            await _userRepository.UpdateAsync(user, cancellationToken);
        }
        else
        {
            throw new AuthException("کد تأیید نامعتبر یا منقضی شده است.");
        }



        await _auditRecorder.RecordAsync(new AuditRecordInput(

            Category: AuditCategories.Authentication,

            Action: AuditActions.AuthenticationOtpVerified,

            Outcome: AuditOutcomes.Success,

            ActorUserId: user!.Id,

            ActorType: AuditActorTypes.User,

            SubjectId: user.Id,

            SubjectType: "User",

            CorrelationId: _auditRequestContext.CorrelationId,

            RequestMethod: _auditRequestContext.RequestMethod,

            RequestPathTemplate: _auditRequestContext.RequestPathTemplate,

            Metadata: new Dictionary<string, string> { ["method"] = "otp" }), cancellationToken);



        await TryIngestAuthEventAsync(user, isNewUser, cancellationToken);



        var (token, expiresIn) = _jwtTokenService.GenerateToken(user.Id, user.Role, user.Mobile);



        return new AuthResponse(

            AccessToken: token,

            ExpiresIn: expiresIn,

            User: MapAuthUser(user));

    }



    private async Task RecordOtpVerificationFailedAsync(string failureCode, CancellationToken cancellationToken)

    {

        await _auditRecorder.RecordAsync(new AuditRecordInput(

            Category: AuditCategories.Authentication,

            Action: AuditActions.AuthenticationOtpVerificationFailed,

            Outcome: AuditOutcomes.Failure,

            ActorUserId: null,

            ActorType: AuditActorTypes.Anonymous,

            CorrelationId: _auditRequestContext.CorrelationId,

            RequestMethod: _auditRequestContext.RequestMethod,

            RequestPathTemplate: _auditRequestContext.RequestPathTemplate,

            Metadata: new Dictionary<string, string>

            {

                ["method"] = "otp",

                ["failureCode"] = failureCode,

            }), cancellationToken);

    }



    private static AuthUserDto MapAuthUser(User user)

    {

        var profile = UserProfileMapper.ToDto(user);

        return new AuthUserDto(

            profile.Id,

            profile.Mobile,

            profile.Role,

            profile.FirstName,

            profile.LastName,

            profile.DisplayName,

            profile.Email,

            profile.ProfileImageUrl,

            profile.Expertise,

            profile.Interests);

    }



    private static string GenerateOtpCode() =>

        Random.Shared.Next(100000, 999999).ToString();



    private async Task TryIngestAuthEventAsync(User user, bool isNewUser, CancellationToken cancellationToken)

    {

        try

        {

            var occurredAt = DateTime.UtcNow;

            if (isNewUser)

            {

                await _analyticsIngestor.IngestAsync(

                    new AnalyticsEventEnvelope(

                        Guid.NewGuid(),

                        AnalyticsEventTypes.IdentityUserRegistered,

                        occurredAt,

                        user.Id,

                        SubjectId: user.Id,

                        SubjectType: null,

                        Dimensions: new Dictionary<string, string>

                        {

                            [AnalyticsDimensionKeys.RegistrationMethod] = "otp",

                        }),

                    cancellationToken);

            }

            else

            {

                await _analyticsIngestor.IngestAsync(

                    new AnalyticsEventEnvelope(

                        Guid.NewGuid(),

                        AnalyticsEventTypes.IdentityUserLoginSucceeded,

                        occurredAt,

                        user.Id,

                        SubjectId: user.Id,

                        SubjectType: null,

                        Dimensions: null),

                    cancellationToken);

            }

        }

        catch (Exception ex)

        {

            _logger.LogWarning(

                ex,

                "Analytics auth event ingestion skipped. Operation={Operation}",

                "analytics_auth_ingestion_skipped");

        }

    }

}



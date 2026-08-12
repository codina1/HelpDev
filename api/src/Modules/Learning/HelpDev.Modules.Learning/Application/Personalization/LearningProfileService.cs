using HelpDev.Modules.Learning.Application.Persistence;
using HelpDev.Modules.Learning.Application.Personalization;
using HelpDev.Modules.Learning.Domain.Personalization;
using HelpDev.SharedApplication.Abstractions.Persistence;
using HelpDev.SharedKernel.Time;

namespace HelpDev.Modules.Learning.Application.Personalization;

public sealed class LearningProfileService : ILearningProfileService
{
    private readonly ILearningProfileRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDateTimeProvider _clock;

    public LearningProfileService(
        ILearningProfileRepository repository,
        IUnitOfWork unitOfWork,
        IDateTimeProvider clock)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _clock = clock;
    }

    public async Task<LearningProfileDto> GetAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        EnsureUserId(userId);
        var profile = await _repository.GetByUserIdAsync(userId, cancellationToken);
        if (profile is null)
        {
            return EmptyDto(userId);
        }

        return Map(profile);
    }

    public async Task<LearningProfileDto> UpsertAsync(
        Guid userId,
        UpdateLearningProfileRequest request,
        CancellationToken cancellationToken = default)
    {
        EnsureUserId(userId);
        ArgumentNullException.ThrowIfNull(request);

        if (!Enum.TryParse<ExperienceLevel>(request.ExperienceLevel, ignoreCase: true, out var level))
        {
            throw new LearningPersonalizationException(
                "سطح تجربه نامعتبر است.",
                LearningPersonalizationErrorCodes.InvalidExperienceLevel);
        }

        var preferences = (request.PreferredTopics ?? [])
            .Select(p => new LearningPreferenceInput(p.Topic, p.Priority, p.InterestLevel))
            .ToList();

        var now = _clock.UtcNow;
        var existing = await _repository.GetByUserIdAsync(userId, cancellationToken);
        if (existing is null)
        {
            var created = LearningProfile.Create(
                Guid.NewGuid(),
                userId,
                level,
                request.LearningGoals,
                request.CurrentSkills,
                now);
            created.Update(level, request.LearningGoals, request.CurrentSkills, preferences, now);
            await _repository.AddAsync(created, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return Map(created);
        }

        existing.Update(level, request.LearningGoals, request.CurrentSkills, preferences, now);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Map(existing);
    }

    private static LearningProfileDto EmptyDto(Guid userId) =>
        new(
            userId,
            ExperienceLevel.Beginner.ToString(),
            string.Empty,
            string.Empty,
            [],
            DateTime.MinValue,
            DateTime.MinValue);

    private static LearningProfileDto Map(LearningProfile profile) =>
        new(
            profile.UserId,
            profile.ExperienceLevel.ToString(),
            profile.LearningGoals,
            profile.CurrentSkills,
            profile.Preferences
                .OrderBy(p => p.SortOrder)
                .Select(p => new LearningPreferenceDto(p.Topic, p.Priority, p.InterestLevel))
                .ToList(),
            profile.CreatedAtUtc,
            profile.UpdatedAtUtc);

    private static void EnsureUserId(Guid userId)
    {
        if (userId == Guid.Empty)
        {
            throw new LearningPersonalizationException(
                "شناسه کاربر نامعتبر است.",
                LearningPersonalizationErrorCodes.InvalidUser);
        }
    }
}

public sealed class LearningPersonalizationException : Exception
{
    public LearningPersonalizationException(string message, string code)
        : base(message)
    {
        Code = code;
    }

    public string Code { get; }
}

public static class LearningPersonalizationErrorCodes
{
    public const string InvalidUser = "learning_personalization_invalid_user";
    public const string InvalidExperienceLevel = "learning_personalization_invalid_level";
    public const string ProfileRequired = "learning_personalization_profile_required";
    public const string RoadmapNotFound = "learning_personalization_roadmap_not_found";
    public const string ProviderFailed = "learning_personalization_provider_failed";
}

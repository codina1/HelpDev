using HelpDev.Modules.Learning.Application.Persistence;
using HelpDev.Modules.Learning.Application.Personalization;
using HelpDev.SharedKernel.Time;
using Microsoft.EntityFrameworkCore;

namespace HelpDev.Modules.Learning.Infrastructure.Persistence;

public sealed class LearningPersonalizationAdminQueries : ILearningPersonalizationAdminQueries
{
    private readonly ILearningDbContext _db;
    private readonly IDateTimeProvider _clock;

    public LearningPersonalizationAdminQueries(ILearningDbContext db, IDateTimeProvider clock)
    {
        _db = db;
        _clock = clock;
    }

    public async Task<LearningPersonalizationAdminDto> GetSummaryAsync(CancellationToken cancellationToken = default)
    {
        var profileCount = await _db.LearningProfiles.AsNoTracking().CountAsync(cancellationToken);
        var preferenceCount = await _db.LearningPreferences.AsNoTracking().CountAsync(cancellationToken);
        var roadmaps = await _db.LearningRoadmaps
            .AsNoTracking()
            .Select(r => r.Status)
            .ToListAsync(cancellationToken);

        return new LearningPersonalizationAdminDto(
            profileCount,
            preferenceCount,
            roadmaps.Count,
            roadmaps.Count(s => s == Domain.Personalization.LearningRoadmapStatus.Approved),
            roadmaps.Count(s => s == Domain.Personalization.LearningRoadmapStatus.Suggested),
            _clock.UtcNow);
    }
}

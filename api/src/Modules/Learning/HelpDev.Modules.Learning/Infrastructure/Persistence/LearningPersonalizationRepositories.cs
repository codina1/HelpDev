using HelpDev.Modules.Learning.Application.Persistence;
using HelpDev.Modules.Learning.Application.Personalization;
using HelpDev.Modules.Learning.Domain.Personalization;
using Microsoft.EntityFrameworkCore;

namespace HelpDev.Modules.Learning.Infrastructure.Persistence;

public sealed class LearningProfileRepository : ILearningProfileRepository
{
    private readonly ILearningDbContext _db;

    public LearningProfileRepository(ILearningDbContext db)
    {
        _db = db;
    }

    public Task<LearningProfile?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default) =>
        _db.LearningProfiles
            .Include(p => p.Preferences)
            .FirstOrDefaultAsync(p => p.UserId == userId, cancellationToken);

    public async Task AddAsync(LearningProfile profile, CancellationToken cancellationToken = default) =>
        await _db.LearningProfiles.AddAsync(profile, cancellationToken);
}

public sealed class LearningRoadmapRepository : ILearningRoadmapRepository
{
    private readonly ILearningDbContext _db;

    public LearningRoadmapRepository(ILearningDbContext db)
    {
        _db = db;
    }

    public Task<LearningRoadmap?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default) =>
        _db.LearningRoadmaps
            .Include(r => r.Steps)
            .FirstOrDefaultAsync(r => r.UserId == userId, cancellationToken);

    public async Task AddAsync(LearningRoadmap roadmap, CancellationToken cancellationToken = default) =>
        await _db.LearningRoadmaps.AddAsync(roadmap, cancellationToken);
}

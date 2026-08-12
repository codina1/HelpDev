using HelpDev.Modules.Learning.Domain.Courses;
using HelpDev.Modules.Learning.Domain.Enrollments;
using HelpDev.Modules.Learning.Domain.Personalization;
using Microsoft.EntityFrameworkCore;

namespace HelpDev.Modules.Learning.Application.Persistence;

/// <summary>
/// Persistence port for Learning. Implemented by the shared ApplicationDbContext
/// so the module does not reference legacy Infrastructure.
/// </summary>
public interface ILearningDbContext
{
    DbSet<Course> Courses { get; }

    DbSet<Enrollment> Enrollments { get; }

    DbSet<LearningProfile> LearningProfiles { get; }

    DbSet<LearningPreference> LearningPreferences { get; }

    DbSet<LearningRoadmap> LearningRoadmaps { get; }

    DbSet<LearningRoadmapStep> LearningRoadmapSteps { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

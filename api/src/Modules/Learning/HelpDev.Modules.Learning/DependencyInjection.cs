using HelpDev.Modules.Learning.Application.Courses;
using HelpDev.Modules.Learning.Application.Enrollments;
using HelpDev.Modules.Learning.Application.Persistence;
using HelpDev.Modules.Learning.Application.Personalization;
using HelpDev.Modules.Learning.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;

namespace HelpDev.Modules.Learning;

/// <summary>
/// DI entry point for the Learning module.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddLearningModule(this IServiceCollection services)
    {
        services.AddScoped<ICourseRepository, CourseRepository>();
        services.AddScoped<IEnrollmentRepository, EnrollmentRepository>();
        services.AddScoped<ICourseQueries, CourseQueries>();
        services.AddScoped<IPublicCourseQueries, PublicCourseQueries>();
        services.AddScoped<ICourseLearningQueries, CourseLearningQueries>();
        services.AddScoped<IEnrollmentQueries, EnrollmentQueries>();
        services.AddScoped<ICourseService, CourseService>();
        services.AddScoped<IEnrollmentService, EnrollmentService>();

        services.AddScoped<ILearningProfileRepository, LearningProfileRepository>();
        services.AddScoped<ILearningRoadmapRepository, LearningRoadmapRepository>();
        services.AddScoped<ILearningProfileService, LearningProfileService>();
        services.AddScoped<ILearningSignalsService, LearningSignalsService>();
        services.AddScoped<ILearningRecommendationService, LearningRecommendationService>();
        services.AddScoped<ILearningRoadmapService, LearningRoadmapService>();
        services.AddScoped<ILearningPersonalizationAdminQueries, LearningPersonalizationAdminQueries>();

        return services;
    }
}

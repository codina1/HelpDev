using HelpDev.Modules.Analytics.Application.ContentAnalytics;
using HelpDev.Modules.Analytics.Application.Handlers;
using HelpDev.Modules.Analytics.Application.Persistence;
using HelpDev.Modules.Analytics.Application.Processing;
using HelpDev.Modules.Analytics.Application.Queries;
using HelpDev.Modules.Analytics.Domain;
using HelpDev.Modules.Analytics.Infrastructure.Persistence;
using HelpDev.Modules.Content.Domain.Events;
using HelpDev.Modules.Learning.Domain.Courses;
using HelpDev.Modules.Learning.Domain.Enrollments;
using HelpDev.SharedApplication.Abstractions.Events;
using HelpDev.SharedContracts.Ai;
using HelpDev.SharedContracts.Analytics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HelpDev.Modules.Analytics;

public static class DependencyInjection
{
    public static IServiceCollection AddAnalyticsModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<AnalyticsOptions>(configuration.GetSection(AnalyticsOptions.SectionName));

        services.AddScoped<IAnalyticsEventReceiptRepository, AnalyticsEventReceiptRepository>();
        services.AddScoped<IDailyMetricRepository, DailyMetricRepository>();
        services.AddScoped<IDailyActiveUserRepository, DailyActiveUserRepository>();
        services.AddScoped<IAnalyticsSubjectSnapshotRepository, AnalyticsSubjectSnapshotRepository>();

        services.AddSingleton<IAnalyticsFailureInjector, NoOpAnalyticsFailureInjector>();
        services.AddScoped<IAnalyticsEventProcessor, AnalyticsEventProcessor>();
        services.AddScoped<IAnalyticsEventIngestor, AnalyticsEventIngestor>();

        services.AddScoped<IAnalyticsOverviewQueries, AnalyticsOverviewQueries>();
        services.AddScoped<IAnalyticsTimeSeriesQueries, AnalyticsTimeSeriesQueries>();
        services.AddScoped<IAnalyticsTopItemsQueries, AnalyticsTopItemsQueries>();
        services.AddScoped<ISearchAnalyticsQueries, SearchAnalyticsQueries>();
        services.AddScoped<IToolboxAnalyticsQueries, ToolboxAnalyticsQueries>();
        services.AddScoped<IPromptLabAnalyticsQueries, PromptLabAnalyticsQueries>();
        services.AddScoped<IContentAnalyticsQueries, ContentAnalyticsQueries>();
        services.AddScoped<IAiAnalyticsQueries, AiAnalyticsQueries>();
        services.AddScoped<IAiUsageRecordRepository, AiUsageRecordRepository>();
        services.AddScoped<IAiUsageRecorder, AiUsageRecorder>();

        services.AddScoped<IDomainEventHandler<ContentPublishedDomainEvent>, ContentPublishedAnalyticsHandler>();
        services.AddScoped<IDomainEventHandler<CoursePublishedDomainEvent>, CoursePublishedAnalyticsHandler>();
        services.AddScoped<IDomainEventHandler<StudentEnrolledDomainEvent>, StudentEnrolledAnalyticsHandler>();
        services.AddScoped<IDomainEventHandler<LessonCompletedDomainEvent>, LessonCompletedAnalyticsHandler>();

        return services;
    }
}

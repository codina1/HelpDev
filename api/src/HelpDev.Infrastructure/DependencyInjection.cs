using HelpDev.Application.Persistence;
using HelpDev.Infrastructure.Administration;
using HelpDev.Infrastructure.Outbox;
using HelpDev.Infrastructure.Outbox.Operations;
using HelpDev.Infrastructure.Persistence;
using HelpDev.Infrastructure.Persistence.Repositories;
using HelpDev.Infrastructure.Analytics;
using HelpDev.Infrastructure.Content;
using HelpDev.Infrastructure.Learning;
using HelpDev.Infrastructure.Search;
using HelpDev.Infrastructure.Seo;
using HelpDev.Modules.Analytics.Application.ContentAnalytics;
using HelpDev.Modules.Content.Application.AiWorkflow;
using HelpDev.Modules.Learning.Application.Personalization;
using HelpDev.Modules.Content.Application.InternalLinks;
using HelpDev.Modules.Administration.Application.Dashboard;
using HelpDev.Modules.Administration.Application.Persistence;
using HelpDev.Modules.Content.Application.Persistence;
using HelpDev.Modules.Content.Domain.Events;
using HelpDev.Modules.Identity.Application.Persistence;
using HelpDev.Modules.Learning.Application.Persistence;
using HelpDev.Modules.Learning.Domain.Courses;
using HelpDev.Modules.Learning.Domain.Enrollments;
using HelpDev.Modules.Search.Application.Contracts;
using HelpDev.Modules.Search.Application.Reindex;
using HelpDev.Modules.Search.Application.Persistence;
using HelpDev.Modules.Toolbox.Application.Persistence;
using HelpDev.Modules.Toolbox.Domain.Tools;
using HelpDev.Modules.PromptLab.Application.Persistence;
using HelpDev.Modules.PromptLab.Domain.Prompts;
using HelpDev.Modules.Analytics.Application.Persistence;
using HelpDev.Modules.Auditing.Application.Persistence;
using HelpDev.Modules.Media.Application.Persistence;
using HelpDev.Infrastructure.Observability;
using HelpDev.SharedContracts.Observability;
using HelpDev.SharedApplication.Abstractions.Persistence;
using HelpDev.SharedInfrastructure.Outbox;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Pgvector.EntityFrameworkCore;

namespace HelpDev.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddPostgreSqlDbContext(configuration);
        services.AddOutbox(configuration);
        services.AddScoped<IDatabaseConnectionChecker, DatabaseConnectionChecker>();
        services.AddScoped<IIdentityDbContext>(sp => sp.GetRequiredService<ApplicationDbContext>());
        services.AddScoped<IContentDbContext>(sp => sp.GetRequiredService<ApplicationDbContext>());
        services.AddScoped<ILearningDbContext>(sp => sp.GetRequiredService<ApplicationDbContext>());
        services.AddScoped<ISearchDbContext>(sp => sp.GetRequiredService<ApplicationDbContext>());
        services.AddScoped<IAdministrationDbContext>(sp => sp.GetRequiredService<ApplicationDbContext>());
        services.AddScoped<IToolboxDbContext>(sp => sp.GetRequiredService<ApplicationDbContext>());
        services.AddScoped<IPromptLabDbContext>(sp => sp.GetRequiredService<ApplicationDbContext>());
        services.AddScoped<IAnalyticsDbContext>(sp => sp.GetRequiredService<ApplicationDbContext>());
        services.AddScoped<IAuditDbContext>(sp => sp.GetRequiredService<ApplicationDbContext>());
        services.AddScoped<IMediaDbContext>(sp => sp.GetRequiredService<ApplicationDbContext>());
        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<ApplicationDbContext>());
        services.AddScoped<IContentSearchSource, ContentKnowledgeSource>();
        services.AddScoped<IInternalLinkSuggestionService, InternalLinkSuggestionService>();
        services.AddScoped<IContentAnalyticsFactsSource, ContentAnalyticsFactsSource>();
        services.AddScoped<LearningKnowledgeSource>();
        services.AddScoped<ICourseSearchSource>(sp => sp.GetRequiredService<LearningKnowledgeSource>());
        services.AddScoped<ILessonSearchSource>(sp => sp.GetRequiredService<LearningKnowledgeSource>());
        services.AddScoped<IToolSearchSource, ToolboxKnowledgeSource>();
        services.AddScoped<IPromptSearchSource, PromptLabKnowledgeSource>();
        services.AddScoped<IWorkflowKnowledgeRetriever, WorkflowKnowledgeRetrieverAdapter>();
        services.AddScoped<ILearningKnowledgeRetriever, LearningKnowledgeRetrieverAdapter>();
        services.AddScoped<ISearchReindexLock, PostgresSearchReindexLock>();
        services.AddScoped<ITestRepository, TestRepository>();
        services.AddScoped<IIdentityAdministrationStatisticsSource, IdentityAdministrationStatisticsSource>();
        services.AddScoped<IContentAdministrationStatisticsSource, ContentAdministrationStatisticsSource>();
        services.AddScoped<ILearningAdministrationStatisticsSource, LearningAdministrationStatisticsSource>();
        services.AddScoped<ISearchAdministrationStatisticsSource, SearchAdministrationStatisticsSource>();
        services.AddScoped<IOutboxAdministrationStatisticsSource, OutboxAdministrationStatisticsSource>();
        services.AddScoped<IAnalyticsAdministrationStatisticsSource, AnalyticsAdministrationStatisticsSource>();

        return services;
    }

    public static IServiceCollection AddPostgreSqlDbContext(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "Connection string 'DefaultConnection' is not configured.");
        }

        services.AddSingleton<IValidateOptions<DatabaseStartupOptions>, DatabaseStartupOptionsValidator>();
        services.AddOptions<DatabaseStartupOptions>()
            .Bind(configuration.GetSection(DatabaseStartupOptions.SectionName))
            .ValidateOnStart();

        var runtimeOptions = configuration
            .GetSection(DatabaseStartupOptions.SectionName)
            .GetSection(nameof(DatabaseStartupOptions.Postgres))
            .Get<PostgreSqlRuntimeOptions>() ?? new PostgreSqlRuntimeOptions();

        var effectiveConnectionString = ApplyPoolSettings(connectionString, runtimeOptions);

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(
                effectiveConnectionString,
                npgsql =>
                {
                    npgsql.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.GetName().Name);
                    npgsql.CommandTimeout(runtimeOptions.CommandTimeoutSeconds);
                    npgsql.UseVector();

                    if (runtimeOptions.EnableRetryOnFailure)
                    {
                        npgsql.EnableRetryOnFailure(
                            maxRetryCount: runtimeOptions.MaxRetryCount,
                            maxRetryDelay: TimeSpan.FromSeconds(runtimeOptions.MaxRetryDelaySeconds),
                            errorCodesToAdd: null);
                    }
                }));

        return services;
    }

    private static string ApplyPoolSettings(string connectionString, PostgreSqlRuntimeOptions options)
    {
        // Merge pool bounds without discarding explicitly configured values in the connection string.
        var builder = new Npgsql.NpgsqlConnectionStringBuilder(connectionString);

        if (!connectionString.Contains("Minimum Pool Size", StringComparison.OrdinalIgnoreCase)
            && !connectionString.Contains("MinPoolSize", StringComparison.OrdinalIgnoreCase))
        {
            builder.MinPoolSize = options.MinPoolSize;
        }

        if (!connectionString.Contains("Maximum Pool Size", StringComparison.OrdinalIgnoreCase)
            && !connectionString.Contains("MaxPoolSize", StringComparison.OrdinalIgnoreCase))
        {
            builder.MaxPoolSize = options.MaxPoolSize;
        }

        if (!connectionString.Contains("Timeout", StringComparison.OrdinalIgnoreCase))
        {
            builder.Timeout = options.ConnectionTimeoutSeconds;
        }

        if (options.KeepAliveSeconds > 0
            && !connectionString.Contains("Keepalive", StringComparison.OrdinalIgnoreCase))
        {
            builder.KeepAlive = options.KeepAliveSeconds;
        }

        return builder.ConnectionString;
    }

    public static IServiceCollection AddOutbox(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddSingleton<IValidateOptions<OutboxOptions>, OutboxOptionsValidator>();
        services.AddOptions<OutboxOptions>()
            .Bind(configuration.GetSection(OutboxOptions.SectionName))
            .ValidateOnStart();

        services.AddSingleton<IOutboxEventTypeRegistry>(_ =>
        {
            var registry = new OutboxEventTypeRegistry();
            RegisterKnownDomainEvents(registry);
            registry.Seal();
            return registry;
        });

        services.AddSingleton<IOutboxEventSerializer, SystemTextJsonOutboxEventSerializer>();
        services.AddScoped<IOutboxMessageStore, OutboxMessageStore>();
        services.AddScoped<IOutboxRetryStore, EfOutboxRetryStore>();
        services.AddScoped<IOutboxOperationsQueries, OutboxOperationsQueries>();
        services.AddScoped<IOutboxOperationsService, OutboxOperationsService>();
        services.AddSingleton<IHealthSnapshotCache, HealthSnapshotCache>();
        services.AddSingleton<OutboxProcessorHeartbeat>();
        services.AddSingleton<IOperationalSafeDetailsSanitizer, OperationalSafeDetailsSanitizer>();

        services.AddScoped<IOutboxOperationalQueries, OutboxOperationalQueries>();
        services.AddScoped<ISearchOperationalDataSource, SearchOperationalDataSource>();
        services.AddScoped<ISearchOperationalQueries, SearchOperationalQueries>();
        services.AddScoped<IAnalyticsOperationalQueries, AnalyticsOperationalQueries>();
        services.AddScoped<IAuditOperationalQueries, AuditOperationalQueries>();
        services.AddScoped<IPostgreSqlHealthProbe, PostgreSqlHealthProbe>();
        services.AddScoped<IOperationalStatusService, OperationalStatusService>();

        var outboxOptions = configuration.GetSection(OutboxOptions.SectionName).Get<OutboxOptions>() ?? new OutboxOptions();
        if (outboxOptions.Enabled)
        {
            services.AddHostedService<OutboxProcessor>();
        }

        return services;
    }

    private static void RegisterKnownDomainEvents(OutboxEventTypeRegistry registry)
    {
        registry.Register<ContentPublishedDomainEvent>("content.published.v1");
        registry.Register<ContentUpdatedDomainEvent>("content.updated.v1");
        registry.Register<CoursePublishedDomainEvent>("learning.course-published.v1");
        registry.Register<CourseUpdatedDomainEvent>("learning.course-updated.v1");
        registry.Register<LessonPublishedDomainEvent>("learning.lesson-published.v1");
        registry.Register<StudentEnrolledDomainEvent>("learning.student-enrolled.v1");
        registry.Register<LessonCompletedDomainEvent>("learning.lesson-completed.v1");
        registry.Register<CourseCompletedDomainEvent>("learning.course-completed.v1");
        registry.Register<ToolPublishedDomainEvent>("toolbox.tool-published.v1");
        registry.Register<ToolUnpublishedDomainEvent>("toolbox.tool-unpublished.v1");
        registry.Register<PromptPublishedDomainEvent>("promptlab.prompt-published.v1");
        registry.Register<PromptUnpublishedDomainEvent>("promptlab.prompt-unpublished.v1");
    }
}

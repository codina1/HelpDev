using HelpDev.Modules.Content.Domain.Events;
using HelpDev.Modules.Learning.Domain.Courses;
using HelpDev.Modules.Learning.Domain.Enrollments;
using HelpDev.Modules.Search;
using HelpDev.Modules.Search.Application.Chunking;
using HelpDev.Modules.Search.Application.Contracts;
using HelpDev.Modules.Search.Application.Knowledge;
using HelpDev.Modules.Search.Application.Persistence;
using HelpDev.Modules.Search.Application.Rag;
using HelpDev.Modules.Search.Application.Semantic;
using HelpDev.Modules.Search.Domain;
using HelpDev.Search.Tests.Fakes;
using HelpDev.SharedApplication.Abstractions.Events;
using HelpDev.SharedApplication.Abstractions.Persistence;
using HelpDev.SharedContracts.Ai;
using HelpDev.SharedContracts.Auditing;
using HelpDev.SharedInfrastructure.Events;
using HelpDev.SharedKernel.Time;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace HelpDev.Search.Tests;

public sealed class SearchHandlerRegistrationTests
{
    [Fact]
    public void Production_handlers_resolve_for_mapped_content_and_course_events()
    {
        using var provider = BuildProvider();
        using var scope = provider.CreateScope();

        Assert.Equal(2, scope.ServiceProvider.GetServices<IDomainEventHandler<ContentPublishedDomainEvent>>().Count());
        Assert.Equal(2, scope.ServiceProvider.GetServices<IDomainEventHandler<ContentUpdatedDomainEvent>>().Count());
        Assert.Equal(2, scope.ServiceProvider.GetServices<IDomainEventHandler<CoursePublishedDomainEvent>>().Count());
        Assert.Equal(2, scope.ServiceProvider.GetServices<IDomainEventHandler<CourseUpdatedDomainEvent>>().Count());
        Assert.Single(scope.ServiceProvider.GetServices<IDomainEventHandler<LessonPublishedDomainEvent>>());
        Assert.Empty(scope.ServiceProvider.GetServices<IDomainEventHandler<StudentEnrolledDomainEvent>>());
    }

    [Fact]
    public async Task Handler_failure_propagates_through_dispatcher_for_outbox_retry()
    {
        var contentSource = new FakeContentSearchSource
        {
            ExceptionToThrow = new InvalidOperationException("temporary"),
        };
        using var provider = BuildProvider(contentSource);
        using var scope = provider.CreateScope();
        var dispatcher = scope.ServiceProvider.GetRequiredService<IDomainEventDispatcher>();

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            dispatcher.DispatchAsync([new ContentPublishedDomainEvent(Guid.NewGuid(), "x")]));
    }

    [Fact]
    public async Task CourseUpdated_handler_failure_propagates_through_dispatcher()
    {
        var courseSource = new FakeCourseSearchSource
        {
            ExceptionToThrow = new InvalidOperationException("temporary course source"),
        };
        using var provider = BuildProvider(courseSource: courseSource);
        using var scope = provider.CreateScope();
        var dispatcher = scope.ServiceProvider.GetRequiredService<IDomainEventDispatcher>();

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            dispatcher.DispatchAsync([new CourseUpdatedDomainEvent(Guid.NewGuid())]));
    }

    [Fact]
    public async Task Outbox_dispatcher_invokes_CourseUpdated_handler()
    {
        var courseId = Guid.NewGuid();
        var at = DateTime.UtcNow;
        var courseSource = new FakeCourseSearchSource
        {
            Document = new SearchSourceDocument(
                courseId,
                SearchSourceTypes.Course,
                "Title",
                "title",
                "Summary",
                "/courses/title",
                true,
                at,
                at),
        };
        var repository = new FakeSearchDocumentRepository();
        var unitOfWork = new FakeUnitOfWork();
        using var provider = BuildProvider(
            courseSource: courseSource,
            documentRepository: repository,
            unitOfWork: unitOfWork,
            clock: new FakeDateTimeProvider(at));
        using var scope = provider.CreateScope();
        var dispatcher = scope.ServiceProvider.GetRequiredService<IDomainEventDispatcher>();

        await dispatcher.DispatchAsync([new CourseUpdatedDomainEvent(courseId) { OccurredAtUtc = at }]);

        Assert.Equal(courseId, Assert.Single(repository.Documents).SourceId);
        Assert.True(unitOfWork.SaveChangesCount >= 1);
    }

    private static ServiceProvider BuildProvider(
        IContentSearchSource? contentSource = null,
        ICourseSearchSource? courseSource = null,
        ISearchDocumentRepository? documentRepository = null,
        IUnitOfWork? unitOfWork = null,
        IDateTimeProvider? clock = null)
    {
        var services = new ServiceCollection();
        services.AddSearchModule();
        services.AddSingleton(contentSource ?? new FakeContentSearchSource());
        services.AddSingleton(courseSource ?? new FakeCourseSearchSource());
        services.AddSingleton<ILessonSearchSource>(new FakeLessonSearchSource());
        services.AddSingleton<IToolSearchSource>(new FakeToolSearchSource());
        services.AddSingleton<IPromptSearchSource>(new FakePromptSearchSource());
        services.AddSingleton(documentRepository ?? new FakeSearchDocumentRepository());
        services.AddSingleton(unitOfWork ?? new FakeUnitOfWork());
        services.AddSingleton(clock ?? new FakeDateTimeProvider(DateTime.UtcNow));
        services.AddSingleton<ISearchChunkRepository, NoOpSearchChunkRepository>();
        services.AddSingleton<ISearchVectorRepository, NoOpSearchVectorRepository>();
        services.AddSingleton<ISearchSemanticIndexStateRepository, NoOpSearchSemanticIndexStateRepository>();
        services.AddSingleton<ISearchDbContext, NoOpSearchDbContext>();
        services.AddSingleton<IEmbeddingGenerator, NoOpEmbeddingGenerator>();
        services.AddSingleton<IAiTextGenerator, NoOpAiTextGenerator>();
        services.AddSingleton<IAuditRecorder, NoOpAuditRecorder>();
        services.AddSingleton<ILoggerFactory>(NullLoggerFactory.Instance);
        services.AddSingleton(typeof(ILogger<>), typeof(Logger<>));
        services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();
        return services.BuildServiceProvider();
    }

    private sealed class NoOpSearchChunkRepository : ISearchChunkRepository
    {
        public Task<IReadOnlyList<SearchChunk>> ListBySourceAsync(string sourceType, Guid sourceId, CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<SearchChunk>>([]);

        public Task AddRangeAsync(IEnumerable<SearchChunk> chunks, CancellationToken cancellationToken = default) =>
            Task.CompletedTask;

        public void RemoveRange(IEnumerable<SearchChunk> chunks)
        {
        }
    }

    private sealed class NoOpSearchVectorRepository : ISearchVectorRepository
    {
        public Task AddRangeAsync(IEnumerable<SearchVector> vectors, CancellationToken cancellationToken = default) =>
            Task.CompletedTask;

        public Task RemoveByChunkIdsAsync(IEnumerable<Guid> chunkIds, CancellationToken cancellationToken = default) =>
            Task.CompletedTask;

        public Task<IReadOnlyList<SemanticHit>> SearchSimilarAsync(float[] queryVector, int take, CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<SemanticHit>>([]);
    }

    private sealed class NoOpSearchSemanticIndexStateRepository : ISearchSemanticIndexStateRepository
    {
        public Task<SearchSemanticIndexState?> GetAsync(string sourceType, Guid sourceId, CancellationToken cancellationToken = default) =>
            Task.FromResult<SearchSemanticIndexState?>(null);

        public Task AddAsync(SearchSemanticIndexState state, CancellationToken cancellationToken = default) =>
            Task.CompletedTask;

        public Task<KnowledgeCounts> GetCountsAsync(
            string? sourceType = null,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(new KnowledgeCounts(0, 0, 0, 0));

        public Task<IReadOnlyList<SearchSemanticIndexState>> ListRecentByStatusAsync(
            string status,
            int take,
            string? sourceType = null,
            CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<SearchSemanticIndexState>>([]);
    }

    private sealed class NoOpSearchDbContext : ISearchDbContext
    {
        public Microsoft.EntityFrameworkCore.DbSet<SearchDocument> SearchDocuments => throw new NotSupportedException();
        public Microsoft.EntityFrameworkCore.DbSet<SearchChunk> SearchChunks => throw new NotSupportedException();
        public Microsoft.EntityFrameworkCore.DbSet<SearchVector> SearchVectors => throw new NotSupportedException();
        public Microsoft.EntityFrameworkCore.DbSet<SearchSemanticIndexState> SearchSemanticIndexStates => throw new NotSupportedException();
        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) => Task.FromResult(0);
    }

    private sealed class NoOpEmbeddingGenerator : IEmbeddingGenerator
    {
        public Task<EmbeddingResult> GenerateAsync(string text, CancellationToken cancellationToken = default) =>
            Task.FromResult(new EmbeddingResult(new float[384], 384, "noop", "Fake"));
    }

    private sealed class NoOpAiTextGenerator : IAiTextGenerator
    {
        public Task<AiTextResponse> GenerateAsync(AiTextRequest request, CancellationToken cancellationToken = default) =>
            Task.FromResult(new AiTextResponse("ok", "m", "Fake", null));

        public Task<AiGenerationResult> GenerateSafeAsync(
            AiTextRequest request,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(AiGenerationResult.Ok("ok", 1, "m", "Fake", null));
    }

    private sealed class NoOpAuditRecorder : IAuditRecorder
    {
        public Task RecordAsync(AuditRecordInput input, CancellationToken cancellationToken = default) =>
            Task.CompletedTask;
    }
}

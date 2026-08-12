using HelpDev.Modules.Content.Domain.Events;
using HelpDev.Modules.Learning.Domain.Courses;
using HelpDev.Modules.Search.Application.Contracts;
using HelpDev.Modules.Search.Application.Handlers;
using HelpDev.Modules.Search.Application.Indexing;
using HelpDev.Modules.Search.Domain;
using HelpDev.Search.Tests.Fakes;
using HelpDev.SharedKernel.Time;

namespace HelpDev.Search.Tests;

public sealed class SearchProjectionHandlerTests
{
    private readonly FakeSearchDocumentRepository _repository = new();
    private readonly FakeUnitOfWork _unitOfWork = new();
    private readonly FakeDateTimeProvider _clock = new(new DateTime(2026, 7, 19, 12, 0, 0, DateTimeKind.Utc));
    private readonly FakeContentSearchSource _contentSource = new();
    private readonly FakeCourseSearchSource _courseSource = new();

    private SearchProjectionService CreateProjection() =>
        new(_repository, _unitOfWork, _clock);

    private CourseSearchProjectionApplier CreateCourseApplier() =>
        new(_courseSource, CreateProjection());

    private CoursePublishedSearchHandler CreateCoursePublishedHandler() =>
        new(CreateCourseApplier());

    private CourseUpdatedSearchHandler CreateCourseUpdatedHandler() =>
        new(CreateCourseApplier());


    [Fact]
    public async Task ContentPublished_creates_projection()
    {
        var contentId = Guid.NewGuid();
        var publishedAt = new DateTime(2026, 7, 1, 0, 0, 0, DateTimeKind.Utc);
        _contentSource.Document = new SearchSourceDocument(
            contentId,
            SearchSourceTypes.Content,
            "Hello World",
            "hello-world",
            "Summary",
            "/content/hello-world",
            IsPublished: true,
            PublishedAtUtc: publishedAt,
            UpdatedAtUtc: publishedAt);

        var domainEvent = new ContentPublishedDomainEvent(contentId, "hello-world");
        var handler = new ContentPublishedSearchHandler(_contentSource, CreateProjection());

        await handler.HandleAsync(domainEvent);

        var document = Assert.Single(_repository.Documents);
        Assert.Equal(SearchSourceTypes.Content, document.SourceType);
        Assert.Equal(contentId, document.SourceId);
        Assert.Equal("Hello World", document.Title);
        Assert.True(document.IsPublished);
        Assert.Equal(domainEvent.EventId, document.LastEventId);
        Assert.Equal(1, _unitOfWork.SaveChangesCount);
    }

    [Fact]
    public async Task ContentUpdated_updates_existing_projection()
    {
        var contentId = Guid.NewGuid();
        var older = new DateTime(2026, 7, 1, 0, 0, 0, DateTimeKind.Utc);
        _repository.Seed(new SearchDocument
        {
            Id = Guid.NewGuid(),
            SourceType = SearchSourceTypes.Content,
            SourceId = contentId,
            Title = "Old",
            Slug = "old",
            Summary = "old",
            Url = "/content/old",
            IsPublished = true,
            SourcePublishedAtUtc = older,
            SourceUpdatedAtUtc = older,
            IndexedAtUtc = older,
            LastEventId = Guid.NewGuid(),
        });

        var newer = new DateTime(2026, 7, 10, 0, 0, 0, DateTimeKind.Utc);
        _contentSource.Document = new SearchSourceDocument(
            contentId,
            SearchSourceTypes.Content,
            "New Title",
            "new-title",
            "New summary",
            "/content/new-title",
            IsPublished: true,
            PublishedAtUtc: older,
            UpdatedAtUtc: newer);

        var domainEvent = new ContentUpdatedDomainEvent(contentId, "new-title")
        {
            OccurredAtUtc = newer,
        };
        var handler = new ContentUpdatedSearchHandler(_contentSource, CreateProjection());

        await handler.HandleAsync(domainEvent);

        var document = Assert.Single(_repository.Documents);
        Assert.Equal("New Title", document.Title);
        Assert.Equal("new-title", document.Slug);
        Assert.Equal(domainEvent.EventId, document.LastEventId);
        Assert.Equal(1, _unitOfWork.SaveChangesCount);
    }

    [Fact]
    public async Task Draft_or_unpublished_content_is_not_indexed_and_existing_is_removed()
    {
        var contentId = Guid.NewGuid();
        _repository.Seed(new SearchDocument
        {
            Id = Guid.NewGuid(),
            SourceType = SearchSourceTypes.Content,
            SourceId = contentId,
            Title = "Published",
            Slug = "published",
            Summary = "s",
            Url = "/content/published",
            IsPublished = true,
            SourceUpdatedAtUtc = DateTime.UtcNow,
            IndexedAtUtc = DateTime.UtcNow,
            LastEventId = Guid.NewGuid(),
        });
        _contentSource.Document = null;

        var handler = new ContentUpdatedSearchHandler(_contentSource, CreateProjection());
        await handler.HandleAsync(new ContentUpdatedDomainEvent(contentId, "published"));

        Assert.Empty(_repository.Documents);
        Assert.Equal(1, _unitOfWork.SaveChangesCount);

        await handler.HandleAsync(new ContentUpdatedDomainEvent(Guid.NewGuid(), "draft"));
        Assert.Empty(_repository.Documents);
        Assert.Equal(1, _unitOfWork.SaveChangesCount);
    }

    [Fact]
    public async Task Duplicate_event_is_noop_and_does_not_commit()
    {
        var contentId = Guid.NewGuid();
        var eventId = Guid.NewGuid();
        var at = new DateTime(2026, 7, 1, 0, 0, 0, DateTimeKind.Utc);
        _repository.Seed(new SearchDocument
        {
            Id = Guid.NewGuid(),
            SourceType = SearchSourceTypes.Content,
            SourceId = contentId,
            Title = "Title",
            Slug = "title",
            Summary = "s",
            Url = "/content/title",
            IsPublished = true,
            SourceUpdatedAtUtc = at,
            IndexedAtUtc = at,
            LastEventId = eventId,
        });
        _contentSource.Document = new SearchSourceDocument(
            contentId,
            SearchSourceTypes.Content,
            "Changed",
            "changed",
            "s",
            "/content/changed",
            true,
            at,
            at);

        var domainEvent = new ContentPublishedDomainEvent(contentId, "title")
        {
            EventId = eventId,
            OccurredAtUtc = at,
        };
        var handler = new ContentPublishedSearchHandler(_contentSource, CreateProjection());

        await handler.HandleAsync(domainEvent);

        Assert.Equal("Title", Assert.Single(_repository.Documents).Title);
        Assert.Equal(0, _unitOfWork.SaveChangesCount);
    }

    [Fact]
    public async Task Older_source_state_does_not_overwrite_newer_projection()
    {
        var contentId = Guid.NewGuid();
        var newer = new DateTime(2026, 7, 10, 0, 0, 0, DateTimeKind.Utc);
        var older = new DateTime(2026, 7, 1, 0, 0, 0, DateTimeKind.Utc);
        _repository.Seed(new SearchDocument
        {
            Id = Guid.NewGuid(),
            SourceType = SearchSourceTypes.Content,
            SourceId = contentId,
            Title = "Newer",
            Slug = "newer",
            Summary = "s",
            Url = "/content/newer",
            IsPublished = true,
            SourceUpdatedAtUtc = newer,
            IndexedAtUtc = newer,
            LastEventId = Guid.NewGuid(),
        });
        _contentSource.Document = new SearchSourceDocument(
            contentId,
            SearchSourceTypes.Content,
            "Older",
            "older",
            "s",
            "/content/older",
            true,
            older,
            older);

        var handler = new ContentUpdatedSearchHandler(_contentSource, CreateProjection());
        await handler.HandleAsync(new ContentUpdatedDomainEvent(contentId, "older")
        {
            OccurredAtUtc = older,
        });

        Assert.Equal("Newer", Assert.Single(_repository.Documents).Title);
        Assert.Equal(0, _unitOfWork.SaveChangesCount);
    }

    [Fact]
    public async Task Unique_source_invariant_is_enforced_by_repository()
    {
        var contentId = Guid.NewGuid();
        var at = DateTime.UtcNow;
        _repository.Seed(new SearchDocument
        {
            Id = Guid.NewGuid(),
            SourceType = SearchSourceTypes.Content,
            SourceId = contentId,
            Title = "A",
            Slug = "a",
            Summary = "s",
            Url = "/content/a",
            IsPublished = true,
            SourceUpdatedAtUtc = at,
            IndexedAtUtc = at,
            LastEventId = Guid.NewGuid(),
        });

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _repository.AddAsync(new SearchDocument
            {
                Id = Guid.NewGuid(),
                SourceType = SearchSourceTypes.Content,
                SourceId = contentId,
                Title = "B",
                Slug = "b",
                Summary = "s",
                Url = "/content/b",
                IsPublished = true,
                SourceUpdatedAtUtc = at,
                IndexedAtUtc = at,
                LastEventId = Guid.NewGuid(),
            }));
    }

    [Fact]
    public async Task CoursePublished_creates_projection()
    {
        var courseId = Guid.NewGuid();
        var publishedAt = new DateTime(2026, 7, 5, 0, 0, 0, DateTimeKind.Utc);
        _courseSource.Document = new SearchSourceDocument(
            courseId,
            SearchSourceTypes.Course,
            "C# Course",
            "csharp-course",
            "Learn C#",
            "/courses/csharp-course",
            true,
            publishedAt,
            publishedAt);

        var domainEvent = new CoursePublishedDomainEvent(courseId, "csharp-course");
        await CreateCoursePublishedHandler().HandleAsync(domainEvent);

        var document = Assert.Single(_repository.Documents);
        Assert.Equal(SearchSourceTypes.Course, document.SourceType);
        Assert.Equal(courseId, document.SourceId);
        Assert.Equal(1, _unitOfWork.SaveChangesCount);
    }

    [Fact]
    public async Task Draft_course_is_not_indexed()
    {
        _courseSource.Document = null;
        await CreateCoursePublishedHandler()
            .HandleAsync(new CoursePublishedDomainEvent(Guid.NewGuid(), "draft"));

        Assert.Empty(_repository.Documents);
        Assert.Equal(0, _unitOfWork.SaveChangesCount);
    }

    [Fact]
    public async Task CourseUpdated_refreshes_title_slug_url_and_summary()
    {
        var courseId = Guid.NewGuid();
        var publishedAt = new DateTime(2026, 7, 5, 0, 0, 0, DateTimeKind.Utc);
        _repository.Seed(new SearchDocument
        {
            Id = Guid.NewGuid(),
            SourceType = SearchSourceTypes.Course,
            SourceId = courseId,
            Title = "Old Title",
            Slug = "old-slug",
            Summary = "Old summary",
            Url = "/courses/old-slug",
            IsPublished = true,
            SourcePublishedAtUtc = publishedAt,
            SourceUpdatedAtUtc = publishedAt,
            IndexedAtUtc = publishedAt,
            LastEventId = Guid.NewGuid(),
        });

        var updatedAt = new DateTime(2026, 7, 19, 12, 0, 0, DateTimeKind.Utc);
        _courseSource.Document = new SearchSourceDocument(
            courseId,
            SearchSourceTypes.Course,
            "New Title",
            "new-slug",
            "New summary",
            "/courses/new-slug",
            true,
            publishedAt,
            updatedAt);

        var domainEvent = new CourseUpdatedDomainEvent(courseId)
        {
            OccurredAtUtc = updatedAt,
        };

        await CreateCourseUpdatedHandler().HandleAsync(domainEvent);

        var document = Assert.Single(_repository.Documents);
        Assert.Equal("New Title", document.Title);
        Assert.Equal("new-slug", document.Slug);
        Assert.Equal("/courses/new-slug", document.Url);
        Assert.Equal("New summary", document.Summary);
        Assert.DoesNotContain(_repository.Documents, d => d.Slug == "old-slug" || d.Url == "/courses/old-slug");
        Assert.Equal(domainEvent.EventId, document.LastEventId);
        Assert.Equal(1, _unitOfWork.SaveChangesCount);
    }

    [Fact]
    public async Task CourseUpdated_duplicate_EventId_is_noop()
    {
        var courseId = Guid.NewGuid();
        var eventId = Guid.NewGuid();
        var at = new DateTime(2026, 7, 5, 0, 0, 0, DateTimeKind.Utc);
        _repository.Seed(new SearchDocument
        {
            Id = Guid.NewGuid(),
            SourceType = SearchSourceTypes.Course,
            SourceId = courseId,
            Title = "Title",
            Slug = "title",
            Summary = "s",
            Url = "/courses/title",
            IsPublished = true,
            SourceUpdatedAtUtc = at,
            IndexedAtUtc = at,
            LastEventId = eventId,
        });
        _courseSource.Document = new SearchSourceDocument(
            courseId,
            SearchSourceTypes.Course,
            "Changed",
            "changed",
            "s",
            "/courses/changed",
            true,
            at,
            at);

        await CreateCourseUpdatedHandler().HandleAsync(new CourseUpdatedDomainEvent(courseId)
        {
            EventId = eventId,
            OccurredAtUtc = at,
        });

        Assert.Equal("Title", Assert.Single(_repository.Documents).Title);
        Assert.Equal(0, _unitOfWork.SaveChangesCount);
    }

    [Fact]
    public async Task CourseUpdated_older_event_does_not_overwrite_newer_projection()
    {
        var courseId = Guid.NewGuid();
        var newer = new DateTime(2026, 7, 10, 0, 0, 0, DateTimeKind.Utc);
        var older = new DateTime(2026, 7, 1, 0, 0, 0, DateTimeKind.Utc);
        _repository.Seed(new SearchDocument
        {
            Id = Guid.NewGuid(),
            SourceType = SearchSourceTypes.Course,
            SourceId = courseId,
            Title = "Newer",
            Slug = "newer",
            Summary = "s",
            Url = "/courses/newer",
            IsPublished = true,
            SourceUpdatedAtUtc = newer,
            IndexedAtUtc = newer,
            LastEventId = Guid.NewGuid(),
        });
        _courseSource.Document = new SearchSourceDocument(
            courseId,
            SearchSourceTypes.Course,
            "Older",
            "older",
            "s",
            "/courses/older",
            true,
            older,
            older);

        await CreateCourseUpdatedHandler().HandleAsync(new CourseUpdatedDomainEvent(courseId)
        {
            OccurredAtUtc = older,
        });

        Assert.Equal("Newer", Assert.Single(_repository.Documents).Title);
        Assert.Equal(0, _unitOfWork.SaveChangesCount);
    }

    [Fact]
    public async Task CourseUpdated_missing_public_source_removes_existing_projection()
    {
        var courseId = Guid.NewGuid();
        var at = DateTime.UtcNow;
        _repository.Seed(new SearchDocument
        {
            Id = Guid.NewGuid(),
            SourceType = SearchSourceTypes.Course,
            SourceId = courseId,
            Title = "Gone",
            Slug = "gone",
            Summary = "s",
            Url = "/courses/gone",
            IsPublished = true,
            SourceUpdatedAtUtc = at,
            IndexedAtUtc = at,
            LastEventId = Guid.NewGuid(),
        });
        _courseSource.Document = null;

        await CreateCourseUpdatedHandler().HandleAsync(new CourseUpdatedDomainEvent(courseId));

        Assert.Empty(_repository.Documents);
        Assert.Equal(1, _unitOfWork.SaveChangesCount);
    }

    [Fact]
    public async Task CourseUpdated_missing_public_source_with_no_projection_is_successful_noop()
    {
        _courseSource.Document = null;

        await CreateCourseUpdatedHandler().HandleAsync(new CourseUpdatedDomainEvent(Guid.NewGuid()));

        Assert.Empty(_repository.Documents);
        Assert.Equal(0, _unitOfWork.SaveChangesCount);
    }

    [Fact]
    public async Task CourseUpdated_forwards_CancellationToken()
    {
        var courseId = Guid.NewGuid();
        var at = DateTime.UtcNow;
        _courseSource.Document = new SearchSourceDocument(
            courseId,
            SearchSourceTypes.Course,
            "T",
            "t",
            "s",
            "/courses/t",
            true,
            at,
            at);

        using var cts = new CancellationTokenSource();
        var token = cts.Token;
        await CreateCourseUpdatedHandler().HandleAsync(new CourseUpdatedDomainEvent(courseId), token);

        Assert.Equal(token, _courseSource.LastCancellationToken);
        Assert.Equal(token, _repository.LastCancellationToken);
        Assert.Equal(token, _unitOfWork.LastCancellationToken);
    }

    [Fact]
    public async Task Missing_temporary_source_failure_propagates()
    {
        _contentSource.ExceptionToThrow = new InvalidOperationException("source unavailable");
        var handler = new ContentPublishedSearchHandler(_contentSource, CreateProjection());

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            handler.HandleAsync(new ContentPublishedDomainEvent(Guid.NewGuid(), "x")));

        Assert.Equal(0, _unitOfWork.SaveChangesCount);
    }

    [Fact]
    public async Task CancellationToken_is_forwarded()
    {
        var contentId = Guid.NewGuid();
        var at = DateTime.UtcNow;
        _contentSource.Document = new SearchSourceDocument(
            contentId,
            SearchSourceTypes.Content,
            "T",
            "t",
            "s",
            "/content/t",
            true,
            at,
            at);

        using var cts = new CancellationTokenSource();
        var token = cts.Token;
        var handler = new ContentPublishedSearchHandler(_contentSource, CreateProjection());

        await handler.HandleAsync(new ContentPublishedDomainEvent(contentId, "t"), token);

        Assert.Equal(token, _contentSource.LastCancellationToken);
        Assert.Equal(token, _repository.LastCancellationToken);
        Assert.Equal(token, _unitOfWork.LastCancellationToken);
    }

    [Fact]
    public async Task Duplicate_processing_converges_to_same_final_state()
    {
        var contentId = Guid.NewGuid();
        var at = new DateTime(2026, 7, 1, 0, 0, 0, DateTimeKind.Utc);
        _contentSource.Document = new SearchSourceDocument(
            contentId,
            SearchSourceTypes.Content,
            "Title",
            "title",
            "Summary",
            "/content/title",
            true,
            at,
            at);

        var domainEvent = new ContentPublishedDomainEvent(contentId, "title")
        {
            OccurredAtUtc = at,
        };
        var handler = new ContentPublishedSearchHandler(_contentSource, CreateProjection());

        await handler.HandleAsync(domainEvent);
        await handler.HandleAsync(domainEvent);

        Assert.Single(_repository.Documents);
        Assert.Equal(1, _unitOfWork.SaveChangesCount);
    }
}

using HelpDev.Modules.Search.Application.Contracts;
using HelpDev.Modules.Search.Application.Semantic;
using HelpDev.Modules.Search.Domain;
using HelpDev.Modules.Learning.Domain.Courses;
using HelpDev.Modules.PromptLab.Domain.Prompts;
using HelpDev.Modules.Toolbox.Domain.Tools;
using HelpDev.SharedApplication.Abstractions.Events;

namespace HelpDev.Modules.Search.Application.Handlers;

public sealed class CoursePublishedSemanticIndexingHandler : IDomainEventHandler<CoursePublishedDomainEvent>
{
    private readonly ICourseSearchSource _courseSearchSource;
    private readonly ISemanticIndexingService _semanticIndexing;

    public CoursePublishedSemanticIndexingHandler(
        ICourseSearchSource courseSearchSource,
        ISemanticIndexingService semanticIndexing)
    {
        _courseSearchSource = courseSearchSource;
        _semanticIndexing = semanticIndexing;
    }

    public async Task HandleAsync(
        CoursePublishedDomainEvent domainEvent,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(domainEvent);
        var source = await _courseSearchSource.GetByIdAsync(domainEvent.CourseId, cancellationToken);
        await _semanticIndexing.ApplyAsync(
            KnowledgeSourceType.Course,
            domainEvent.CourseId,
            source,
            domainEvent.EventId,
            cancellationToken);
    }
}

public sealed class CourseUpdatedSemanticIndexingHandler : IDomainEventHandler<CourseUpdatedDomainEvent>
{
    private readonly ICourseSearchSource _courseSearchSource;
    private readonly ISemanticIndexingService _semanticIndexing;

    public CourseUpdatedSemanticIndexingHandler(
        ICourseSearchSource courseSearchSource,
        ISemanticIndexingService semanticIndexing)
    {
        _courseSearchSource = courseSearchSource;
        _semanticIndexing = semanticIndexing;
    }

    public async Task HandleAsync(
        CourseUpdatedDomainEvent domainEvent,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(domainEvent);
        var source = await _courseSearchSource.GetByIdAsync(domainEvent.CourseId, cancellationToken);
        await _semanticIndexing.ApplyAsync(
            KnowledgeSourceType.Course,
            domainEvent.CourseId,
            source,
            domainEvent.EventId,
            cancellationToken);
    }
}

public sealed class LessonPublishedSemanticIndexingHandler : IDomainEventHandler<LessonPublishedDomainEvent>
{
    private readonly ILessonSearchSource _lessonSearchSource;
    private readonly ISemanticIndexingService _semanticIndexing;

    public LessonPublishedSemanticIndexingHandler(
        ILessonSearchSource lessonSearchSource,
        ISemanticIndexingService semanticIndexing)
    {
        _lessonSearchSource = lessonSearchSource;
        _semanticIndexing = semanticIndexing;
    }

    public async Task HandleAsync(
        LessonPublishedDomainEvent domainEvent,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(domainEvent);
        var source = await _lessonSearchSource.GetByIdAsync(domainEvent.LessonId, cancellationToken);
        await _semanticIndexing.ApplyAsync(
            KnowledgeSourceType.Lesson,
            domainEvent.LessonId,
            source,
            domainEvent.EventId,
            cancellationToken);
    }
}

public sealed class ToolPublishedSemanticIndexingHandler : IDomainEventHandler<ToolPublishedDomainEvent>
{
    private readonly IToolSearchSource _toolSearchSource;
    private readonly ISemanticIndexingService _semanticIndexing;

    public ToolPublishedSemanticIndexingHandler(
        IToolSearchSource toolSearchSource,
        ISemanticIndexingService semanticIndexing)
    {
        _toolSearchSource = toolSearchSource;
        _semanticIndexing = semanticIndexing;
    }

    public async Task HandleAsync(
        ToolPublishedDomainEvent domainEvent,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(domainEvent);
        var source = await _toolSearchSource.GetByIdAsync(domainEvent.ToolId, cancellationToken);
        await _semanticIndexing.ApplyAsync(
            KnowledgeSourceType.Tool,
            domainEvent.ToolId,
            source,
            domainEvent.EventId,
            cancellationToken);
    }
}

public sealed class ToolUnpublishedSemanticIndexingHandler : IDomainEventHandler<ToolUnpublishedDomainEvent>
{
    private readonly ISemanticIndexingService _semanticIndexing;

    public ToolUnpublishedSemanticIndexingHandler(ISemanticIndexingService semanticIndexing)
    {
        _semanticIndexing = semanticIndexing;
    }

    public async Task HandleAsync(
        ToolUnpublishedDomainEvent domainEvent,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(domainEvent);
        await _semanticIndexing.ApplyAsync(
            KnowledgeSourceType.Tool,
            domainEvent.ToolId,
            source: null,
            domainEvent.EventId,
            cancellationToken);
    }
}

public sealed class PromptPublishedSemanticIndexingHandler : IDomainEventHandler<PromptPublishedDomainEvent>
{
    private readonly IPromptSearchSource _promptSearchSource;
    private readonly ISemanticIndexingService _semanticIndexing;

    public PromptPublishedSemanticIndexingHandler(
        IPromptSearchSource promptSearchSource,
        ISemanticIndexingService semanticIndexing)
    {
        _promptSearchSource = promptSearchSource;
        _semanticIndexing = semanticIndexing;
    }

    public async Task HandleAsync(
        PromptPublishedDomainEvent domainEvent,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(domainEvent);
        var source = await _promptSearchSource.GetByIdAsync(domainEvent.PromptId, cancellationToken);
        await _semanticIndexing.ApplyAsync(
            KnowledgeSourceType.Prompt,
            domainEvent.PromptId,
            source,
            domainEvent.EventId,
            cancellationToken);
    }
}

public sealed class PromptUnpublishedSemanticIndexingHandler : IDomainEventHandler<PromptUnpublishedDomainEvent>
{
    private readonly ISemanticIndexingService _semanticIndexing;

    public PromptUnpublishedSemanticIndexingHandler(ISemanticIndexingService semanticIndexing)
    {
        _semanticIndexing = semanticIndexing;
    }

    public async Task HandleAsync(
        PromptUnpublishedDomainEvent domainEvent,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(domainEvent);
        await _semanticIndexing.ApplyAsync(
            KnowledgeSourceType.Prompt,
            domainEvent.PromptId,
            source: null,
            domainEvent.EventId,
            cancellationToken);
    }
}

using HelpDev.Modules.Content.Domain.Events;
using HelpDev.Modules.Search.Application.Contracts;
using HelpDev.Modules.Search.Application.Indexing;
using HelpDev.Modules.Search.Domain;
using HelpDev.SharedApplication.Abstractions.Events;

namespace HelpDev.Modules.Search.Application.Handlers;

public sealed class ContentPublishedSearchHandler : IDomainEventHandler<ContentPublishedDomainEvent>
{
    private readonly IContentSearchSource _contentSearchSource;
    private readonly ISearchProjectionService _projectionService;

    public ContentPublishedSearchHandler(
        IContentSearchSource contentSearchSource,
        ISearchProjectionService projectionService)
    {
        _contentSearchSource = contentSearchSource;
        _projectionService = projectionService;
    }

    public async Task HandleAsync(
        ContentPublishedDomainEvent domainEvent,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(domainEvent);

        var source = await _contentSearchSource.GetByIdAsync(domainEvent.ContentId, cancellationToken);
        await _projectionService.ApplyAsync(
            SearchSourceTypes.Content,
            domainEvent.ContentId,
            source,
            domainEvent.EventId,
            domainEvent.OccurredAtUtc,
            cancellationToken);
    }
}

public sealed class ContentUpdatedSearchHandler : IDomainEventHandler<ContentUpdatedDomainEvent>
{
    private readonly IContentSearchSource _contentSearchSource;
    private readonly ISearchProjectionService _projectionService;

    public ContentUpdatedSearchHandler(
        IContentSearchSource contentSearchSource,
        ISearchProjectionService projectionService)
    {
        _contentSearchSource = contentSearchSource;
        _projectionService = projectionService;
    }

    public async Task HandleAsync(
        ContentUpdatedDomainEvent domainEvent,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(domainEvent);

        // Re-read source so Draft updates remove/never create public projections.
        var source = await _contentSearchSource.GetByIdAsync(domainEvent.ContentId, cancellationToken);
        await _projectionService.ApplyAsync(
            SearchSourceTypes.Content,
            domainEvent.ContentId,
            source,
            domainEvent.EventId,
            domainEvent.OccurredAtUtc,
            cancellationToken);
    }
}

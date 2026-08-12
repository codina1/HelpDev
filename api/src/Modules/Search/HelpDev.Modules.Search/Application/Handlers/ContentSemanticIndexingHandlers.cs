using HelpDev.Modules.Search.Application.Contracts;
using HelpDev.Modules.Search.Application.Semantic;
using HelpDev.Modules.Search.Domain;
using HelpDev.Modules.Content.Domain.Events;
using HelpDev.SharedApplication.Abstractions.Events;

namespace HelpDev.Modules.Search.Application.Handlers;

public sealed class ContentPublishedSemanticIndexingHandler : IDomainEventHandler<ContentPublishedDomainEvent>
{
    private readonly IContentSearchSource _contentSearchSource;
    private readonly ISemanticIndexingService _semanticIndexing;

    public ContentPublishedSemanticIndexingHandler(
        IContentSearchSource contentSearchSource,
        ISemanticIndexingService semanticIndexing)
    {
        _contentSearchSource = contentSearchSource;
        _semanticIndexing = semanticIndexing;
    }

    public async Task HandleAsync(
        ContentPublishedDomainEvent domainEvent,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(domainEvent);
        var source = await _contentSearchSource.GetByIdAsync(domainEvent.ContentId, cancellationToken);
        await _semanticIndexing.ApplyAsync(
            SearchSourceTypes.Content,
            domainEvent.ContentId,
            source,
            domainEvent.EventId,
            cancellationToken);
    }
}

public sealed class ContentUpdatedSemanticIndexingHandler : IDomainEventHandler<ContentUpdatedDomainEvent>
{
    private readonly IContentSearchSource _contentSearchSource;
    private readonly ISemanticIndexingService _semanticIndexing;

    public ContentUpdatedSemanticIndexingHandler(
        IContentSearchSource contentSearchSource,
        ISemanticIndexingService semanticIndexing)
    {
        _contentSearchSource = contentSearchSource;
        _semanticIndexing = semanticIndexing;
    }

    public async Task HandleAsync(
        ContentUpdatedDomainEvent domainEvent,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(domainEvent);
        var source = await _contentSearchSource.GetByIdAsync(domainEvent.ContentId, cancellationToken);
        await _semanticIndexing.ApplyAsync(
            SearchSourceTypes.Content,
            domainEvent.ContentId,
            source,
            domainEvent.EventId,
            cancellationToken);
    }
}

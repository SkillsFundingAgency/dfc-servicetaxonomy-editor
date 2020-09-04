using System.Threading.Tasks;
using DFC.ServiceTaxonomy.Events.Configuration;
using DFC.ServiceTaxonomy.Events.Models;
using DFC.ServiceTaxonomy.Events.Services.Interfaces;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.ContentItemVersions;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Helpers;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Results.AllowSync;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.DisplayManagement.Notify;

namespace DFC.ServiceTaxonomy.GraphSync.Handlers.Orchestrators
{
    public class Orchestrator
    {
        protected readonly IContentDefinitionManager _contentDefinitionManager;
        protected readonly INotifier _notifier;
        private readonly ILogger _logger;
        private readonly IOptionsMonitor<EventGridConfiguration> _eventGridConfiguration;
        private readonly IEventGridContentClient _eventGridContentClient;
        private readonly ISyncNameProvider _syncNameProvider;
        private readonly IPublishedContentItemVersion _publishedContentItemVersion;
        private readonly IPreviewContentItemVersion _previewContentItemVersion;
        private readonly INeutralEventContentItemVersion _neutralEventContentItemVersion;

        protected Orchestrator(
            IContentDefinitionManager contentDefinitionManager,
            INotifier notifier,
            ILogger logger,
            IOptionsMonitor<EventGridConfiguration> eventGridConfiguration,
            IEventGridContentClient eventGridContentClient,
            ISyncNameProvider syncNameProvider,
            IPublishedContentItemVersion publishedContentItemVersion,
            IPreviewContentItemVersion previewContentItemVersion,
            INeutralEventContentItemVersion neutralEventContentItemVersion)
        {
            _contentDefinitionManager = contentDefinitionManager;
            _notifier = notifier;
            _logger = logger;
            _eventGridConfiguration = eventGridConfiguration;
            _eventGridContentClient = eventGridContentClient;
            _syncNameProvider = syncNameProvider;
            _publishedContentItemVersion = publishedContentItemVersion;
            _previewContentItemVersion = previewContentItemVersion;
            _neutralEventContentItemVersion = neutralEventContentItemVersion;
        }

#pragma warning disable S1172
        protected void AddBlockedNotifier(
            string operationDescription,
            string graphReplicaSetName,
            IAllowSyncResult allowSyncResult,
            ContentItem contentItem)
        {
            //string contentType = GetContentTypeDisplayName(contentItem);

            _logger.LogWarning("{OperationDescription} the {GraphReplicaSetName} graphs has been cancelled. These items relate: {AllowSyncResult}.",
                operationDescription, graphReplicaSetName, allowSyncResult);

            //todo: need details of the content item with incoming relationships
            _notifier.Add(NotifyType.Error, new LocalizedHtmlString(nameof(GraphSyncContentHandler),
                $"{operationDescription} the {graphReplicaSetName} graphs has been cancelled. These items relate: {allowSyncResult}."));
        }
#pragma warning restore S1172

        protected void AddFailureNotifier(ContentItem contentItem)
        {
            string contentType = GetContentTypeDisplayName(contentItem);

            _notifier.Add(NotifyType.Error, new LocalizedHtmlString(nameof(GraphSyncContentHandler),
                $"The '{contentItem.DisplayText}' {contentType} could not be removed because the associated node could not be deleted from the graph."));
        }

        //todo: todo temporarily protected
        protected string GetContentTypeDisplayName(ContentItem contentItem)
        {
            return _contentDefinitionManager.GetTypeDefinition(contentItem.ContentType).DisplayName;
        }

        protected async Task PublishContentEvent(
            ContentItem contentItem,
            ContentEventType eventType)
        {
            if (!_eventGridConfiguration.CurrentValue.PublishEvents)
            {
                _logger.LogInformation("Event grid publishing is disabled. No events will be published.");
                return;
            }

            IContentItemVersion contentItemVersion = eventType switch
            {
                ContentEventType.Published => _publishedContentItemVersion,
                ContentEventType.Draft => _previewContentItemVersion,
                _ => _neutralEventContentItemVersion
            };

            string userId = _syncNameProvider.GetIdPropertyValue(contentItem.Content.GraphSyncPart, contentItemVersion);

            ContentEvent contentEvent = new ContentEvent(contentItem, userId, eventType);
            await _eventGridContentClient.Publish(contentEvent);
        }
    }
}

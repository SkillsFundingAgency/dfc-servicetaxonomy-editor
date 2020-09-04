using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Results.AllowSync;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.Logging;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.DisplayManagement.Notify;

namespace DFC.ServiceTaxonomy.GraphSync.Handlers.Orchestrators
{
    public class Orchestrator
    {
        protected readonly IContentDefinitionManager _contentDefinitionManager;
        protected readonly INotifier _notifier;
        protected readonly ILogger _logger;

        protected Orchestrator(
            IContentDefinitionManager contentDefinitionManager,
            INotifier notifier,
            ILogger logger)
        {
            _contentDefinitionManager = contentDefinitionManager;
            _notifier = notifier;
            _logger = logger;
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
    }
}

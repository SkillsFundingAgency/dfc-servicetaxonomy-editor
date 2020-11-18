using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.ContentItemVersions;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Results.AllowSync;
using DFC.ServiceTaxonomy.GraphSync.Handlers.Contexts;
using DFC.ServiceTaxonomy.GraphSync.Handlers.Interfaces;
using DFC.ServiceTaxonomy.GraphSync.Notifications;
using DFC.ServiceTaxonomy.GraphSync.Services;
using DFC.ServiceTaxonomy.Neo4j.Exceptions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata;

namespace DFC.ServiceTaxonomy.GraphSync.Orchestrators
{
    public class Orchestrator
    {
        protected readonly IContentDefinitionManager _contentDefinitionManager;
        protected readonly IGraphSyncNotifier _notifier;
        protected readonly IServiceProvider _serviceProvider;
        protected readonly ILogger _logger;
        private readonly IEnumerable<IContentOrchestrationHandler> _contentOrchestrationHandlers;

        protected Orchestrator(
            IContentDefinitionManager contentDefinitionManager,
            IGraphSyncNotifier notifier,
            IServiceProvider serviceProvider,
            IEnumerable<IContentOrchestrationHandler> contentOrchestrationHandlers,
            ILogger logger)
        {
            _contentDefinitionManager = contentDefinitionManager;
            _notifier = notifier;
            _serviceProvider = serviceProvider;
            _contentOrchestrationHandlers = contentOrchestrationHandlers;
            _logger = logger;
        }

        protected async Task CallContentOrchestrationHandlers(
            ContentItem contentItem,
            Func<IContentOrchestrationHandler, IOrchestrationContext, Task> callHandlerWhenAllowed)
        {
            var context = new OrchestrationContext(contentItem, _notifier);

            foreach (var contentOrchestrationHandler in _contentOrchestrationHandlers)
            {
                await callHandlerWhenAllowed(contentOrchestrationHandler, context);

                if (context.Cancel)
                {
                    //todo: implement if/when we need it
                }
            }
        }

        //todo: temporarily protected
        protected string GetContentTypeDisplayName(ContentItem contentItem)
        {
            return _contentDefinitionManager.GetTypeDefinition(contentItem.ContentType).DisplayName;
        }

        protected string GetSyncOperationCancelledUserMessage(
            SyncOperation syncOperation,
            string displayText,
            string contentType)
        {
            return $"{syncOperation} the '{displayText}' {contentType} has been cancelled, due to an issue with graph syncing.";
        }

        protected async Task<(IAllowSync, IDeleteGraphSyncer?)> GetDeleteGraphSyncerIfDeleteAllowed(
            ContentItem contentItem,
            IContentItemVersion contentItemVersion,
            SyncOperation syncOperation)
        {
            try
            {
                IDeleteGraphSyncer deleteGraphSyncer = _serviceProvider.GetRequiredService<IDeleteGraphSyncer>();

                IAllowSync allowSync = await deleteGraphSyncer.DeleteAllowed(
                    contentItem,
                    contentItemVersion,
                    syncOperation);

                return (allowSync, deleteGraphSyncer);
            }
            catch (Exception exception)
            {
                string contentType = GetContentTypeDisplayName(contentItem);

                //todo: will get logged twice, but want to keep the param version
                _logger.LogError(exception, "Unable to check if the '{ContentItem}' {ContentType} can be {DeleteOperation} from the {GraphReplicaSetName} graph.",
                    contentItem.DisplayText, contentType, syncOperation.ToString("PrP", null).ToLower(), contentItemVersion.GraphReplicaSetName);

                await _notifier.Add(GetSyncOperationCancelledUserMessage(syncOperation, contentItem.DisplayText, contentType),
                    $"Unable to check if the '{contentItem.DisplayText}' {contentType} can be {syncOperation.ToString("PrP", null).ToLower()} from the {contentItemVersion.GraphReplicaSetName} graph.",
                    exception: exception);

                throw;
            }
        }

        protected async Task DeleteFromGraphReplicaSet(
            IDeleteGraphSyncer deleteGraphSyncer,
            ContentItem contentItem,
            SyncOperation syncOperation)
        {
            try
            {
                await deleteGraphSyncer.Delete();
            }
            catch (CommandValidationException ex)
            {
                // don't fail when node was not found in the graph
                // at the moment, we only add published items to the graph,
                // so if you try to delete a draft only item, this task fails and the item isn't deleted
                //todo: if this check is needed after the published/draft work, don't rely on the message!
                if (ex.Message == "Expecting 1 node to be deleted, but 0 were actually deleted.")
                {
                    return;
                }

                AddFailureNotifier(deleteGraphSyncer, contentItem, ex, syncOperation);
                throw;
            }
            catch (Exception ex)
            {
                AddFailureNotifier(deleteGraphSyncer, contentItem, ex, syncOperation);
                throw;
            }
        }

        private void AddFailureNotifier(IDeleteGraphSyncer deleteGraphSyncer,
            ContentItem contentItem,
            Exception exception,
            SyncOperation syncOperation)
        {
            string contentType = GetContentTypeDisplayName(contentItem);

            string operation = syncOperation.ToString("PrP", null);

            _logger.LogError(exception, "{Operation} the '{ContentItem}' {ContentType} has been cancelled because the {GraphReplicaSetName} graph couldn't be updated.",
                operation, contentItem.DisplayText, contentType, deleteGraphSyncer.GraphReplicaSetName);

            _notifier.Add(GetSyncOperationCancelledUserMessage(syncOperation, contentItem.DisplayText, contentType),
                $"{operation} the '{contentItem.DisplayText}' {contentType} has been cancelled because the {deleteGraphSyncer.GraphReplicaSetName} graph couldn't be updated.",
                exception: exception);
        }
    }
}

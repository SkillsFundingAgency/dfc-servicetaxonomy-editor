using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.Handlers.Contexts;
using DFC.ServiceTaxonomy.GraphSync.Handlers.Interfaces;
using DFC.ServiceTaxonomy.GraphSync.Notifications;
using DFC.ServiceTaxonomy.GraphSync.Services;
using Microsoft.Extensions.Logging;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata;

namespace DFC.ServiceTaxonomy.GraphSync.Orchestrators
{
    public class Orchestrator
    {
        protected readonly IContentDefinitionManager _contentDefinitionManager;
        protected readonly IGraphSyncNotifier _notifier;
        protected readonly ILogger _logger;
        private readonly IEnumerable<IContentOrchestrationHandler> _contentOrchestrationHandlers;

        protected Orchestrator(
            IContentDefinitionManager contentDefinitionManager,
            IGraphSyncNotifier notifier,
            IEnumerable<IContentOrchestrationHandler> contentOrchestrationHandlers,
            ILogger logger)
        {
            _contentDefinitionManager = contentDefinitionManager;
            _notifier = notifier;
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
                try
                {
                    await callHandlerWhenAllowed(contentOrchestrationHandler, context);

                    if (context.Cancel)
                    {
                        //todo:
                    }
                }
                catch (Exception exception)
                {
                    //todo:
                    _logger.LogError(exception, "Content orchestration handler threw exception.");
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
    }
}
